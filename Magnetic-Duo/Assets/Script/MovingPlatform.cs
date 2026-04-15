using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("연결된 버튼")]
    [SerializeField] private PressureButton button;

    [Header("이동 설정")]
    [SerializeField] private Vector3 targetOffset;  // 시작 위치 기준 이동할 거리
    [SerializeField] private float moveSpeed = 3f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + targetOffset;
    }

    private void Update()
    {
        Vector3 destination = button.IsPressed ? targetPosition : startPosition;
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + targetOffset, transform.localScale);
        Gizmos.DrawLine(transform.position, transform.position + targetOffset);
    }
}
