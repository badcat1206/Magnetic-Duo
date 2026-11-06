using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    private PlayerMovement movement;        // 이동 스크립트 연결
    private PlayerInteractor interactor;    // 상호작용 스크립트 연결
    private MagneticAbility magnetic;

    private bool isLocked = false;

    public bool IsDead { get; set; }    // 사망 연출 중 (이때는 맵을 열 수 없음)


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
        if(Time.timeScale == 0f) return;    // ESC 일시정지 중에는 입력 무시

        // 맵이 열려 있으면 조작만 막고 제자리에 세움 (자성·발판·컨베이어 등은 그대로 작동)
        if(MapView.Instance != null && MapView.Instance.IsOpen)
        {
            movement.Move(0);
            return;
        }
        
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
