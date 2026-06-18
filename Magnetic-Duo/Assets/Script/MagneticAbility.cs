using UnityEngine;

public class MagneticAbility : MonoBehaviour
{
    [Header("Magnetic Settings")]
    [SerializeField] private Polarity botPolarity;
    [SerializeField] private float range = 5f;
    [SerializeField] private float forceStrength = 10f;

    [SerializeField] private float stopDistance = 1.8f; // 당길 때 플레이어에게 다가올 거리

    private bool isActive = false;
    private Rigidbody2D rb;
    private PlayerAnimation playerAnim;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<PlayerAnimation>();

        if(playerAnim == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerAnimation 컴포넌트를 찾을 수 없어 자성 시각 효과가 재생되지 않습니다.");
        }
    }

    public bool IsActive => isActive;

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
                    // stopDistance 보다 멀때만 당김
                    if (distance > stopDistance)
                    {
                        if (targetRb != null)
                        {
                            target.AddMagneticForce(-direction.x * forceAmount); // X축 힘 전달
                            targetRb.AddForce(Vector2.up * -direction.y * forceAmount); // Y축은 물리 법칙 적용
                        }
                        // 플레이어에 가해지는 반작용 힘을 제거하여 끌려가지 않게 함
                    }
                    else
                    {
                        target.AddMagneticForce(0f);
                    }
                }
            }

            Lever tagetLever = col.GetComponent<Lever>();
            if (tagetLever != null)
            {
                if (botPolarity == tagetLever.polarity)
                {
                    tagetLever.AddMagneticForce(forceAmount);
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
