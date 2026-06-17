using Unity.VisualScripting;
using UnityEngine;

public class MagneticField : MonoBehaviour
{
   [Header("기본 설정")]
   [SerializeField] private bool isOn = true;

   [Header("연결할 컴포넌트")]
   [SerializeField] private GameObject electricEffect;
   [SerializeField] Collider2D electricCollider;

   [Header("오디오")]
   [SerializeField] private AudioClip fieldOnClip;
   [SerializeField] private AudioClip fieldOffClip;
   private AudioSource audioSource;

   [Header("버튼으로 강제 비활성화 (누르는 동안 OFF)")]
   [SerializeField] private PressureButton[] disableButtons;
   private bool buttonForced = false;

   private Animator animator;

    void Awake()
    {
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
        audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        UpdateFieldState();
    }

    private void Update()
    {
        bool anyPressed = false;
        if (disableButtons != null)
            foreach (var b in disableButtons)
                if (b != null && b.IsPressed) { anyPressed = true; break; }

        if (anyPressed == buttonForced) return;
        buttonForced = anyPressed;
        UpdateFieldState();

        if (audioSource != null)
        {
            bool active = isOn && !buttonForced;
            if (active && fieldOnClip != null) audioSource.PlayOneShot(fieldOnClip);
            else if (!active && fieldOffClip != null) audioSource.PlayOneShot(fieldOffClip);
        }
    }

    public void ToggleField()
    {
        isOn = !isOn;
        UpdateFieldState();

        if (audioSource != null)
        {
            if (isOn && fieldOnClip != null)
                audioSource.PlayOneShot(fieldOnClip);
            else if (!isOn && fieldOffClip != null)
                audioSource.PlayOneShot(fieldOffClip);
        }
    }

    void UpdateFieldState()
    {
        bool active = isOn && !buttonForced;
        animator.SetBool("IsOn", active);
        if(electricEffect != null) electricEffect.SetActive(active);
        if(electricCollider != null) electricCollider.enabled = active;
    }
}
