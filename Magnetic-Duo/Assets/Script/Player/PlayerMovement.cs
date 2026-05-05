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
    public bool IsGrounded { get; private set; }    // 바닥 감지 여부

    private int activeEffectorCount = 0;
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
