using UnityEngine;

public class PolarityWall : MonoBehaviour
{
    [Header("벽 설정")]
    [SerializeField] private bool isOn = true;
    [Header("시각효과")]
    [Tooltip("체크하면 꺼졌을 때 아예 투명해집니다. 해제하면 반투명으로 남습니다.")]
    [SerializeField] private bool hideCompletelyWhenOff = true;

    private Collider2D wallCollider;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        wallCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if(wallCollider != null)
        {
            wallCollider.isTrigger = true;
        }
    }
    void Start()
    {
        UpdateWallState();
    }

    public void ToggleWall()
    {
        isOn = !isOn;
        UpdateWallState();
    }

    private void UpdateWallState()
    {
        if(wallCollider != null) wallCollider.enabled = isOn;

        if(spriteRenderer != null)
        {
            if(hideCompletelyWhenOff)
            {
                spriteRenderer.enabled = isOn;
            }
            else
            {
                spriteRenderer.enabled = true;
                Color c = spriteRenderer.color;
                c.a = isOn ? 1f : 0.2f; 
                spriteRenderer.color = c;
            }
        }
    }

}
