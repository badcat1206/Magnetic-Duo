using UnityEngine;
using System.Collections.Generic;

public enum ButtonType { N, S }

public class PressureButton : MonoBehaviour
{
    [Header("버튼 타입")]
    [SerializeField] private ButtonType buttonType;

    [Header("버튼 스프라이트")]
    [SerializeField] private Sprite unpressedSprite;
    [SerializeField] private Sprite pressedSprite;

    private SpriteRenderer spriteRenderer;
    private readonly HashSet<Collider2D> objectsOnButton = new();
    public bool IsPressed { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsValidObject(collision))
        {
            objectsOnButton.Add(collision);
            IsPressed = objectsOnButton.Count > 0;
            UpdateSprite();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsValidObject(collision))
        {
            objectsOnButton.Remove(collision);
            IsPressed = objectsOnButton.Count > 0;
            UpdateSprite();
        }
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.sprite = IsPressed ? pressedSprite : unpressedSprite;
    }

    private bool IsValidObject(Collider2D collision)
    {
        if (buttonType == ButtonType.N)
            return collision.CompareTag("NBot") || collision.CompareTag("NBox");
        else
            return collision.CompareTag("SBot") || collision.CompareTag("SBox");
    }
}
