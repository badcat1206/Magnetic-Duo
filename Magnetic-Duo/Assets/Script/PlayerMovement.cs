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
    public bool IsGrounded { get; private set; }    // 바닥 감지 여부
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
        if (currentMoveInput != 0)
        {
            // 이동 중일 때는 X축 잠금을 풀고 속도 적용
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = new Vector2(currentMoveInput * moveSpeed, rb.linearVelocity.y);
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
}
