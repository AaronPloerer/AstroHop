using UnityEngine;
using UnityEngine.EventSystems;

public class AimJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Components")]
    [SerializeField] private RectTransform knob;
    [SerializeField] private float knobRange = 60f;

    private RectTransform outerRect;
    private Vector2 inputDirection = Vector2.zero;
    private bool isPressed = false;

    void Start()
    {
        outerRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Show and update aim preview always when pressed
        if (isPressed)
        {
            AimPreviewScript.instance.ShowPreview(inputDirection); 
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateKnob(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateKnob(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Hide preview line
        AimPreviewScript.instance.HidePreview();

        // Shoot in the direction the joystick was pointing on release
        if (inputDirection.magnitude > 0.1f)
        {
            LaserSpawnerScript.instance.TryFireInDirection(inputDirection);
        }

        // Reset knob to center
        isPressed = false;
        inputDirection = Vector2.zero;
        knob.anchoredPosition = Vector2.zero;
    }

    private void UpdateKnob(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            outerRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        // Clamp knob within outer circle radius
        if (localPoint.magnitude > knobRange)
            localPoint = localPoint.normalized * knobRange;

        knob.anchoredPosition = localPoint;

        // Normalize to get direction (-1 to 1)
        inputDirection = localPoint / knobRange;
    }
}