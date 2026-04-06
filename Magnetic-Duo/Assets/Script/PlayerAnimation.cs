using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("연결할 컴포넌트")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerInput playerInput;

    private Rigidbody2D rb;

    void Awake()
    {
        if(animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        if(spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        if(movement == null)
        {
            movement = GetComponent<PlayerMovement>();
        }
        if(playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(animator != null && spriteRenderer != null && movement != null && rb != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("IsGrounded", movement.IsGrounded);
            animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);

            if(rb.linearVelocity.x > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
            else if(rb.linearVelocity.x < -0.1f)
            {
                spriteRenderer.flipX = true;
            }

            if(playerInput != null)
            {
                animator.SetBool("IsActive", playerInput.enabled);
            }
        }
    }
    
}
