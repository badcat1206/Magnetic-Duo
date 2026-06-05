using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public enum BeltMode { ToggleOnOff, ReverseDirection }

    [Header("벨트 설정")]
    [SerializeField] private BeltMode beltMode = BeltMode.ToggleOnOff;
    [SerializeField] private float activeSpeed = 5f;
    [SerializeField] private bool isRunning = false;

    [Header("파트 프리팹 (SpriteRenderer + ConveyorBeltPart 포함)")]
    [SerializeField] private GameObject leftEdgePrefab;
    [SerializeField] private GameObject centerPrefab;
    [SerializeField] private GameObject rightEdgePrefab;
    [SerializeField] private int centerCount = 1;

    [Header("애니메이션")]
    [SerializeField, Range(1f, 24f)] private float frameRate = 8f;

    [Header("필터링 설정")]
    [SerializeField] private LayerMask boxLayer;

    [Header("오디오")]
    [SerializeField] private AudioClip beltRightClip;
    [SerializeField] private AudioClip beltLeftClip;

    private AudioSource audioSource;
    private SurfaceEffector2D effector;
    private BoxCollider2D beltCollider;

    private SpriteRenderer leftRenderer;
    private SpriteRenderer rightRenderer;
    private readonly List<SpriteRenderer> centerRenderers = new List<SpriteRenderer>();

    // 프리팹에서 읽어온 프레임 배열
    private Sprite[] leftFrames;
    private Sprite[] centerFrames;
    private Sprite[] rightFrames;

    private float frameTimer;
    private int currentFrame;

    void Awake()
    {
        effector    = GetComponent<SurfaceEffector2D>();
        beltCollider = GetComponent<BoxCollider2D>();
        audioSource  = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (!LoadFramesFromPrefabs()) return;
        BuildBeltVisuals();
        ApplyBeltState();
    }

    // 각 프리팹의 ConveyorBeltPart 컴포넌트에서 프레임 배열을 읽음
    private bool LoadFramesFromPrefabs()
    {
        leftFrames   = ReadFrames(leftEdgePrefab,  "왼쪽 엣지");
        centerFrames = ReadFrames(centerPrefab,    "중앙");
        rightFrames  = ReadFrames(rightEdgePrefab, "오른쪽 엣지");
        return leftFrames != null && centerFrames != null && rightFrames != null;
    }

    private Sprite[] ReadFrames(GameObject prefab, string label)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[ConveyorBelt] {label} 프리팹이 할당되지 않았습니다.", this);
            return null;
        }
        var part = prefab.GetComponent<ConveyorBeltPart>();
        if (part == null)
        {
            Debug.LogWarning($"[ConveyorBelt] {label} 프리팹에 ConveyorBeltPart 컴포넌트가 없습니다.", this);
            return null;
        }
        if (part.frames == null || part.frames.Length < 3 || part.frames[0] == null)
        {
            Debug.LogWarning($"[ConveyorBelt] {label} 프리팹의 ConveyorBeltPart에 프레임이 부족합니다 (3개 필요).", this);
            return null;
        }
        return part.frames;
    }

    private void BuildBeltVisuals()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        centerRenderers.Clear();

        // 루트 SpriteRenderer(Square) 비활성화
        var rootSr = GetComponent<SpriteRenderer>();
        if (rootSr != null) rootSr.enabled = false;

        // 스프라이트 너비를 월드 단위로 계산
        float leftW   = GetPrefabWorldWidth(leftEdgePrefab);
        float centerW = GetPrefabWorldWidth(centerPrefab);
        float rightW  = GetPrefabWorldWidth(rightEdgePrefab);

        if (leftW <= 0f || centerW <= 0f || rightW <= 0f) return;

        float totalWorldWidth = leftW + centerCount * centerW + rightW;

        // 부모 스케일: 자식 localPosition/localScale 계산에 사용
        float psx = transform.lossyScale.x;
        float psy = transform.lossyScale.y;

        // cursor는 월드 좌표 기준으로 진행
        float cursorWorld = -totalWorldWidth * 0.5f;

        leftRenderer = SpawnTile(leftEdgePrefab, cursorWorld, leftW, psx, psy);
        cursorWorld += leftW;

        for (int i = 0; i < centerCount; i++)
        {
            centerRenderers.Add(SpawnTile(centerPrefab, cursorWorld, centerW, psx, psy));
            cursorWorld += centerW;
        }

        rightRenderer = SpawnTile(rightEdgePrefab, cursorWorld, rightW, psx, psy);

        // BoxCollider2D size는 로컬 스케일 기준이므로 부모 스케일로 나눔
        if (beltCollider != null)
        {
            beltCollider.size   = new Vector2(totalWorldWidth / psx, beltCollider.size.y);
            beltCollider.offset = new Vector2(0f, beltCollider.offset.y);
        }
    }

    private SpriteRenderer SpawnTile(GameObject prefab, float worldCursorX, float worldWidth, float psx, float psy)
    {
        var obj = Instantiate(prefab, transform);
        // localPosition = 월드 좌표 / 부모 스케일
        obj.transform.localPosition = new Vector3((worldCursorX + worldWidth * 0.5f) / psx, 0f, 0f);
        // 부모 스케일을 상쇄해 스프라이트가 자연 크기로 렌더링
        var ps = prefab.transform.localScale;
        obj.transform.localScale = new Vector3(ps.x / psx, ps.y / psy, 1f);
        return obj.GetComponent<SpriteRenderer>();
    }

    // 프리팹 스프라이트의 월드 너비 (prefab localScale 반영)
    private float GetPrefabWorldWidth(GameObject prefab)
    {
        var sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return 0f;
        return sr.sprite.bounds.size.x * prefab.transform.localScale.x;
    }

    void Update()
    {
        if (!isRunning) return;

        // 벨트 속도에 비례해 애니메이션 재생 속도 결정 (기준 속도 5)
        frameTimer += Time.deltaTime * (Mathf.Abs(activeSpeed) / 5f);
        if (frameTimer >= 1f / frameRate)
        {
            frameTimer = 0f;
            // activeSpeed < 0 (왼쪽): 001→002→003 순방향
            // activeSpeed > 0 (오른쪽): 001→003→002 역방향
            currentFrame = activeSpeed < 0
                ? (currentFrame + 1) % 3
                : (currentFrame + 2) % 3;
            RefreshSprites();
        }
    }

    private void RefreshSprites()
    {
        if (leftFrames == null) return;

        if (leftRenderer != null)
            leftRenderer.sprite = leftFrames[currentFrame];
        if (rightRenderer != null)
            rightRenderer.sprite = rightFrames[currentFrame];
        foreach (var r in centerRenderers)
        {
            if (r != null) r.sprite = centerFrames[currentFrame];
        }
    }

    private void ApplyBeltState()
    {
        effector.speed = isRunning ? activeSpeed : 0f;
        currentFrame   = 0;
        RefreshSprites();
    }

    public void ToggleGBelt()
    {
        if (beltMode == BeltMode.ToggleOnOff)
            isRunning = !isRunning;
        else if (beltMode == BeltMode.ReverseDirection)
            activeSpeed = -activeSpeed;

        ApplyBeltState();

        if (audioSource != null)
        {
            AudioClip clip = null;
            if (beltMode == BeltMode.ToggleOnOff)
                clip = isRunning ? beltLeftClip : beltRightClip;   // 켜기 → 왼쪽, 끄기 → 오른쪽
            else if (beltMode == BeltMode.ReverseDirection)
                clip = activeSpeed < 0 ? beltLeftClip : beltRightClip;

            if (clip != null) audioSource.PlayOneShot(clip);
        }

        if (isRunning) GiveNudgeToBoxesOnMe();
    }

    private void GiveNudgeToBoxesOnMe()
    {
        var results = new List<Collider2D>();
        var filter  = new ContactFilter2D();
        filter.SetLayerMask(boxLayer);
        filter.useLayerMask = true;
        Physics2D.OverlapCollider(beltCollider, filter, results);

        foreach (var col in results)
        {
            if (col.bounds.center.x < beltCollider.bounds.min.x ||
                col.bounds.center.x > beltCollider.bounds.max.x) continue;

            var rb = col.GetComponent<Rigidbody2D>();
            if (rb == null) continue;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(Mathf.Sign(-activeSpeed) * 3f, 0), ForceMode2D.Impulse);
        }
    }
}
