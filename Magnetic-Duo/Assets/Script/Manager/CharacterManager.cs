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
    }

    void Start()
    {
        nBotInput.enabled = isNBotActive;
        sBotInput.enabled = !isNBotActive;
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

        // 비활성화된 캐릭터 즉시 정지
        if (isNBotActive)
            sBotInput.GetComponent<PlayerMovement>().StopMovement();
        else
            nBotInput.GetComponent<PlayerMovement>().StopMovement();
    }

    public PlayerInput NBotInput => nBotInput;
    public PlayerInput SBotInput => sBotInput;
    public Transform ActiveCharacterTransform =>
        isNBotActive ? nBotInput.transform : sBotInput.transform;
}
