using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class Trap : MonoBehaviour
{
    [Header("트랩 설정")]
    [SerializeField] private bool isElectric = false;
    [Tooltip("적힌 태그의 봇은 닿아도 죽지 않음")]
    [SerializeField] private string safeBotTag = "";

    [Space]
    [Header("상자 리스폰 설정")]
    [Tooltip("체크하면 이 트랩에 닿은 상자가 리스폰됩니다.")]
    [SerializeField] private bool canDestroyBox = true;
    [SerializeField] private Vector2 boxSpawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(safeBotTag != "" && collision.CompareTag(safeBotTag)) return;

        if (collision.CompareTag("NBot") || collision.CompareTag("SBot"))
        {
            PlayerInput deadPlayer = collision.GetComponent<PlayerInput>();
            StartCoroutine(DieSequence(deadPlayer));
        }
        else if (canDestroyBox &&
            (collision.CompareTag("NBox") || collision.CompareTag("SBox")))
        {
            RespawnBox(collision.gameObject);
        }
    }

    private void RespawnBox(GameObject box)
    {
        if (boxSpawnPoint != null)
        {
            box.transform.position = boxSpawnPoint;

            Rigidbody2D boxRb = box.GetComponent<Rigidbody2D>();
            if (boxRb != null)
            {
                boxRb.linearVelocity = Vector2.zero;
                boxRb.angularVelocity = 0f;
            }
        }
        else
        {
            Debug.LogWarning("상자 리스폰 위치가 할당되지 않음.");
        }
    }

    private IEnumerator DieSequence(PlayerInput deadPlayer)
    {
        Debug.Log($"[Trap] DieSequence 시작 / isElectric={isElectric} / deadPlayer={deadPlayer}");

        if (deadPlayer != null)
        {
            deadPlayer.SetLocked(true);

            Rigidbody2D rb = deadPlayer.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }

            PlayerAnimation playerAnim = deadPlayer.GetComponent<PlayerAnimation>();
            Debug.Log($"[Trap] playerAnim={playerAnim}");
            if (playerAnim != null)
            {
                playerAnim.TriggerDeath(isElectric);
            }
        }
        else
        {
            Debug.LogWarning("[Trap] deadPlayer가 null - PlayerInput 컴포넌트를 찾지 못함");
        }

        yield return new WaitForSeconds(1.0f);

        if(ScreenFader.Instance != null)
        {
            yield return StartCoroutine(ScreenFader.Instance.FadeOut());
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}
