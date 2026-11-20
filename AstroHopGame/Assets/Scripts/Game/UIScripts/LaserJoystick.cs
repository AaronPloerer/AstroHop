using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AimJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    #region Parameters
    [SerializeField] private RectTransform knob;
    [SerializeField] private float knobRange = 60f;                // Area in which knob can be moved
    [SerializeField] private float aimPreviewSmoothing = 10f;      // Higher = more responsive, lower = smoother
    [SerializeField] private float knobSmoothing = 50f;            // Higher = more responsive, lower = smoother
    [SerializeField] private float holdThreshold = 0.5f;           // Seconds before minimum movement check activates
    public float minimumShootingFingerMovement;                    // Finger movement needed for an aimed shot. Less than this = tap (shoots straight up)

    [Header("Knob Sprite Change")]
    [SerializeField] private Image knobImage;                      // The Image whose sprite should change (can be on any object)
    [SerializeField] private Sprite pressedKnobSprite;             // Sprite shown while a finger is on the joystick area

    [Header("Shot Spam Protection")]
    [SerializeField] private float minTimeBetweenShots;             // Minimum seconds between two shots from this joystick (0 = off)
    #endregion

    #region Ruintime State
    private RectTransform outerRect;
    private Sprite normalKnobSprite;
    private bool isPressed = false;
    private Vector2 inputDirection = Vector2.zero;
    private Vector2 smoothedDirection = Vector2.zero;
    private Vector2 initialLocalTouchPosition = Vector2.zero;
    private Vector2 targetKnobPosition = Vector2.zero;
    private float pressTime = 0f;
    private float maxDragDistance = 0f;                            // Furthest the finger got from its initial touch position during this press
    private Vector2 currentLocalDelta = Vector2.zero;              // Raw (unsmoothed) finger offset from the initial touch position
    private float lastShotTime = -999f;
    private bool wasMinimumMet = false;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        // Cache the knob image and its default sprite so we can swap back after a press
        normalKnobSprite = knobImage.sprite;
    }

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

        // Haptic when preview hides due to finger returning to center
        if (wasMinimumMet && !minimumMet && isPressed)
        {
            AndroidHaptics.TriggerHapticFeedback();
        }
        wasMinimumMet = minimumMet;

        if (minimumMet)
        {
            AimPreviewScript.instance.ShowPreview(smoothedDirection);
        }
        else
        {
            AimPreviewScript.instance.HidePreview();
        }
    }
    #endregion

    #region Input Events
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;

        pressTime = 0f;                 // Reset hold timer on each new press
        maxDragDistance = 0f;           // Reset tap detection on each new press
        currentLocalDelta = Vector2.zero;

        SetKnobPressed(true);

        // Record initial touch position in local space — this becomes the new "center"
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            outerRect,
            eventData.position,
            eventData.pressEventCamera,
            out initialLocalTouchPosition
        );

        // Knob is at center on press
        knob.anchoredPosition = Vector2.zero;
        targetKnobPosition = Vector2.zero;
        inputDirection = Vector2.zero;
        smoothedDirection = Vector2.zero;
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

        // Remember how far the finger travelled (before clamping) to tell taps from drags
        maxDragDistance = Mathf.Max(maxDragDistance, localDelta.magnitude);
        currentLocalDelta = localDelta;

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
        // Did the finger ever leave the minimum-movement radius during this press? Is it outside right now?
        bool leftRadius = maxDragDistance > minimumShootingFingerMovement;
        bool outsideNow = currentLocalDelta.magnitude > minimumShootingFingerMovement;

        if (!leftRadius)
        {
            // Never moved enough: short press = tap -> shoot straight up. Long hold = cancelled
            if (pressTime < holdThreshold)
            {
                TryShoot(Vector2.up * knobRange);
            }
        }
        else if (outsideNow && smoothedDirection.magnitude > 0.1f)
        {
            // Aimed shot: finger left the radius and was released outside of it
            TryShoot(smoothedDirection);
        }
        // else: finger left the radius and came back to the center -> cancelled, no shot

        // Reset everything including initial touch position
        isPressed = false;
        maxDragDistance = 0f;
        currentLocalDelta = Vector2.zero;
        inputDirection = Vector2.zero;
        smoothedDirection = Vector2.zero;
        targetKnobPosition = Vector2.zero;
        knob.anchoredPosition = Vector2.zero;
        initialLocalTouchPosition = Vector2.zero;

        SetKnobPressed(false);
    }
    #endregion

    #region Shooting
    private void TryShoot(Vector2 direction)
    {
        // Ignore accidental double taps
        if (Time.unscaledTime - lastShotTime < minTimeBetweenShots) return;
        lastShotTime = Time.unscaledTime;

        LaserSpawnerScript.instance.TryFireInDirection(direction);
    }
    #endregion

    #region Knob Sprite
    private void SetKnobPressed(bool pressed)
    {
        if (knobImage == null) return;

        // Only swap if a pressed sprite was assigned; otherwise keep the normal one
        if (pressed && pressedKnobSprite != null)
        {
            knobImage.sprite = pressedKnobSprite;
        }
        else
        {
            knobImage.sprite = normalKnobSprite;
        }
    }
    #endregion

    public void ForceReset()
    {
        isPressed = false;
        pressTime = 0f;                         // Prevents minimumMet from being true after reset
        maxDragDistance = 0f;
        currentLocalDelta = Vector2.zero;

        inputDirection = Vector2.zero;
        smoothedDirection = Vector2.zero;

        initialLocalTouchPosition = Vector2.zero;
        targetKnobPosition = Vector2.zero;

        knob.anchoredPosition = Vector2.zero;   // Snap directly, no lerp

        SetKnobPressed(false);

        AimPreviewScript.instance.HidePreview();
    }
}