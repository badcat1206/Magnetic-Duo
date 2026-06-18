using UnityEngine;

public class OptionsPanel : MonoBehaviour
{
    // 패널이 열릴 때 현재 설정을 스냅샷으로 저장
    private void OnEnable()
    {
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

    private void SaveSnapshot()
    {
        // 나중에 설정 항목이 추가되면 여기서 스냅샷 저장
    }

    private void RestoreSnapshot()
    {
        // 나중에 설정 항목이 추가되면 여기서 스냅샷 복원
    }
}
