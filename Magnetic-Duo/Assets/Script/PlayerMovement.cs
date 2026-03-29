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
    [SerializeField] private Transform groundCheck;    // 바닥 감지 원의 중심(발밑 빈 오브젝트)
    [SerializeField] private float groundCheckRadius = 0.2f;    // 감지 반경
    [SerializeField] private LayerMask groundLayer;    // 바닥으로 취급할 레이어

    private Rigidbody2D rb;
    // PlayerAnimation에서 참조 — 매 프레임 groundCheck 위치에서 원 검사로 갱신
    public bool IsGrounded { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // direction: -1 ~ 1. 수직 속도는 유지(점프 중 하강 등 유지)
    public void Move(float direction)
    {
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
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
