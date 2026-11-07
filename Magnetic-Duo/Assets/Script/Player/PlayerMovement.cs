using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [Header("바닥 감지")]
    [SerializeField] private Transform groundCheck;    // 바닥 감지 위치
    [SerializeField] private float groundCheckRadius = 0.2f;    // 바닥 감지 반경
    [SerializeField] private LayerMask groundLayer;    // 바닥 레이어
    private Rigidbody2D rb;     
    private float currentMoveInput = 0f; // 현재 입력 저장
    public float CurrentMoveInput => currentMoveInput;
    public float MoveSpeed => moveSpeed;
    public bool IsGrounded { get; private set; }    // 바닥 감지 여부

    private int activeEffectorCount = 0;

    // 맵이 열려 있는 동안 정지 (공중에서도 멈춤, 밟고 있는 발판·상자가 움직이면 함께 이동)
    public bool IsFrozen { get; private set; }
    private const float FrozenMass = 1000f;    // 공중·고정된 바닥에서 상자에 밀리지 않도록
    private Vector2 velocityBeforeFreeze;
    private float massBeforeFreeze;
    private Transform followTarget;    // 정지 중에 따라 움직일, 발 바로 아래의 발판·상자·봇
    private Vector2 followOffset;
    private Collider2D bodyCollider;
    private readonly Collider2D[] groundHits = new Collider2D[8];

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();

        if (rb != null)
        {
            // 질량을 정상으로 복구 (박스를 뚫고 내려가지 않게 함)
            rb.mass = 1f; 
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void FixedUpdate()
    {
        if (rb.bodyType == RigidbodyType2D.Static) return;

        if (IsFrozen)
        {
            if (activeEffectorCount > 0)
            {
                // 컨베이어 위에서는 평소 가만히 서 있을 때처럼 벨트 방향으로 실려 감
                rb.mass = massBeforeFreeze;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                return;
            }

            // 가로는 고정, 세로는 중력만 상쇄 → 공중에서도 그 자리에 멈춤
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = Vector2.zero;

            // 밟고 있는 발판·상자가 움직이면 간격을 유지한 채 같이 이동 (공중에 남거나 아래 상자를 누르지 않도록)
            if (followTarget == null) FindFollowTarget();
            if (followTarget != null)
            {
                Vector2 target = (Vector2)followTarget.position + followOffset;
                if ((target - rb.position).sqrMagnitude > 1f) followTarget = null;    // 리스폰 등으로 순간이동하면 따라가지 않음
                else rb.position = target;
            }

            // 따라가는 중에는 원래 질량 (무거우면 아래 상자를 발판 속으로 눌러버림)
            rb.mass = followTarget != null ? massBeforeFreeze : FrozenMass;
            rb.AddForce(-Physics2D.gravity * rb.gravityScale * rb.mass);
            return;
        }

        bool isOnEffector  = (activeEffectorCount > 0);
        if (currentMoveInput != 0)
        {
            // 이동 중일 때는 X축 잠금을 풀고 속도 적용
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = new Vector2(currentMoveInput * moveSpeed, rb.linearVelocity.y);
        }
        else if(isOnEffector)
        {
            // 컨베이어 벨트 위에 가만히 서 있을 때 움직이도록
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            // 가만히 있을 때는 X축을 완전히 잠가서 어떤 충돌에도 밀리지 않게 함
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // 이동
    public void Move(float direction)
    {
        currentMoveInput = direction;
    }

    // 즉시 정지
    public void StopMovement()
    {
        currentMoveInput = 0f;
    }

    // 맵 열기/닫기 시 정지·해제 (해제하면 멈추기 직전 속도로 이어서 움직임)
    public void SetFrozen(bool frozen)
    {
        if (IsFrozen == frozen) return;
        IsFrozen = frozen;

        if (frozen)
        {
            velocityBeforeFreeze = rb.linearVelocity;
            massBeforeFreeze = rb.mass;
            rb.linearVelocity = Vector2.zero;
            FindFollowTarget();
            return;
        }

        followTarget = null;
        rb.mass = massBeforeFreeze;
        if (rb.bodyType == RigidbodyType2D.Dynamic)
            rb.linearVelocity = velocityBeforeFreeze;
    }

    // 발 바로 아래의 움직일 수 있는 것(체인 발판·자성 발판·상자·다른 봇)을 따라갈 대상으로 정함
    // 점프 판정과 같은 바닥 감지 범위로 찾음 → 내려가는 발판처럼 살짝 떠 있어도 찾음
    private void FindFollowTarget()
    {
        Vector2 feet = groundCheck.position;
        float halfWidth = bodyCollider.bounds.extents.x * 0.9f;
        ContactFilter2D filter = new ContactFilter2D { useLayerMask = true, layerMask = groundLayer };
        int count = Physics2D.OverlapCircle(feet, groundCheckRadius, filter, groundHits);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = groundHits[i];

            // 발 바로 아래만 (옆에 붙어 있는 상자 등은 제외)
            Vector2 closest = hit.ClosestPoint(feet);
            if (closest.y > feet.y + 0.05f || Mathf.Abs(closest.x - feet.x) > halfWidth) continue;

            // 고정된 바닥은 따라갈 필요 없음
            bool canMove = hit.GetComponentInParent<MovingPlatform>() != null
                        || (hit.attachedRigidbody != null && hit.attachedRigidbody.bodyType != RigidbodyType2D.Static);
            if (!canMove) continue;

            followTarget = hit.transform;
            followOffset = rb.position - (Vector2)followTarget.position;
            return;
        }
    }

    // 점프 
    public void Jump()
    {
        if(IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // --- [발바닥 센서: 이펙터 감지] ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.usedByEffector) 
        {
            activeEffectorCount++;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.usedByEffector) 
        {
            activeEffectorCount--;
        }
    }
}
