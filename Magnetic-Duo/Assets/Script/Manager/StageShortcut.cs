using UnityEngine;
using UnityEngine.SceneManagement;

// 빌드된 게임에서만 작동하는 스테이지 바로가기 단축키 (Ctrl + 키). 씬에 배치하지 않아도 자동 생성됨
public class StageShortcut : MonoBehaviour
{
#if !UNITY_EDITOR
    private static readonly (KeyCode key, string sceneName)[] shortcuts =
    {
        (KeyCode.Alpha1, "Stage1-1"),
        (KeyCode.Alpha2, "Stage1-2"),
        (KeyCode.Alpha3, "Stage1-3"),
        (KeyCode.Alpha4, "Stage2-1"),
        (KeyCode.Alpha5, "Stage2-2"),
        (KeyCode.Alpha6, "Stage2-3"),
        (KeyCode.Alpha7, "Stage2-4"),
        (KeyCode.Alpha8, "Stage3-1"),
        (KeyCode.Alpha9, "Stage3-2"),
        (KeyCode.Alpha0, "Stage3-3"),
        (KeyCode.Q, "Stage3-4"),
        (KeyCode.W, "Stage3-5"),
        (KeyCode.E, "Stage4-1"),
        (KeyCode.R, "Stage4-2"),
        (KeyCode.T, "Stage4-3"),
        (KeyCode.Y, "Stage4-4"),
        (KeyCode.U, "Stage5-1"),
        (KeyCode.I, "Stage5-2"),
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("StageShortcut");
        go.AddComponent<StageShortcut>();
        DontDestroyOnLoad(go);
    }

    private void Update()
    {
        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) return;

        foreach (var (key, sceneName) in shortcuts)
        {
            if (Input.GetKeyDown(key))
            {
                Time.timeScale = 1f; // 일시정지 중에 눌러도 정상 속도로 시작
                SceneManager.LoadScene(sceneName);
                return;
            }
        }
    }
#endif
}
