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

   [Header("상자 리스폰 설정")]
   [SerializeField] private Vector2 nBoxSpawnPoint;
   [SerializeField] private Vector2 sBoxSpawnPoint;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(isOn && !buttonForced)) return;

        if (collision.CompareTag("NBox"))
            RespawnBox(collision.gameObject, nBoxSpawnPoint);
        else if (collision.CompareTag("SBox"))
            RespawnBox(collision.gameObject, sBoxSpawnPoint);
    }

    private void RespawnBox(GameObject box, Vector2 spawnPoint)
    {
        if (spawnPoint == Vector2.zero)
        {
            Debug.LogWarning($"[MagneticField] {box.name} 리스폰 위치가 (0,0)입니다. Inspector에서 좌표를 설정하세요.");
            return;
        }

        box.transform.position = spawnPoint;

        Rigidbody2D rb = box.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
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
