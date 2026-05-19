using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

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

    private float shakeTimer;
    private float shakeDuration;
    private float shakeMagnitude;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        halfCamHeight = Camera.main.orthographicSize;
        halfCamWidth = halfCamHeight * Camera.main.aspect;
        fixedZ = transform.position.z;
    }

    // duration: 흔들림 지속 시간(초), magnitude: 흔들림 강도(유닛)
    public void Shake(float duration = 0.4f, float magnitude = 0.18f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        shakeTimer = duration;
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

        Vector2 shakeOffset = Vector2.zero;
        if (shakeTimer > 0f)
        {
            float ratio = shakeTimer / shakeDuration; // 1→0으로 감쇠
            shakeOffset = Random.insideUnitCircle * shakeMagnitude * ratio;
            shakeTimer -= Time.deltaTime;
        }

        transform.position = new Vector3(newX + shakeOffset.x, newY + shakeOffset.y, fixedZ);
    }
}
