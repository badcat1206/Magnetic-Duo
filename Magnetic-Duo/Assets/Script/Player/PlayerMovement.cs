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

    // 맵이 열려 있는 동안 정지 (공중에서도 멈춤, 체인 발판 위면 발판에 붙어 함께 이동)
    public bool IsFrozen { get; private set; }
    private const float FrozenMass = 1000f;    // 상자에 밀리지 않도록 (발판은 그대로 밀어 올림)
    private Vector2 velocityBeforeFreeze;
    private float massBeforeFreeze;
    private Transform parentBeforeFreeze;
    private MovingPlatform attachedPlatform;
    private readonly Collider2D[] groundHits = new Collider2D[8];

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

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

            // 가로는 고정, 세로는 중력만 상쇄 → 공중에서도 멈추고 올라오는 발판에는 밀려 올라감
            rb.mass = FrozenMass;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(-Physics2D.gravity * rb.gravityScale * rb.mass);
            AttachToPlatformBelow();
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
            rb.mass = FrozenMass;
            rb.linearVelocity = Vector2.zero;
            AttachToPlatformBelow();
            return;
        }

        // 정지 중에 붙인 체인 발판에서 떼어냄 (그사이 다른 발판이 부모를 바꿨다면 건드리지 않음)
        if (attachedPlatform != null && transform.parent == attachedPlatform.transform)
            transform.SetParent(parentBeforeFreeze);
        attachedPlatform = null;

        rb.mass = massBeforeFreeze;
        if (rb.bodyType == RigidbodyType2D.Dynamic)
            rb.linearVelocity = velocityBeforeFreeze;
    }

    // 발 아래의 체인 발판(MovingPlatform)에 붙여서 발판이 움직이는 대로 함께 이동
    // 점프 판정과 같은 바닥 감지 범위로 찾음 → 발판이 내려가는 중이라 살짝 떠 있어도 붙음
    // (셔틀 발판은 원래 봇을 자식으로 태우므로 따로 처리하지 않음)
    private void AttachToPlatformBelow()
    {
        if (attachedPlatform != null) return;

        ContactFilter2D filter = new ContactFilter2D { useLayerMask = true, layerMask = groundLayer };
        int count = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, filter, groundHits);
        for (int i = 0; i < count; i++)
        {
            MovingPlatform platform = groundHits[i].GetComponentInParent<MovingPlatform>();
            if (platform == null) continue;

            attachedPlatform = platform;
            parentBeforeFreeze = transform.parent;
            transform.SetParent(platform.transform);
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
