using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
   [Header("도착 지점 연결")]
   [SerializeField] private Goal nBotGoal;
   [SerializeField] private Goal sBotGoal;

   private bool isClearing = false;

    private void Update()
    {
        if(isClearing) return;

        if(nBotGoal.IsReached && sBotGoal.IsReached)
        {
            ClearStage();
        }
    }

    private void ClearStage()
    {
        isClearing = true;
        Debug.Log("Stage Cleared");

        if(CharacterManager.Instance != null)
        {
            CharacterManager.Instance.enabled = false;
            // 도착하면 입력 잠금
            // CharacterManager.Instance.NBotInput.SetLocked(true);
            // CharacterManager.Instance.SBotInput.SetLocked(true);
        }

        Invoke("LoadNextStage", 1.5f);
    }

    void LoadNextStage()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if(nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("Last Stage");
        }
    }

}
