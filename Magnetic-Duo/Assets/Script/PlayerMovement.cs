using UnityEngine;

/// <summary>
/// Rigidbody2D로 좌우 이동·점프를 처리하고, groundCheck 원으로 지면 접촉을 검사합니다.
/// PlayerInput이 Move/Jump를 호출합니다.
/// </summary>
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
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float currentMoveInput = 0f; // 현재 입력 저장
    public bool IsGrounded { get; private set; }    // 바닥 감지 여부
    [SerializeField] private Transform groundCheck;    // 바닥 감지 원의 중심(발밑 빈 오브젝트)
    [SerializeField] private float groundCheckRadius = 0.2f;    // 감지 반경
    [SerializeField] private LayerMask groundLayer;    // 바닥으로 취급할 레이어

    private Rigidbody2D rb;
    // PlayerAnimation에서 참조 — 매 프레임 groundCheck 위치에서 원 검사로 갱신
    public bool IsGrounded { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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
        
        // 애니메이터 파라미터 업데이트
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("IsGrounded", IsGrounded);
            animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        }
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

    // direction: -1 ~ 1. 수직 속도는 유지(점프 중 하강 등 유지)
    public void Move(float direction)
    {
        currentMoveInput = direction;

        // 캐릭터 방향 전환 (Flip)
        if (direction > 0)
            spriteRenderer.flipX = false;
        else if (direction < 0)
            spriteRenderer.flipX = true;
    }

    // 바닥에 닿았을 때만 위로 속도 부여 (공중 추가 점프 방지)
    public void Jump()
    {
        if(IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
}
