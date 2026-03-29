using UnityEngine;

/// <summary>
/// 두 봇(N-Bot / S-Bot) 중 하나만 조작 가능하게 하고 Tab으로 전환합니다.
/// 싱글톤으로 다른 스크립트에서 Instance로 접근할 수 있습니다.
/// </summary>
public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("Input 연결")]
    [SerializeField] private PlayerInput nBotInput;     // N-Bot 조작 스크립트
    [SerializeField] private PlayerInput sBotInput;     // s-Bot 조작 스크립트
    [Header("캐릭터 애니메이션 연결")]
    [SerializeField] private PlayerAnimation nBotAnimation;     // N-Bot 애니메이션 스크립트
    [SerializeField] private PlayerAnimation sBotAnimation;     // s-Bot 애니메이션 스크립트
    [Header("물리 엔진 연결")]
    [SerializeField] private Rigidbody2D nBotRb;    //충돌 시 밀림 방지를 위한 N-Bot 물리 컴포넌트
    [SerializeField] private Rigidbody2D sBotRb;    //충돌 시 밀림 방지를 위한 s-Bot 물리 컴포넌트

    // true: N-Bot이 조작 대상 / false: S-Bot이 조작 대상
    private bool isNBotActive = true;

    void Awake()
    {
        // 중복 매니저 방지: 씬에 하나만 유지
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 초기: N-Bot만 입력·전원 ON, S-Bot은 OFF
        nBotInput.enabled = isNBotActive;
        sBotInput.enabled = !isNBotActive;
        nBotAnimation.SetPowerState(isNBotActive);
        sBotAnimation.SetPowerState(!isNBotActive);

        // 꺼진 쪽은 powerOff 애니 클립 끝 프레임으로 고정
        // 꺼진 상태로 시작
        sBotAnimation.Anim.Play("ani_nBot_powerOff", 0, 1.0f);
        // 활성 캐릭터는 X 이동 허용(회전만 고정), 비활성은 X까지 고정해 미끄러짐·간섭 완화
        nBotRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        sBotRb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)){
            SwapCharacter();
        }
    }

    /// <summary>Tab 입력 시 조작 대상 봇을 바꾸고 입력·애니·Rigidbody 제약을 맞춥니다.</summary>
    void SwapCharacter()
    {
        isNBotActive = !isNBotActive;
        // 활성 쪽만 입력 허용
        nBotInput.enabled = isNBotActive;
        sBotInput.enabled = !isNBotActive;

        nBotAnimation.SetPowerState(isNBotActive);
        sBotAnimation.SetPowerState(!isNBotActive);

        // 활성: X 이동 가능 / 비활성: X 고정 (Start와 동일 규칙)
        if(isNBotActive)
        {
            nBotRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            sBotRb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }
        else
        {
            nBotRb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
            sBotRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        Debug.Log("조작중인 캐릭터: " + (isNBotActive ? "N-Bot" : "S-Bot"));
    }
}
