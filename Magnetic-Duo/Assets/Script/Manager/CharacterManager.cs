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
        UpdateDrawOrder();
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

        UpdateDrawOrder();
        OnCharacterSwapped?.Invoke(isNBotActive);
    }

    // 두 봇이 겹칠 때 조작 중인 봇이 앞에 그려지도록 스프라이트(몸체·자석 아이콘)의 z만 살짝 앞으로 당김
    // Order in Layer는 그대로 2 → 레이저(3)·상자(1) 등 다른 오브젝트와의 앞뒤 순서는 변하지 않음
    private const float FrontZ = -0.01f;

    private void UpdateDrawOrder()
    {
        SetSpriteZ(nBotInput, isNBotActive ? FrontZ : 0f);
        SetSpriteZ(sBotInput, isNBotActive ? 0f : FrontZ);
    }

    private static void SetSpriteZ(PlayerInput bot, float z)
    {
        foreach (SpriteRenderer sr in bot.GetComponentsInChildren<SpriteRenderer>(true))
        {
            Vector3 pos = sr.transform.localPosition;
            pos.z = z;
            sr.transform.localPosition = pos;
        }
    }

    public PlayerInput NBotInput => nBotInput;
    public PlayerInput SBotInput => sBotInput;
    public Transform ActiveCharacterTransform =>
        isNBotActive ? nBotInput.transform : sBotInput.transform;
}
