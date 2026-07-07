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
        sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
    }

    // 패널이 열릴 때 현재 설정을 스냅샷으로 저장
    private void OnEnable()
    {
        sfxSlider.SetValueWithoutNotify(AudioManager.Instance.GetSfxVolume());
        bgmSlider.SetValueWithoutNotify(AudioManager.Instance.GetBgmVolume());
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
        sfxSlider.value = AudioManager.DefaultVolume;
        bgmSlider.value = AudioManager.DefaultVolume;
    }

    private void OnSfxSliderChanged(float value)
    {
        AudioManager.Instance.SetSfxVolume(value);
    }

    private void OnBgmSliderChanged(float value)
    {
        AudioManager.Instance.SetBgmVolume(value);
    }

    private void SaveSnapshot()
    {
        sfxSnapshot = sfxSlider.value;
        bgmSnapshot = bgmSlider.value;
    }

    private void RestoreSnapshot()
    {
        AudioManager.Instance.SetSfxVolume(sfxSnapshot);
        AudioManager.Instance.SetBgmVolume(bgmSnapshot);
        sfxSlider.SetValueWithoutNotify(sfxSnapshot);
        bgmSlider.SetValueWithoutNotify(bgmSnapshot);
    }
}
