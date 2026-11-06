using System;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
    [Header("Input 연결")]
    [SerializeField] private PlayerInput nBotInput;
    [SerializeField] private PlayerInput sBotInput;

    public event Action<bool> OnCharacterSwapped;

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
        OnCharacterSwapped?.Invoke(isNBotActive);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;    // ESC 일시정지 중에는 캐릭터 교체 막음
        if (MapView.Instance != null && MapView.Instance.IsOpen) return;    // 맵이 열려 있는 동안에도 막음

        if (Input.GetKeyDown(KeyCode.Tab)){
            SwapCharacter();
        }
    }

    void SwapCharacter()
    {
        isNBotActive = !isNBotActive;
        nBotInput.enabled = isNBotActive;
        sBotInput.enabled = !isNBotActive;

        if (isNBotActive)
            sBotInput.GetComponent<PlayerMovement>().StopMovement();
        else
            nBotInput.GetComponent<PlayerMovement>().StopMovement();

        OnCharacterSwapped?.Invoke(isNBotActive);
    }

    public PlayerInput NBotInput => nBotInput;
    public PlayerInput SBotInput => sBotInput;
    public Transform ActiveCharacterTransform =>
        isNBotActive ? nBotInput.transform : sBotInput.transform;
}
