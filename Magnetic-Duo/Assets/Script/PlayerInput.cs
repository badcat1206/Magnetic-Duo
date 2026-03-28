using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // 이동 스크립트 연결
    private PlayerMovement movement;

    void Awake()
    {
        // 이동 스크립트 연결
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // 이동 입력 처리
        float moveInput = Input.GetAxisRaw("Horizontal");
        movement.Move(moveInput);

        // 점프 입력 처리
        if (Input.GetKeyDown(KeyCode.Space))
        {   
            movement.Jump();
        }
    }
}
