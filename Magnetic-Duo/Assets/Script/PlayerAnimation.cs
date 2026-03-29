using UnityEngine;

/// <summary>
/// 이동·지면 상태에 맞춰 Animator 파라미터를 갱신하고, 속도 방향으로 스프라이트 좌우 반전합니다.
/// </summary>
public class PlayerAnimation : MonoBehaviour
{
    [Header("애니메이션 객체 연결")]
    [SerializeField] private Animator anim;      // Animator 연결
    [SerializeField] private SpriteRenderer sprite;  // 좌우 flip 대상

    private Rigidbody2D rb;
    private PlayerMovement movement;  // 지면 여부(IsGrounded) 참조

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // A/D 누르면 걷기, 정지에 쓰는 Speed (0~1)
        float currentSpeed = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) ? 1f : 0f;
        anim.SetFloat("Speed", currentSpeed);
        anim.SetBool("IsGrounded", movement.IsGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);

        // 실제 이동 속도로 스프라이트 방향만 반전 (입력이 아닌 물리 결과 기준)
        if(rb.linearVelocity.x > 0.1f)
        {
            sprite.flipX = false;
        }
        else if(rb.linearVelocity.x < -0.1f)
        {
            sprite.flipX = true;
        }
    }

    // 캐릭터 전원 ON/OFF 등 — 애니메이터의 IsActive와 연동 (CharacterManager에서 호출)
    public void SetPowerState(bool isActive)
    {
        anim.SetBool("IsActive", isActive);
    }

    // 외부에서 클립 직접 재생 (CharacterManager)
    public Animator Anim { get { return anim; } }
}
