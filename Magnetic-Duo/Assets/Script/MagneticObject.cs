using UnityEngine;

public enum Polarity { N, S }

public class MagneticObject : MonoBehaviour
{
    public Polarity polarity;

    [Header("물리 설정 (미끄러짐 방지)")]
    [SerializeField] private float linearDrag = 10f; // 값이 높을수록 빨리 멈춤
    [SerializeField] private float angularDrag = 5f;  // 회전 멈춤 속도

    private Rigidbody2D rb;
    private bool isBeingMagnetized = false;
    private float accumulatedForceX = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
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
            // X축 잠금을 풀지만, 속도는 자기력으로만 결정함
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            // 자기력(accumulatedForceX)을 바탕으로 목표 X 속도 계산
            // 이 방식은 플레이어가 부딪혀서 생기는 속도 변화를 매 프레임 덮어써서 무시합니다.
            float targetVelocityX = accumulatedForceX * 0.5f; 
            
            // X축은 자기력으로 제어하고, Y축은 기존 물리(중력 등)를 그대로 유지
            rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
        }
        else
        {
            // 컨베이어 벨트 위에서 움직이게 하기 위해 주석화
            // 자력이 없을 때는 X축을 완전히 고정
            // rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            // rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // 초기화
        isBeingMagnetized = false;
        accumulatedForceX = 0f;
    }
}
