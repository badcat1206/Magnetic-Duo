using UnityEngine;

public class MagneticAbility : MonoBehaviour
{
    [Header("Magnetic Settings")]
    [SerializeField] private Polarity botPolarity;
    [SerializeField] private float range = 5f;
    [SerializeField] private float forceStrength = 10f;

    [SerializeField] private float stopDistance = 1.8f; // 당길 때 플레이어에게 다가올 거리

    // 끌어당긴 상자가 벌어진 간격만큼 빨라지는 정도, 최대 속도(봇 이동속도 대비 배수)
    private const float PullFollowGain = 15f;
    private const float PullMaxSpeedRatio = 2f;

    private bool isActive = false;
    private Rigidbody2D rb;
    private PlayerAnimation playerAnim;
    private PlayerMovement movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<PlayerAnimation>();
        movement = GetComponent<PlayerMovement>();

        if(playerAnim == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerAnimation 컴포넌트를 찾을 수 없어 자성 시각 효과가 재생되지 않습니다.");
        }
    }

    public bool IsActive => isActive;
    public Polarity BotPolarity => botPolarity;

    public void ToggleMagnetic()
    {
        SetMagneticActive(!isActive);
        //Debug.Log(gameObject.name + " Magnetic Power: " + (isActive ? "ON" : "OFF"));
    }

    public void SetMagneticActive(bool active)
    {
        isActive = active;

        if(playerAnim != null)
        {
            playerAnim.SetMagneticMode(isActive);
        }
    }

    public void DeactivateMagnetic()
    {
        SetMagneticActive(false);
        //Debug.Log(gameObject.name + " Magnetic Power: Forced OFF");
    }

    void FixedUpdate()
    {
        if (isActive)
        {
            ApplyForces();
        }
    }

    private void ApplyForces()
    {
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, range);

        foreach (var col in objectsInRange)
        {
            if (col.gameObject == gameObject) continue;

            Vector2 direction = (Vector2)col.transform.position - (Vector2)transform.position;
            float distance = direction.magnitude;
            if (distance == 0) continue;

            direction.Normalize();
            float forceAmount = forceStrength / Mathf.Max(distance, 1.0f);

            MagneticObject target = col.GetComponent<MagneticObject>();
            if (target != null)
            {
                bool isSamePolarity = (botPolarity == target.polarity);
                Rigidbody2D targetRb = col.GetComponent<Rigidbody2D>();

                if (isSamePolarity)
                {
                    if (targetRb != null)
                    {
                        target.AddMagneticForce(direction.x * forceAmount); // X축 힘 전달
                        targetRb.AddForce(Vector2.up * direction.y * forceAmount); // Y축은 물리 법칙 적용
                    }
                    // 플레이어에 가해지는 반작용 힘을 제거하여 밀려나지 않게 함
                }
                else
                {
                    // 가로 간격이 stopDistance보다 벌어진 만큼 빨라짐 → 봇이 계속 걸어가도 같은 속도로 뒤따라옴
                    // (상자의 저항/바닥 마찰로 깎이는 속도도 간격 차이로 보정됨)
                    float dx = transform.position.x - col.transform.position.x;
                    float gap = Mathf.Abs(dx) - stopDistance;
                    float pullVelocity = gap > 0f
                        ? Mathf.Sign(dx) * Mathf.Min(gap * PullFollowGain, movement.MoveSpeed * PullMaxSpeedRatio)
                        : 0f;

                    target.AddMagneticForce(pullVelocity * 2f); // MagneticObject는 전달값의 절반을 속도로 사용

                    // stopDistance 보다 멀때만 Y축으로 당김 (Y축은 물리 법칙 적용)
                    if (distance > stopDistance && targetRb != null)
                        targetRb.AddForce(Vector2.up * -direction.y * forceAmount);
                    // 플레이어에 가해지는 반작용 힘을 제거하여 끌려가지 않게 함
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = (botPolarity == Polarity.N) ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
