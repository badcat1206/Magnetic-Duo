using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Trap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("NBot") || collision.CompareTag("SBot"))
        {
            PlayerInput deadPlayer = collision.GetComponent<PlayerInput>();
            StartCoroutine(DieSequence(deadPlayer));
        }
    }

    private IEnumerator DieSequence(PlayerInput deadPlayer)
    {
        Debug.Log("죽음");

        if(deadPlayer != null)
        {
            deadPlayer.SetLocked(true);
            // 여기에 죽는 애니메이션 넣기
        }

        yield return new WaitForSeconds(1f);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}
