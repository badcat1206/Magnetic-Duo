using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("애니메이션 객체 연결")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer sprite;

    private Rigidbody2D rb;
    private PlayerMovement movement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        float currentSpeed = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) ? 1f : 0f;
        anim.SetFloat("Speed", currentSpeed);
        anim.SetBool("IsGrounded", movement.IsGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);

        if(rb.linearVelocity.x > 0.1f)
        {
            sprite.flipX = false;
        }
        else if(rb.linearVelocity.x < -0.1f)
        {
            sprite.flipX = true;
        }
    }

    public void SetPowerState(bool isActive)
    {
        anim.SetBool("IsActive", isActive);
    }

    public Animator Anim { get { return anim; } }
}
