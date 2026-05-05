using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    private PlayerMovement movement;        // 이동 스크립트 연결
    private PlayerInteractor interactor;    // 상호작용 스크립트 연결
    private MagneticAbility magnetic;

    private bool isLocked = false;

    

    void Awake()
    {
        // 이동 스크립트 연결
        movement = GetComponent<PlayerMovement>();
        magnetic = GetComponent<MagneticAbility>();
        interactor = GetComponent<PlayerInteractor>();
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

        if(Input.GetKeyDown(KeyCode.E))
        {
            interactor.TrtInteract();
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
