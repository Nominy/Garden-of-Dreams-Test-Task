using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Button : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private GameObject buttonObject;
    
    private bool isPressed = false;
    private bool wasClickedThisFrame = false;
    
    private Image buttonImage;
    private Color originalColor;
    private Color pressedColor;
    
    public bool IsPressed => isPressed;
    public bool WasClicked => wasClickedThisFrame;
    
    void Start()
    {
        buttonObject = gameObject;
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
            pressedColor = originalColor * 0.7f;
        }
    }
    
    void LateUpdate()
    {
        wasClickedThisFrame = false;
    }
    
    void Update()
    {
        if (buttonImage != null)
        {
            buttonImage.color = isPressed ? pressedColor : originalColor;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPressed)
        {
            wasClickedThisFrame = true;
        }
        isPressed = false;
    }
} 