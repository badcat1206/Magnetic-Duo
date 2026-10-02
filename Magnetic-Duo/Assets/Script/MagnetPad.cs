using System.Collections.Generic;
using UnityEngine;

// 천장에 붙어 반대 극성 상자를 끌어당겨 매다는 패드 (N패드 ↔ S상자, S패드 ↔ N상자)
public class MagnetPad : MonoBehaviour
{
    [Header("패드 설정")]
    [SerializeField] private Polarity padPolarity;
    [SerializeField] private bool startActive = true;

    [Header("버튼 (누르는 동안 현재 상태의 반대로 작동)")]
    [SerializeField] private PressureButton[] buttons;

    [Header("끌어당김 설정")]
    [SerializeField] private float pullRange = 4f;
    [SerializeField] private float pullSpeed = 6f;

    [Header("스프라이트")]
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;

    [Header("오디오")]
    [SerializeField] private AudioClip padOnClip;
    [SerializeField] private AudioClip padOffClip;
    private AudioSource audioSource;

    private SpriteRenderer spriteRenderer;
    private bool leverState; // 시작 상태 + 레버로 바뀌는 기본 상태
    private bool isActive;   // 버튼까지 반영한 실제 상태

    // 끌어당기는 중인 상자와 원래 중력값
    private readonly Dictionary<Rigidbody2D, float> heldBoxes = new();

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        leverState = startActive;
        isActive = leverState;
        UpdateVisual();
    }

    private void Start()
    {
        if (audioSource != null && AudioManager.Instance != null)
            audioSource.outputAudioMixerGroup = AudioManager.Instance.SfxGroup;
    }

    public void TogglePad()
    {
        leverState = !leverState;
        RefreshState();
    }

    private void Update()
    {
        RefreshState();
    }

    private bool IsAnyButtonPressed()
    {
        if (buttons == null) return false;
        foreach (var b in buttons)
            if (b != null && b.IsPressed) return true;
        return false;
    }

    private void RefreshState()
    {
        bool target = leverState != IsAnyButtonPressed();
        if (target == isActive) return;

        isActive = target;
        if (!isActive) ReleaseAll();
        UpdateVisual();

        AudioClip clip = isActive ? padOnClip : padOffClip;
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        var inRange = new HashSet<Rigidbody2D>();

        foreach (var col in Physics2D.OverlapCircleAll(transform.position, pullRange))
        {
            MagneticObject box = col.GetComponent<MagneticObject>();
            if (box == null || box.polarity == padPolarity) continue;

            Rigidbody2D rb = col.attachedRigidbody;
            if (rb == null) continue;

            inRange.Add(rb);
            if (!heldBoxes.ContainsKey(rb))
            {
                heldBoxes[rb] = rb.gravityScale;
                rb.gravityScale = 0f;
            }

            PullTowardPad(rb, col);
        }

        var leftRange = new List<Rigidbody2D>();
        foreach (var rb in heldBoxes.Keys)
            if (!inRange.Contains(rb)) leftRange.Add(rb);
        foreach (var rb in leftRange)
            Release(rb);
    }

    private void PullTowardPad(Rigidbody2D rb, Collider2D col)
    {
        float padHalfHeight = spriteRenderer != null ? spriteRenderer.bounds.extents.y : 0f;
        Vector2 attachPoint = (Vector2)transform.position + Vector2.down * (padHalfHeight + col.bounds.extents.y);

        Vector2 toTarget = attachPoint - rb.position;
        float maxStep = pullSpeed * Time.fixedDeltaTime;

        rb.linearVelocity = toTarget.magnitude <= maxStep
            ? toTarget / Time.fixedDeltaTime
            : toTarget.normalized * pullSpeed;
    }

    private void Release(Rigidbody2D rb)
    {
        if (rb != null) rb.gravityScale = heldBoxes[rb];
        heldBoxes.Remove(rb);
    }

    private void ReleaseAll()
    {
        foreach (var pair in heldBoxes)
            if (pair.Key != null) pair.Key.gravityScale = pair.Value;
        heldBoxes.Clear();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = isActive ? activeSprite : inactiveSprite;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = padPolarity == Polarity.N ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, pullRange);
    }
}
