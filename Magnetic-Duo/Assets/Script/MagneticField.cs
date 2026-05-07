using Unity.VisualScripting;
using UnityEngine;

public class MagneticField : MonoBehaviour
{
   [Header("기본 설정")]
   [SerializeField] private bool isOn = true;

   [Header("연결할 컴포넌트")]
   [SerializeField] private GameObject electricEffect;
   [SerializeField] Collider2D electricCollider;

   private Animator animator;

    void Awake()
    {
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
    }
    void Start()
    {
        animator.SetBool("IsOn", isOn);
        UpdateFieldState();
    }

    public void ToggleField()
    {
        
        isOn = !isOn;
        animator.SetBool("IsOn", isOn);
        UpdateFieldState();
    }

    void UpdateFieldState()
    {
        if(electricEffect != null) electricEffect.SetActive(isOn);
        if(electricCollider != null) electricCollider.enabled = isOn;
    }
}
