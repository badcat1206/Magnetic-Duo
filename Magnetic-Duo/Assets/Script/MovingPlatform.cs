using UnityEngine;
using UnityEngine.Tilemaps;

public class MovingPlatform : MonoBehaviour
{
    [Header("연결된 버튼 (하나라도 눌리면 이동)")]
    [SerializeField] private PressureButton[] buttons;

    [Header("이동 설정")]
    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private float moveSpeed = 3f;

    [Header("추가 하강 설정 (primaryActive 상태에서 추가 이동)")]
    [SerializeField] private PressureButton[] extraButtons;
    [SerializeField] private Vector3 extraOffset;

    [Header("화면 흔들림 설정")]
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float shakeMagnitude = 0.18f;

    [Header("체인 설정 (타일맵 방식)")]
    [SerializeField] private Tilemap hideChainTilemap;

    [Header("체인 설정 (스프라이트 방식 - 타일맵 대신 사용)")]
    [SerializeField] private SpriteRenderer chainSpriteRenderer;

    [Header("오디오")]
    [SerializeField] private AudioClip chainMoveClip;
    private AudioSource audioSource;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 extraTargetPosition;
    private Transform chainMaskTransform;
    private bool isLeverActive = false;
    private Vector3 previousDestination;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        startPosition = transform.position;
        targetPosition = startPosition + targetOffset;
        extraTargetPosition = targetPosition + extraOffset;
        previousDestination = startPosition;

        if (hideChainTilemap != null)
        {
            hideChainTilemap.gameObject.SetActive(true);
            InitChain();
        }
    }

    public void SetLeverActive(bool active)
    {
        isLeverActive = active;
    }

    private bool IsPrimaryActive()
    {
        if (isLeverActive) return true;
        if (buttons != null)
            foreach (var b in buttons)
                if (b != null && b.IsPressed) return true;
        return false;
    }

    private bool IsExtraActive()
    {
        if (extraButtons == null) return false;
        foreach (var b in extraButtons)
            if (b != null && b.IsPressed) return true;
        return false;
    }

    private void Update()
    {
        bool primaryActive = IsPrimaryActive();
        bool extraActive = IsExtraActive();

        Vector3 destination;
        if (primaryActive && extraActive)
            destination = extraTargetPosition;
        else if (primaryActive)
            destination = targetPosition;
        else
            destination = startPosition;

        if (destination != previousDestination)
        {
            CameraFollow.Instance?.Shake(shakeDuration, shakeMagnitude);
            if (audioSource != null && chainMoveClip != null)
                audioSource.PlayOneShot(chainMoveClip);
            previousDestination = destination;
        }

        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        if (transform.position == destination && audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void InitChain()
    {
        hideChainTilemap.CompressBounds();

        float cellW = hideChainTilemap.layoutGrid.cellSize.x;
        float cellH = hideChainTilemap.layoutGrid.cellSize.y;

        int maxTileY = int.MinValue;
        float sumX = 0f;
        int count = 0;

        foreach (Vector3Int pos in hideChainTilemap.cellBounds.allPositionsWithin)
        {
            if (!hideChainTilemap.HasTile(pos)) continue;
            if (pos.y > maxTileY) maxTileY = pos.y;
            sumX += hideChainTilemap.GetCellCenterWorld(pos).x;
            count++;
        }

        if (count == 0) return;

        float centerX = sumX / count;
        Vector3Int topCell = new Vector3Int(hideChainTilemap.cellBounds.xMin, maxTileY, 0);
        float chainTopY = hideChainTilemap.GetCellCenterWorld(topCell).y + cellH * 0.5f;

        hideChainTilemap.GetComponent<TilemapRenderer>().maskInteraction =
            SpriteMaskInteraction.VisibleInsideMask;

        var maskObj = new GameObject("ChainMask");
        maskObj.transform.SetParent(hideChainTilemap.transform.parent);
        maskObj.transform.position = new Vector3(centerX, chainTopY, 0f);

        float maskWidth = hideChainTilemap.cellBounds.size.x * cellW + cellW;
        maskObj.transform.localScale = new Vector3(maskWidth, 0f, 1f);

        var mask = maskObj.AddComponent<SpriteMask>();

        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        mask.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 1f), 1f);

        chainMaskTransform = maskObj.transform;
    }

    private void LateUpdate()
    {
        float movedDistance = Mathf.Max(0f, startPosition.y - transform.position.y);

        if (chainMaskTransform != null)
        {
            Vector3 scale = chainMaskTransform.localScale;
            scale.y = movedDistance;
            chainMaskTransform.localScale = scale;
        }

        if (chainSpriteRenderer != null)
        {
            Vector2 size = chainSpriteRenderer.size;
            size.y = movedDistance;
            chainSpriteRenderer.size = size;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + targetOffset, transform.localScale);
        Gizmos.DrawLine(transform.position, transform.position + targetOffset);

        if (extraOffset != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + targetOffset + extraOffset, transform.localScale);
            Gizmos.DrawLine(transform.position + targetOffset, transform.position + targetOffset + extraOffset);
        }
    }
}
