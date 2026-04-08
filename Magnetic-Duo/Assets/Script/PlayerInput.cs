using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // 이동 스크립트 연결
    private PlayerMovement movement;
    private MagneticAbility magnetic;

    private bool isLocked = false;

    void Awake()
    {
        // 이동 스크립트 연결
        movement = GetComponent<PlayerMovement>();
        magnetic = GetComponent<MagneticAbility>();
    }

    void Update()
    {
        if(isLocked) return;
        
        // 이동 입력 처리
        float moveInput = Input.GetAxisRaw("Horizontal");
        movement.Move(moveInput);

        // 점프 입력 처리
        if (Input.GetKeyDown(KeyCode.Space))
        {   
            movement.Jump();
        }

        // 자기력 활성화 처리
        if (Input.GetKeyDown(KeyCode.Q))
        {
            magnetic.ToggleMagnetic();
        }
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        if(isLocked && movement != null)
        {
            movement.Move(0);
        }
    }
}
