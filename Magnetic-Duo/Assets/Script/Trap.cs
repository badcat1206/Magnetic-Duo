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
    [SerializeField] private Vector2 nBoxSpawnPoint;
    [SerializeField] private Vector2 sBoxSpawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(safeBotTag != "" && collision.CompareTag(safeBotTag)) return;

        if (collision.CompareTag("NBot") || collision.CompareTag("SBot"))
        {
            PlayerInput deadPlayer = collision.GetComponent<PlayerInput>();
            StartCoroutine(DieSequence(deadPlayer));
        }
        else if (canDestroyBox && collision.CompareTag("NBox"))
        {
            RespawnBox(collision.gameObject, nBoxSpawnPoint);
        }
        else if (canDestroyBox && collision.CompareTag("SBox"))
        {
            RespawnBox(collision.gameObject, sBoxSpawnPoint);
        }
    }

    private void RespawnBox(GameObject box, Vector2 spawnPoint)
    {
        if (spawnPoint != Vector2.zero)
        {
            box.transform.position = spawnPoint;

            Rigidbody2D boxRb = box.GetComponent<Rigidbody2D>();
            if (boxRb != null)
            {
                boxRb.linearVelocity = Vector2.zero;
                boxRb.angularVelocity = 0f;
            }
        }
        else
        {
            Debug.LogWarning($"[Trap] '{gameObject.name}'의 {box.name} 리스폰 위치가 (0,0)입니다. Inspector에서 좌표를 설정하세요.", gameObject);
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
