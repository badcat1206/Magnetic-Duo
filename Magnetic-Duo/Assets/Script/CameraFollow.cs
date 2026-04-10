using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("카메라 추적 설정")]
    [SerializeField] private float followSpeed = 5f;

    [Header("맵 경계 설정")]
    [SerializeField] private float mapMinX = -20f;
    [SerializeField] private float mapMaxX = 20f;

    private float halfCamWidth;
    private float fixedY;
    private float fixedZ;

    void Start()
    {
        float camHeight = Camera.main.orthographicSize;
        float aspect = Camera.main.aspect;
        halfCamWidth = camHeight * aspect;

        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        Transform target = CharacterManager.Instance?.ActiveCharacterTransform;
        if (target == null) return;

        float targetX = target.position.x;
        float clampedX = Mathf.Clamp(targetX, mapMinX + halfCamWidth, mapMaxX - halfCamWidth);

        float newX = Mathf.Lerp(transform.position.x, clampedX, followSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, fixedY, fixedZ);
    }
}
