using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject optionsPanel;

    public void OnClickPlay()
    {
        StartCoroutine(FadeOutThenLoad(1));
    }

    private IEnumerator FadeOutThenLoad(int sceneBuildIndex)
    {
        if (ScreenFader.Instance != null)
            yield return StartCoroutine(ScreenFader.Instance.FadeOut());
        SceneManager.LoadScene(sceneBuildIndex);
    }

    public void OnClickOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
