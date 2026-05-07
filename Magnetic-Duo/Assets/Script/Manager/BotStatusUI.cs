using UnityEngine;
using UnityEngine.UI;

public class BotStatusUI : MonoBehaviour
{
    [Header("nBot UI")]
    [SerializeField] private Image nBotImage;
    [SerializeField] private Sprite nBotOn;
    [SerializeField] private Sprite nBotOff;

    [Header("sBot UI")]
    [SerializeField] private Image sBotImage;
    [SerializeField] private Sprite sBotOn;
    [SerializeField] private Sprite sBotOff;

    void Start()
    {
        CharacterManager.Instance.OnCharacterSwapped += UpdateUI;
        UpdateUI(true);
    }

    void OnDestroy()
    {
        if (CharacterManager.Instance != null)
            CharacterManager.Instance.OnCharacterSwapped -= UpdateUI;
    }

    void UpdateUI(bool isNBotActive)
    {
        nBotImage.sprite = isNBotActive ? nBotOn : nBotOff;
        sBotImage.sprite = isNBotActive ? sBotOff : sBotOn;
    }
}
