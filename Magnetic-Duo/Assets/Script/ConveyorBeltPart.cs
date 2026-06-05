using UnityEngine;

// 컨베이어 벨트 파트 프리팹에 붙이는 컴포넌트.
// 3프레임 스프라이트를 프리팹 안에서 직접 관리한다.
public class ConveyorBeltPart : MonoBehaviour
{
    [Tooltip("frame_001, frame_002, frame_003 순서로 할당")]
    public Sprite[] frames = new Sprite[3];
}
