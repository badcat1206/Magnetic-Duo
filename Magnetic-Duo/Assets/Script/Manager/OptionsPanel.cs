using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    [Header("슬라이더")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;

    private float sfxSnapshot;
    private float bgmSnapshot;

    private void Awake()
    {
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
    }

    // 패널이 열릴 때 현재 설정을 스냅샷으로 저장
    private void OnEnable()
    {
        float sfx = AudioManager.Instance != null
            ? AudioManager.Instance.GetSfxVolume()
            : PlayerPrefs.GetFloat("SFXVolume", AudioManager.DefaultVolume);

        float bgm = AudioManager.Instance != null
            ? AudioManager.Instance.GetBgmVolume()
            : PlayerPrefs.GetFloat("BGMVolume", AudioManager.DefaultVolume);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(sfx);
        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(bgm);

        SaveSnapshot();
    }

    public void OnClickOK()
    {
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }

    public void OnClickCancel()
    {
        RestoreSnapshot();
        gameObject.SetActive(false);
    }

    public void OnClickReset()
    {
        if (sfxSlider != null)
            sfxSlider.value = AudioManager.DefaultVolume;
        if (bgmSlider != null)
            bgmSlider.value = AudioManager.DefaultVolume;
    }

    private void OnSfxSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSfxVolume(value);
        else
            PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private void OnBgmSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBgmVolume(value);
        else
            PlayerPrefs.SetFloat("BGMVolume", value);
    }

    private void SaveSnapshot()
    {
        sfxSnapshot = sfxSlider != null ? sfxSlider.value : AudioManager.DefaultVolume;
        bgmSnapshot = bgmSlider != null ? bgmSlider.value : AudioManager.DefaultVolume;
    }

    private void RestoreSnapshot()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(sfxSnapshot);
            AudioManager.Instance.SetBgmVolume(bgmSnapshot);
        }
        else
        {
            PlayerPrefs.SetFloat("SFXVolume", sfxSnapshot);
            PlayerPrefs.SetFloat("BGMVolume", bgmSnapshot);
        }

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(sfxSnapshot);
        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(bgmSnapshot);
    }
}
