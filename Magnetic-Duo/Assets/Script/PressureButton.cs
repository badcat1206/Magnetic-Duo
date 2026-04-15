using UnityEngine;

public enum ButtonType { N, S }

public class PressureButton : MonoBehaviour
{
    [Header("버튼 타입")]
    [SerializeField] private ButtonType buttonType;

    public bool IsPressed { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsValidObject(collision))
        {
            IsPressed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsValidObject(collision))
        {
            IsPressed = false;
        }
    }

    private bool IsValidObject(Collider2D collision)
    {
        if (buttonType == ButtonType.N)
            return collision.CompareTag("NBot") || collision.CompareTag("NBox");
        else
            return collision.CompareTag("SBot") || collision.CompareTag("SBox");
    }
}
