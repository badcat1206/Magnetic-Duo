using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    [Header("연결된 버튼")]
    [SerializeField] private PressureButton button;

    [Header("이동 설정")]
    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private float moveSpeed = 3f;

    [Header("체인 설정")]
    [SerializeField] private Tilemap hideChainTilemap; // 비활성화된 HideChainTilemap

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3Int[] chainTiles; // Y 내림차순 (높은 Y = 천장 근처부터 먼저 보임)
    private int lastShownCount = 0;

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + targetOffset;

        if (hideChainTilemap != null)
        {
            hideChainTilemap.gameObject.SetActive(true);
            InitChainTiles();
        }
    }

    private void InitChainTiles()
    {
        hideChainTilemap.CompressBounds();
        var positions = new List<Vector3Int>();

        foreach (Vector3Int pos in hideChainTilemap.cellBounds.allPositionsWithin)
        {
            if (!hideChainTilemap.HasTile(pos)) continue;
            hideChainTilemap.SetTileFlags(pos, TileFlags.None);
            hideChainTilemap.SetColor(pos, Color.clear);
            positions.Add(pos);
        }

        // Y 내림차순: 높은 Y(천장 근처) 타일부터 먼저 보임
        positions.Sort((a, b) => b.y.CompareTo(a.y));
        chainTiles = positions.ToArray();
    }

    private void Update()
    {
        Vector3 destination = button.IsPressed ? targetPosition : startPosition;
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (hideChainTilemap == null || chainTiles == null) return;

        float movedDistance = startPosition.y - transform.position.y;
        int movedCells = Mathf.Clamp(
            Mathf.CeilToInt(movedDistance / hideChainTilemap.layoutGrid.cellSize.y),
            0, chainTiles.Length
        );

        if (movedCells == lastShownCount) return;

        // 플랫폼이 내려갈 때: 새 타일 보이기
        for (int i = lastShownCount; i < movedCells; i++)
            hideChainTilemap.SetColor(chainTiles[i], Color.white);

        // 플랫폼이 올라갈 때: 타일 다시 숨기기
        for (int i = movedCells; i < lastShownCount; i++)
            hideChainTilemap.SetColor(chainTiles[i], Color.clear);

        lastShownCount = movedCells;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + targetOffset, transform.localScale);
        Gizmos.DrawLine(transform.position, transform.position + targetOffset);
    }
}
