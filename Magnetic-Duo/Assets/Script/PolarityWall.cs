using UnityEngine;

public class PolarityWall : MonoBehaviour
{
    [Header("벽 설정")]
    [SerializeField] private bool isOn = true;
    [Header("시각효과")]
    [Tooltip("체크하면 꺼졌을 때 아예 투명해집니다. 해제하면 반투명으로 남습니다.")]
    [SerializeField] private bool hideCompletelyWhenOff = true;

    [Header("버튼으로 강제 비활성화 (누르는 동안 OFF)")]
    [SerializeField] private PressureButton[] disableButtons;
    private bool buttonForced = false;

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

    private void Update()
    {
        bool anyPressed = false;
        if (disableButtons != null)
            foreach (var b in disableButtons)
                if (b != null && b.IsPressed) { anyPressed = true; break; }

        if (anyPressed == buttonForced) return;
        buttonForced = anyPressed;
        UpdateWallState();
    }

    public void ToggleWall()
    {
        isOn = !isOn;
        UpdateWallState();
    }

    private void UpdateWallState()
    {
        bool active = isOn && !buttonForced;

        if(wallCollider != null) wallCollider.enabled = active;

        if(spriteRenderer != null)
        {
            if(hideCompletelyWhenOff)
            {
                spriteRenderer.enabled = active;
            }
            else
            {
                spriteRenderer.enabled = true;
                Color c = spriteRenderer.color;
                c.a = active ? 1f : 0.2f;
                spriteRenderer.color = c;
            }
        }
    }

}
