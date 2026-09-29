using System.Collections.Generic;
using UnityEngine;

// 레버로 켜고 끄는 점프 발판. 켜져 있을 때 같은 극성의 봇/상자가 위에 착지하면 위로 튕겨 올림
public class JumpPlatform : MonoBehaviour
{
    [Header("발판 설정")]
    [SerializeField] private Polarity platformPolarity;
    [SerializeField] private bool startActive = false;
    [Tooltip("튕겨 올라가는 높이")]
    [SerializeField] private float jumpHeight = 5f;
    [Tooltip("꼭대기까지 올라가는 데 걸리는 시간(초). 클수록 천천히 올라감")]
    [SerializeField] private float riseTime = 0.8f;

    [Header("스프라이트")]
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;

    [Header("오디오")]
    [SerializeField] private AudioClip jumpClip;
    private AudioSource audioSource;

    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private bool isActive;

    // 발판 위에 올라와 있는 같은 극성의 대상
    private readonly HashSet<Rigidbody2D> riders = new();

    // 상승 중인 대상과 원래 중력값 (꼭대기에 도달하면 복구)
    private readonly Dictionary<Rigidbody2D, float> rising = new();

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        isActive = startActive;
        UpdateVisual();
    }

    private void Start()
    {
        if (audioSource != null && AudioManager.Instance != null)
            audioSource.outputAudioMixerGroup = AudioManager.Instance.SfxGroup;
    }

    public void TogglePlatform()
    {
        isActive = !isActive;
        UpdateVisual();

        if (isActive)
        {
            foreach (var rb in riders)
                if (rb != null) Launch(rb);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsMatchingPolarity(collision.gameObject) || !IsOnTop(collision.collider)) return;

        Rigidbody2D rb = collision.rigidbody;
        if (rb == null) return;

        riders.Add(rb);
        if (isActive) Launch(rb);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.rigidbody != null)
            riders.Remove(collision.rigidbody);
    }

    private bool IsMatchingPolarity(GameObject obj)
    {
        MagneticAbility bot = obj.GetComponent<MagneticAbility>();
        if (bot != null) return bot.BotPolarity == platformPolarity;

        MagneticObject box = obj.GetComponent<MagneticObject>();
        if (box != null) return box.polarity == platformPolarity;

        return false;
    }

    // 옆면이 아니라 발판 윗면에 올라온 경우만 인정
    private bool IsOnTop(Collider2D other)
    {
        if (platformCollider == null) return true;
        return other.bounds.min.y >= platformCollider.bounds.max.y - 0.1f;
    }

    // 높이 h를 시간 t 동안 올라가려면: 속도 = 2h/t, 중력 = 2h/t²
    private void Launch(Rigidbody2D rb)
    {
        if (!rising.ContainsKey(rb))
            rising[rb] = rb.gravityScale;

        float gravity = 2f * jumpHeight / (riseTime * riseTime);
        rb.gravityScale = gravity / Mathf.Abs(Physics2D.gravity.y);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 2f * jumpHeight / riseTime);

        if (audioSource != null && jumpClip != null)
            audioSource.PlayOneShot(jumpClip);
    }

    private void FixedUpdate()
    {
        if (rising.Count == 0) return;

        var reachedTop = new List<Rigidbody2D>();
        foreach (var pair in rising)
        {
            if (pair.Key == null || pair.Key.linearVelocity.y <= 0f)
                reachedTop.Add(pair.Key);
        }

        foreach (var rb in reachedTop)
        {
            if (rb != null) rb.gravityScale = rising[rb];
            rising.Remove(rb);
        }
    }

    private void UpdateVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = isActive ? activeSprite : inactiveSprite;
    }
}
