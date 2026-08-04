using UnityEngine;

public enum Polarity { N, S }

public class MagneticObject : MonoBehaviour
{
    public Polarity polarity;

    [Header("물리 설정 (미끄러짐 방지)")]
    [SerializeField] private float linearDrag = 10f; // 값이 높을수록 빨리 멈춤
    [SerializeField] private float angularDrag = 5f;  // 회전 멈춤 속도

    [Header("작은 단차 자동 통과 설정")]
    [SerializeField] private float maxStepHeight = 0.15f;
    [SerializeField] private float stepCheckDistance = 0.1f;
    [SerializeField] private float stepClimbSpeed = 4f;
    [SerializeField] private LayerMask stepLayerMask;

    [Header("이미지 설정")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite magnetizedSprite;
    private Rigidbody2D rb;
    private Collider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private bool isBeingMagnetized = false;
    private bool wasMagnetizedLastFrame = false;
    private float accumulatedForceX = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (rb != null)
        {
            rb.linearDamping = linearDrag;
            rb.angularDamping = angularDrag;
            
            // 회전은 항상 고정
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    // 자기력을 누적시키는 메서드
    public void AddMagneticForce(float forceX)
    {
        accumulatedForceX += forceX;
        isBeingMagnetized = true;
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (isBeingMagnetized)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            float targetVelocityX = accumulatedForceX * 0.5f;
            float newVelY = rb.linearVelocity.y;

            // 경사면 감지: 접촉 법선에 X성분이 있으면 경사면
            ContactPoint2D[] contacts = new ContactPoint2D[4];
            int contactCount = rb.GetContacts(contacts);
            for (int i = 0; i < contactCount; i++)
            {
                Vector2 n = contacts[i].normal;
                if (n.y > 0.3f && Mathf.Abs(n.x) > 0.1f)
                {
                    // 오르막 방향일 때 경사각에 맞는 Y속도 계산
                    // slopeVelY = targetVelocityX * tan(경사각) = targetVelocityX * (|n.x| / n.y)
                    float slopeVelY = -targetVelocityX * (n.x / n.y);
                    if (slopeVelY > 0f)
                        newVelY = Mathf.Max(newVelY, slopeVelY);
                    break;
                }
            }

            // 작은 단차 자동 통과: 낮은 위치는 막혀있고 그 위(단차 높이)는 뚫려있으면 밀어 올림
            if (TryDetectStep(targetVelocityX, out float stepVelY))
                newVelY = Mathf.Max(newVelY, stepVelY);

            rb.linearVelocity = new Vector2(targetVelocityX, newVelY);
        }
        else
        {
            // 컨베이어 벨트 위에서 움직이게 하기 위해 주석화
            // 자력이 없을 때는 X축을 완전히 고정
            // rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            // rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if(isBeingMagnetized && !wasMagnetizedLastFrame)
        {
            if(spriteRenderer != null && magnetizedSprite != null)
            {
                spriteRenderer.sprite = magnetizedSprite;
            }
        }
        else if(!isBeingMagnetized && wasMagnetizedLastFrame)
        {
             if(spriteRenderer != null && magnetizedSprite != null)
            {
                spriteRenderer.sprite = normalSprite;
            }
        }
        
        wasMagnetizedLastFrame = isBeingMagnetized;

        // 초기화
        isBeingMagnetized = false;
        accumulatedForceX = 0f;
    }

    // 이동 방향 앞쪽의 낮은 위치는 막혀있고, 단차 높이 지점은 뚫려있으면 넘어갈 수 있는 작은 턱으로 판단
    private bool TryDetectStep(float targetVelocityX, out float stepVelY)
    {
        stepVelY = 0f;
        if (Mathf.Abs(targetVelocityX) < 0.01f || boxCollider == null) return false;

        Vector2 dir = new Vector2(Mathf.Sign(targetVelocityX), 0f);
        Bounds bounds = boxCollider.bounds;
        float castDistance = bounds.extents.x + stepCheckDistance;

        Vector2 lowOrigin = new Vector2(bounds.center.x, bounds.min.y + 0.05f);
        Vector2 highOrigin = new Vector2(bounds.center.x, bounds.min.y + maxStepHeight);

        bool blockedLow = Physics2D.Raycast(lowOrigin, dir, castDistance, stepLayerMask);
        bool blockedHigh = Physics2D.Raycast(highOrigin, dir, castDistance, stepLayerMask);

        if (blockedLow && !blockedHigh)
        {
            stepVelY = stepClimbSpeed;
            return true;
        }
        return false;
    }
}
