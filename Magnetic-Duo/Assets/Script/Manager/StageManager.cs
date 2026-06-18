using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageManager : MonoBehaviour
{
    [Header("도착 지점 연결")]
    [SerializeField] private Goal nBotGoal;
    [SerializeField] private Goal sBotGoal;

    [Header("시작 연출 설정")]
    [SerializeField] private float startWalkTime = 1f;

    private bool isClearing = false;
    private void Start()
    {
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.enabled = false;
            CharacterManager.Instance.NBotInput.SetLocked(true);
            CharacterManager.Instance.SBotInput.SetLocked(true);

            PlayerMovement nMove = CharacterManager.Instance.NBotInput.GetComponent<PlayerMovement>();
            PlayerMovement sMove = CharacterManager.Instance.SBotInput.GetComponent<PlayerMovement>();

            // 컷신 모드
            PlayerAnimation nAnim = nMove.GetComponent<PlayerAnimation>();
            PlayerAnimation sAnim = sMove.GetComponent<PlayerAnimation>();
            if (nAnim != null) nAnim.SetCutsceneMode(true);
            if (sAnim != null) sAnim.SetCutsceneMode(true);

            // 오른쪽으로 강제 걷기 시작
            nMove.Move(1f);
            sMove.Move(1f);

            // 1초동안 뚜벅뚜벅 걷기
            yield return new WaitForSeconds(startWalkTime);

            // 목표 지점 도착 시 정지
            nMove.StopMovement();
            sMove.StopMovement();

            if (nAnim != null) nAnim.SetCutsceneMode(false);
            if (sAnim != null) sAnim.SetCutsceneMode(false);

            // 조작 잠금 해제
            CharacterManager.Instance.NBotInput.SetLocked(false);
            CharacterManager.Instance.SBotInput.SetLocked(false);
            CharacterManager.Instance.enabled = true;
        }
    }

    private void Update()
    {
        if (isClearing) return;

        if (nBotGoal.IsReached && sBotGoal.IsReached)
        {
            ClearStage();
        }
    }

    private void ClearStage()
    {
        isClearing = true;
        Debug.Log("Stage Cleared - 연출 시작");

        StartCoroutine(ClearSequence());
    }

    private IEnumerator ClearSequence()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.enabled = false;
            CharacterManager.Instance.NBotInput.SetLocked(true);
            CharacterManager.Instance.SBotInput.SetLocked(true);

            PlayerMovement nMove = CharacterManager.Instance.NBotInput.GetComponent<PlayerMovement>();
            PlayerAnimation nAnim = nMove.GetComponent<PlayerAnimation>();

            PlayerMovement sMove = CharacterManager.Instance.SBotInput.GetComponent<PlayerMovement>();
            PlayerAnimation sAnim = sMove.GetComponent<PlayerAnimation>();

            if (nAnim != null) nAnim.SetCutsceneMode(true);
            if (sAnim != null) sAnim.SetCutsceneMode(true);

            if (nAnim != null) nAnim.SetClearFace(true);
            if (sAnim != null) sAnim.SetClearFace(true);

            yield return new WaitForSeconds(2f);

            nMove.Move(1f);
            sMove.Move(1f);
        }

        yield return new WaitForSeconds(1.5f);

        if (ScreenFader.Instance != null)
        {
            yield return StartCoroutine(ScreenFader.Instance.FadeOut());
        }

        LoadNextStage();
    }

    void LoadNextStage()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("Last Stage");
        }
    }

}
