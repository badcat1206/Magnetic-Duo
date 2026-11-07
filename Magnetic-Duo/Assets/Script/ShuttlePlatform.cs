using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// 웨이포인트 배열을 순환하며 이동하는 발판
// 발판 위에 올라탄 상자·봇(쌓인 것 포함)은 물리 프레임마다 발판과 같은 거리만큼 함께 옮김 (PlatformRiders)
public class ShuttlePlatform : MonoBehaviour
{
    [Header("발판 설정")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float waitTime = 1f;

    [Header("자성 설정")]
    [Tooltip("이 극성의 자력만 발판을 작동시킵니다.")]
    [SerializeField] private Polarity platformPolarity;

    [Header("위치 지정")]
    [FormerlySerializedAs("startPoint")]
    [SerializeField] private Vector2 middlePoint;

    [Header("첫 번째 움직임")]
    [SerializeField] private bool useFirstPoint = true;
    [FormerlySerializedAs("downPoint")]
    [SerializeField] private Vector2 firstPoint;

    [Header("두 번째 움직임")]
    [SerializeField] private bool useSecondPoint = true;
    [FormerlySerializedAs("endPoint")]
    [FormerlySerializedAs("upPoint")]
    [SerializeField] private Vector2 secondPoint;

    [Header("비주얼 오브젝트 (자성 OFF/ON 이미지)")]
    [SerializeField] private SpriteRenderer visualOff;
    [SerializeField] private SpriteRenderer visualOn;

    [Header("경로 이동 설정")]
    [Tooltip("체크하면 마지막 지점에서 처음으로 바로 돌아가지 않고, 거쳐온 지점을 순서대로 되짚어 돌아갑니다.")]
    [SerializeField] private bool retraceReversePath = false;

    private Vector2[] waypoints;
    private int targetIndex;
    private int direction = 1;
    private Vector2 targetPoint;
    private bool isWaiting = false;
    private bool wasMagneticActive = false;

    private MagneticAbility riderAbility;

    private int matchingBoxCount = 0;

    // 이동 전 충돌 검사용 (다른 발판/봇/상자를 뚫지 않도록)
    private const float CastSkin = 0.02f;
    private readonly RaycastHit2D[] castHits = new RaycastHit2D[16];
    private ContactFilter2D castFilter;
    private Collider2D[] platformColliders;
    private readonly HashSet<Rigidbody2D> riders = new HashSet<Rigidbody2D>();

    void Start()
    {
        List<Vector2> points = new List<Vector2> { middlePoint };

        if (useFirstPoint) points.Add(firstPoint);
        if (useSecondPoint) points.Add(secondPoint);

        waypoints = points.ToArray();

        transform.position = middlePoint;
        targetIndex = waypoints.Length > 1 ? 1 : 0;
        targetPoint = waypoints[targetIndex];
        UpdateVisual(false);

        platformColliders = GetComponents<Collider2D>();
        castFilter = new ContactFilter2D { useTriggers = false };
    }

    void FixedUpdate()
    {
        bool isActive = (riderAbility != null && riderAbility.IsActive) || (matchingBoxCount > 0);

        if (isActive != wasMagneticActive)
        {
            wasMagneticActive = isActive;
            UpdateVisual(isActive);
        }

        if(isWaiting) return;

        if(!isActive) return;

        Vector2 current = transform.position;
        Vector2 next = Vector2.MoveTowards(current, targetPoint, speed * Time.deltaTime);

        PlatformRiders.Collect(platformColliders, riders);

        // 앞이 막혀 있으면 이번 프레임은 제자리 (이동 중 상태는 유지 → 비켜나면 이어서 이동)
        if (IsBlocked(next - current)) return;
        transform.position = next;
        PlatformRiders.Carry(riders, next - current);

        if(Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            StartCoroutine(WaitAndTurn());
        }
    }

    private IEnumerator WaitAndTurn()
    {
        isWaiting = true;

        yield return new WaitForSeconds(waitTime);

        if (retraceReversePath)
        {
            if (targetIndex + direction < 0 || targetIndex + direction >= waypoints.Length)
                direction = -direction;
            targetIndex += direction;
        }
        else
        {
            targetIndex = (targetIndex + 1) % waypoints.Length;
        }
        targetPoint = waypoints[targetIndex];

        isWaiting = false;
    }

    // 발판 자신과 위에 탄 봇/상자 중 하나라도 이동 방향으로 다른 발판/봇/상자에 닿으면 막힘 (바닥/벽은 무시)
    private bool IsBlocked(Vector2 delta)
    {
        if (delta == Vector2.zero) return false;

        foreach (var col in platformColliders)
            if (IsColliderBlocked(col, delta)) return true;

        foreach (Rigidbody2D rider in riders)
        {
            if (!IsRiderTag(rider.gameObject)) continue;
            foreach (var col in rider.GetComponents<Collider2D>())
                if (IsColliderBlocked(col, delta)) return true;
        }
        return false;
    }

    private bool IsColliderBlocked(Collider2D col, Vector2 delta)
    {
        if (col == null || !col.enabled || col.isTrigger) return false;

        // 살짝 줄인 크기로 검사 → 옆면에 맞닿아 있기만 한 물체는 막힘으로 보지 않음
        Bounds b = col.bounds;
        Vector2 size = new Vector2(Mathf.Max(b.size.x - CastSkin * 2f, 0.01f), Mathf.Max(b.size.y - CastSkin * 2f, 0.01f));
        int count = Physics2D.BoxCast(b.center, size, 0f, delta.normalized, castFilter, castHits, delta.magnitude + CastSkin);

        for (int i = 0; i < count; i++)
        {
            Collider2D other = castHits[i].collider;

            // 자기 자신이나 자기 위에 탄 봇/상자는 제외
            if (other.GetComponentInParent<ShuttlePlatform>() == this) continue;
            if (other.attachedRigidbody != null && riders.Contains(other.attachedRigidbody)) continue;
            if (!IsBlockingObject(other)) continue;

            // 이동 방향 앞쪽에 있는 물체만 막힘 (이미 겹쳐 있는 뒤쪽 물체에서는 빠져나갈 수 있게)
            if (Vector2.Dot((Vector2)(other.bounds.center - b.center), delta) > 0f) return true;
        }
        return false;
    }

    private bool IsBlockingObject(Collider2D other)
    {
        if (other.GetComponentInParent<ShuttlePlatform>() != null) return true; // 다른 발판

        GameObject obj = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        return IsRiderTag(obj);
    }

    private static bool IsRiderTag(GameObject obj)
    {
        return obj.CompareTag("NBot") || obj.CompareTag("SBot") || obj.CompareTag("NBox") || obj.CompareTag("SBox");
    }

    private void UpdateVisual(bool isActive)
    {
        if (visualOff != null) visualOff.enabled = !isActive;
        if (visualOn != null) visualOn.enabled = isActive;
    }

    // 현재 오브젝트가 조건에 맞는 상자인지 검사하는 함수
    private bool IsMatchingBox(GameObject obj)
    {
        // Polarity가 N일 때는 NBox, S일 때는 SBox인지 확인합니다.
        if (platformPolarity.ToString() == "N" && obj.CompareTag("NBox")) return true;
        if (platformPolarity.ToString() == "S" && obj.CompareTag("SBox")) return true;
        
        return false;
    }

    // 발판에 닿은 봇/상자의 극성 확인 → 같은 극성이면 발판 작동
    private void OnCollisionEnter2D(Collision2D collision)
    {
        MagneticAbility magneticAbility = collision.gameObject.GetComponent<MagneticAbility>();
        if(magneticAbility != null && magneticAbility.BotPolarity == platformPolarity)
        {
            riderAbility = magneticAbility;
        }

        if (IsMatchingBox(collision.gameObject))
        {
            matchingBoxCount++;
        }
    }

    // 발판에서 떨어진 봇/상자는 작동 조건에서 제외
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (riderAbility != null && collision.gameObject == riderAbility.gameObject)
        {
            riderAbility = null; 
        }
        
        if (IsMatchingBox(collision.gameObject))
        {
            matchingBoxCount--;
            if (matchingBoxCount < 0) matchingBoxCount = 0; // 혹시 모를 버그 방지용 안전장치
        }
    }
}
