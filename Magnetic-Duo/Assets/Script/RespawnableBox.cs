using UnityEngine;

/// <summary>
/// 상자가 스스로 리스폰 위치를 기억하고, 요청받으면 그 위치로 돌아갑니다.
/// Trap 등 외부에서는 Respawn()만 호출하면 됩니다.
/// </summary>
public class RespawnableBox : MonoBehaviour
{
    [Tooltip("비워두면 게임 시작 시의 위치에서 리스폰됩니다.")]
    [SerializeField] private Transform respawnPoint;

    private Vector3 startPosition;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    public void Respawn()
    {
        Vector3 target = startPosition;
        if (respawnPoint != null)
        {
            // 2D 게임이므로 Z값은 상자 원래 값을 유지 (렌더링 순서 보존)
            target = new Vector3(respawnPoint.position.x, respawnPoint.position.y, startPosition.z);
        }

        transform.position = target;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
