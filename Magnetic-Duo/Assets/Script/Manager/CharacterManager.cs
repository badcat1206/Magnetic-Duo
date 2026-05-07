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
