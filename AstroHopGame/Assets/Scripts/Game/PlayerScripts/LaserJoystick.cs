using UnityEngine;
using UnityEngine.EventSystems;

public class AimJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform knob;
    [SerializeField] private float knobRange = 60f;                // Area in which knob can be moved
    [SerializeField] private float aimPreviewSmoothing = 10f;      // Higher = more responsive, lower = smoother
    [SerializeField] private float knobSmoothing = 50f;            // Higher = more responsive, lower = smoother
    [SerializeField] private float holdThreshold = 0.5f;           // Seconds before minimum movement check activates
    public float minimumShootingFingerMovement;                    // How much movement is needed to be able to shoot

    private RectTransform outerRect;
    private bool isPressed = false;
    private Vector2 inputDirection = Vector2.zero;
    private Vector2 smoothedDirection = Vector2.zero;
    private Vector2 initialLocalTouchPosition = Vector2.zero;
    private Vector2 targetKnobPosition = Vector2.zero;
    private float pressTime = 0f;

    void Start()
    {
        outerRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (isPressed)
        {
            // Track how long joystick has been held
            pressTime += Time.deltaTime;

            // Update knob position
            knob.anchoredPosition = Vector2.Lerp(knob.anchoredPosition, targetKnobPosition, knobSmoothing * Time.deltaTime);
        }

        // Only show aiming preview if shooting is possible
        bool minimumMet = (pressTime > 0.0f && pressTime < holdThreshold)
                  || smoothedDirection.magnitude > minimumShootingFingerMovement;

        if (minimumMet)
        {
            AimPreviewScript.instance.ShowPreview(smoothedDirection);
        }
        else
        {
            AimPreviewScript.instance.HidePreview();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;

        pressTime = 0f;                 // Reset hold timer on each new press

        // Record initial touch position in local space — this becomes the new "center"
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            outerRect,
            eventData.position,
            eventData.pressEventCamera,
            out initialLocalTouchPosition
        );

        // Knob is at center on press
        knob.anchoredPosition = Vector2.zero;
        inputDirection = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Calculate delta from initial touch position 
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            outerRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 currentLocalPosition
        );

        Vector2 localDelta = currentLocalPosition - initialLocalTouchPosition;

        // Clamp knob within outer circle radius
        if (localDelta.magnitude > knobRange)
            localDelta = localDelta.normalized * knobRange;

        targetKnobPosition = localDelta;

        // Smoothly give direction to aimpreview
        smoothedDirection = Vector2.Lerp(smoothedDirection, localDelta, aimPreviewSmoothing * Time.deltaTime);
        inputDirection = smoothedDirection;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // On quick tap: always shoot if there is any direction
        // On long hold: only shoot if finger moved enough

        bool canShoot = pressTime < holdThreshold
            ? smoothedDirection.magnitude > 0.1f
            : smoothedDirection.magnitude > minimumShootingFingerMovement;

        if (canShoot)
        {
            LaserSpawnerScript.instance.TryFireInDirection(smoothedDirection);
        }

        // Reset everything including initial touch position
        isPressed = false;
        inputDirection = Vector2.zero;
        smoothedDirection = Vector2.zero;
        knob.anchoredPosition = Vector2.zero;
        initialLocalTouchPosition = Vector2.zero;
    }
}