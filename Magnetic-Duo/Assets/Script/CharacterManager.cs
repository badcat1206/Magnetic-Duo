using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
    [Header("Input 연결")]
    [SerializeField] private PlayerInput nBotInput;
    [SerializeField] private PlayerInput sBotInput;
    [Header("캐릭터 애니메이션 연결")]
    [SerializeField] private PlayerAnimation nBotAnimation;
    [SerializeField] private PlayerAnimation sBotAnimation;
    [Header("물리 엔진 연결")]
    [SerializeField] private Rigidbody2D nBotRb;
    [SerializeField] private Rigidbody2D sBotRb;

    private bool isNBotActive = true;

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
    }

    void Start()
    {
        nBotInput.enabled = isNBotActive;
        sBotInput.enabled = !isNBotActive;
        nBotAnimation.SetPowerState(isNBotActive);
        sBotAnimation.SetPowerState(!isNBotActive);

        sBotAnimation.Anim.Play("ani_nBot_powerOff", 0, 1.0f);
        nBotRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        sBotRb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
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

        nBotAnimation.SetPowerState(isNBotActive);
        sBotAnimation.SetPowerState(!isNBotActive);

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
