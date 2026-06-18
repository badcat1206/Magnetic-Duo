using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject optionsPanel;

    public void OnClickPlay()
    {
        SceneManager.LoadScene(1);
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
