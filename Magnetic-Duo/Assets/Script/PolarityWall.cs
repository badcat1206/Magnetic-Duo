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

    [Header("레버로 강제 비활성화 (레버가 켜져있는 동안 OFF)")]
    [SerializeField] private Lever[] disableLevers;

    [Header("오디오")]
    [Tooltip("켜질 때와 꺼질 때 공통으로 재생할 소리입니다.")]
    [SerializeField] private AudioClip toggleClip;

    private bool forced = false;

    private Collider2D wallCollider;
    private SpriteRenderer spriteRenderer;

    private AudioSource audioSource;
    private bool lastActiveState; 
    private bool isInitialized = false;

    void Awake()
    {
        wallCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if(wallCollider != null)
        {
            wallCollider.isTrigger = true;
        }
    }
    void Start()
    {
        if (audioSource != null && AudioManager.Instance != null)
        {
            audioSource.outputAudioMixerGroup = AudioManager.Instance.SfxGroup;
            audioSource.playOnAwake = false;
        }
        UpdateWallState();
        isInitialized = true;
    }

    private void Update()
    {
        bool anyForced = false;

        if (disableButtons != null)
            foreach (var b in disableButtons)
                if (b != null && b.IsPressed) { anyForced = true; break; }

        if (!anyForced && disableLevers != null)
            foreach (var l in disableLevers)
                if (l != null && l.IsOn) { anyForced = true; break; }

        if (anyForced == forced) return;
        forced = anyForced;
        UpdateWallState();
    }

    public void ToggleWall()
    {
        isOn = !isOn;
        UpdateWallState();
    }

    private void UpdateWallState()
    {
        bool active = isOn && !forced;

        if (isInitialized && active != lastActiveState)
        {
            if (audioSource != null && toggleClip != null) 
            {
                audioSource.PlayOneShot(toggleClip);
            }
        }
        lastActiveState = active;

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
