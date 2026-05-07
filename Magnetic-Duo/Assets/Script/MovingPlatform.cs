using UnityEngine;
using UnityEngine.Tilemaps;

public class MovingPlatform : MonoBehaviour
{
    [Header("연결된 버튼")]
    [SerializeField] private PressureButton button;

    [Header("이동 설정")]
    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private float moveSpeed = 3f;

    [Header("체인 설정")]
    [SerializeField] private Tilemap hideChainTilemap;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Transform chainMaskTransform;

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + targetOffset;

        if (hideChainTilemap != null)
        {
            hideChainTilemap.gameObject.SetActive(true);
            InitChain();
        }
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

        // TilemapRenderer: 마스크 안쪽만 렌더링
        hideChainTilemap.GetComponent<TilemapRenderer>().maskInteraction =
            SpriteMaskInteraction.VisibleInsideMask;

        // SpriteMask 오브젝트 생성
        var maskObj = new GameObject("ChainMask");
        maskObj.transform.SetParent(hideChainTilemap.transform.parent);
        maskObj.transform.position = new Vector3(centerX, chainTopY, 0f);

        float maskWidth = hideChainTilemap.cellBounds.size.x * cellW + cellW;
        maskObj.transform.localScale = new Vector3(maskWidth, 0f, 1f);

        var mask = maskObj.AddComponent<SpriteMask>();

        // 1×1 흰색 텍스처로 직사각형 마스크 스프라이트 생성
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        // 피벗을 상단 중앙(0.5, 1)으로 → scale.y가 아래쪽으로 성장
        mask.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 1f), 1f);

        chainMaskTransform = maskObj.transform;
    }

    private void Update()
    {
        Vector3 destination = button.IsPressed ? targetPosition : startPosition;
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (chainMaskTransform == null) return;

        float movedDistance = Mathf.Max(0f, startPosition.y - transform.position.y);

        Vector3 scale = chainMaskTransform.localScale;
        scale.y = movedDistance;
        chainMaskTransform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + targetOffset, transform.localScale);
        Gizmos.DrawLine(transform.position, transform.position + targetOffset);
    }
}
