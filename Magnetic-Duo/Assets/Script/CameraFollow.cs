using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("카메라 추적 설정")]
    [SerializeField] private float followSpeed = 5f;

    [Header("맵 경계 설정")]
    [SerializeField] private float mapMinX = -20f;
    [SerializeField] private float mapMaxX = 20f;
    [SerializeField] private float mapMinY = -20f;
    [SerializeField] private float mapMaxY = 20f;

    private float halfCamWidth;
    private float halfCamHeight;
    private float fixedZ;

    void Start()
    {
        halfCamHeight = Camera.main.orthographicSize;
        halfCamWidth = halfCamHeight * Camera.main.aspect;
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        Transform target = CharacterManager.Instance?.ActiveCharacterTransform;
        if (target == null) return;

        float targetX = target.position.x;
        float targetY = target.position.y;

        float clampedX = Mathf.Clamp(targetX, mapMinX + halfCamWidth, mapMaxX - halfCamWidth);
        float clampedY = Mathf.Clamp(targetY, mapMinY + halfCamHeight, mapMaxY - halfCamHeight);

        float newX = Mathf.Lerp(transform.position.x, clampedX, followSpeed * Time.deltaTime);
        float newY = Mathf.Lerp(transform.position.y, clampedY, followSpeed * Time.deltaTime);

        transform.position = new Vector3(newX, newY, fixedZ);
    }
}
