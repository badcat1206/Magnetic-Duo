using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("자력 상호작용 설정")]
    public Polarity polarity; // N, S 중 선택
    [SerializeField] private float requiredForce = 0.1f;

    // 💡 쿨타임 대신, 이번 프레임에 자력을 받았는지 체크하는 변수들
    private bool isMagnetizedThisFrame = false;
    private bool wasMagnetizedLastFrame = false;

    [Header("연결할 장치")]
    [SerializeField] private ConveyorBelt[] connectedBelts;
    [SerializeField] private MagneticField[] connectedMagneticFields;

    private SpriteRenderer spriteRenderer;
    private bool isOn = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 외부(캐릭터)에서 매 프레임 힘을 전달할 때 호출됨
    public void AddMagneticForce(float forceX)
    {
        if (Mathf.Abs(forceX) >= requiredForce)
        {
            isMagnetizedThisFrame = true; // 자력을 받고 있다고 표시
        }
    }

    // 물리 엔진 주기에 맞춰 상태 검사
    void FixedUpdate()
    {
        // 💡 핵심 로직: 이전 프레임에는 자력이 없었는데, 이번 프레임에 새로 들어왔을 때만 '딱 한 번' 작동!
        if (isMagnetizedThisFrame && !wasMagnetizedLastFrame)
        {
            ActivateLever();
        }

        // 다음 프레임을 위해 상태 업데이트 및 초기화
        wasMagnetizedLastFrame = isMagnetizedThisFrame;
        isMagnetizedThisFrame = false;
    }

    public void ActivateLever()
    {
        Debug.Log("레버 당겨짐");
        isOn = !isOn;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = isOn ? Color.green : Color.white;
        }

        if (connectedBelts != null)
        {
            foreach (ConveyorBelt belt in connectedBelts)
            {
                if (belt != null) belt.ToggleGBelt(); // 컨베이어 벨트의 isRunning 상태를 뒤집음
            }
        }

        if (connectedMagneticFields != null)
        {
            foreach (MagneticField field in connectedMagneticFields)
            {
                if (field != null) field.ToggleField();
            }
        }
    }
}