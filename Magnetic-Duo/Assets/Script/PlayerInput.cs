using UnityEngine;

/// <summary>
/// Horizontal 축·스페이스 입력을 읽어 PlayerMovement에 전달합니다.
/// CharacterManager에서 활성 봇만 이 컴포넌트를 켜 두므로, 비활성 봇은 입력을 받지 않습니다.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    private PlayerMovement movement;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // 이동 입력 처리
        float moveInput = Input.GetAxisRaw("Horizontal");
        movement.Move(moveInput);

        // 점프 입력 처리리
        if (Input.GetKeyDown(KeyCode.Space))
        {
            movement.Jump();
        }
    }
}
