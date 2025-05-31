using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private Image joystickBackground;
    [SerializeField] private Image joystickHandle;
    [SerializeField] private float deadZone = 0.1f;
    
    private Vector2 inputVector;
    private bool isDragging = false;
    private Canvas parentCanvas;
    
    public Vector2 InputDirection => inputVector;
    public bool IsActive => isDragging;
    
    void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // Check if the touch is within the joystick background bounds
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground.rectTransform, 
            eventData.position, 
            parentCanvas.worldCamera, 
            out localPoint))
        {
            // Check if the local point is within the circular bounds of the joystick
            float radius = joystickBackground.rectTransform.sizeDelta.x / 2;
            if (Vector2.Distance(Vector2.zero, localPoint) <= radius)
            {
                isDragging = true;
                OnDrag(eventData);
            }
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground.rectTransform, 
            eventData.position, 
            parentCanvas.worldCamera, 
            out position);
        
        float radius = joystickBackground.rectTransform.sizeDelta.x / 2;
        position = Vector2.ClampMagnitude(position, radius);
        joystickHandle.rectTransform.anchoredPosition = position;
        
        // Calculate input vector with dead zone
        Vector2 rawInput = position / radius;
        inputVector = rawInput.magnitude > deadZone ? rawInput : Vector2.zero;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        inputVector = Vector2.zero;
        joystickHandle.rectTransform.anchoredPosition = Vector2.zero;
    }
}