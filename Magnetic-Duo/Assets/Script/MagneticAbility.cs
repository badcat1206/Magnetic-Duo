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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public bool IsActive => isActive;

    public void ToggleMagnetic()
    {
        isActive = !isActive;
        //Debug.Log(gameObject.name + " Magnetic Power: " + (isActive ? "ON" : "OFF"));
    }

    public void SetMagneticActive(bool active)
    {
        isActive = active;
    }

    public void DeactivateMagnetic()
    {
        isActive = false;
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

            MagneticObject target = col.GetComponent<MagneticObject>();
            if (target != null)
            {
                Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;
                float distance = direction.magnitude;
                if (distance == 0) continue;

                direction.Normalize();

                bool isSamePolarity = (botPolarity == target.polarity);
                float forceAmount = forceStrength / Mathf.Max(distance, 1.0f);

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
                }
            }

            Lever lever = col.GetComponent<Lever>();
            if (lever != null)
            {
                Vector2 direction = (Vector2)lever.transform.position - (Vector2)transform.position;
                float distance = direction.magnitude;
                if (distance == 0) continue;

                direction.Normalize();

                bool isSamePolarity = (botPolarity == lever.polarity);
                float forceAmount = forceStrength / Mathf.Max(distance, 1.0f);

                if (isSamePolarity)
                {
                    lever.AddMagneticForce(direction.x * forceAmount);
                }
                else
                {
                    if (distance > stopDistance)
                    {
                        lever.AddMagneticForce(-direction.x * forceAmount);
                    }
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
