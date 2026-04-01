using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
    [Header("Input 연결")]
    [SerializeField] private PlayerInput nBotInput;
    [SerializeField] private PlayerInput sBotInput;

    private bool isNBotActive = true;
    private MagneticAbility nBotMagnetic;
    private MagneticAbility sBotMagnetic;
    private Animator nBotAnimator;
    private Animator sBotAnimator;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        nBotMagnetic = nBotInput.GetComponent<MagneticAbility>();
        sBotMagnetic = sBotInput.GetComponent<MagneticAbility>();
        nBotAnimator = nBotInput.GetComponent<Animator>();
        sBotAnimator = sBotInput.GetComponent<Animator>();
    }

    void Start()
    {
        nBotInput.enabled = isNBotActive;
        sBotInput.enabled = !isNBotActive;
        
        // 시작 시 현재 상태에 맞춰 애니메이션 설정
        if (nBotAnimator != null) nBotAnimator.SetTrigger(isNBotActive ? "PowerOn" : "PowerOff");
        if (sBotAnimator != null) sBotAnimator.SetTrigger(!isNBotActive ? "PowerOn" : "PowerOff");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)){
            SwapCharacter();
        }
    }

    void SwapCharacter()
    {
        isNBotActive = !isNBotActive;
        nBotInput.enabled = isNBotActive;
        sBotInput.enabled = !isNBotActive;

        // 전원 온/오프 애니메이션 트리거
        if (nBotAnimator != null && sBotAnimator != null)
        {
            if (isNBotActive)
            {
                nBotAnimator.SetTrigger("PowerOn");
                sBotAnimator.SetTrigger("PowerOff");
            }
            else
            {
                nBotAnimator.SetTrigger("PowerOff");
                sBotAnimator.SetTrigger("PowerOn");
            }
        }
    }
}
