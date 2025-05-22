using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InputFieldAnimationScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDeselectHandler
{

    #region Parameters
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite highlightedSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Sprite selectedSprite;

    private Image image;
    private TMP_InputField inputField;
    private bool isHovered;
    private bool isPressed;
    #endregion

    #region Awake
    private void Awake()
    {
        // Get the InputField component
        inputField = GetComponent<TMP_InputField>();

        // Disable Unity's transitions
        inputField.transition = Selectable.Transition.None;

        // Get the Image component (
        image = GetComponent<Image>();

        UpdateAppearance();
    }
    #endregion

    #region Sprite Logic
    // Update sprite based on current state
    private void UpdateAppearance()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            image.sprite = selectedSprite;
        }
        else if (isPressed)
        {
            image.sprite = pressedSprite;
        }
        else if (isHovered)
        {
            image.sprite = highlightedSprite;
        }
        else
        {
            image.sprite = normalSprite;
        }
    }

    // Mouse enters the InputField
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (EventSystem.current.currentSelectedGameObject != gameObject)
        {
            UpdateAppearance();
        }
    }

    // Mouse exits the InputField
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (EventSystem.current.currentSelectedGameObject != gameObject)
        {
            UpdateAppearance();
        }

        // If mouse exits while pressed, force deselect
        if (isPressed)
        {
            DeselectInputField();
        }
    }

    // Mouse button pressed down
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateAppearance();
    }

    // Mouse button released
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        if (isHovered)
        {
            // Select the InputField and show selected sprite
            EventSystem.current.SetSelectedGameObject(gameObject);
            inputField.ActivateInputField();
        }
        UpdateAppearance();
    }

    // InputField loses focus (deselected)
    public void OnDeselect(BaseEventData eventData)
    {
        UpdateAppearance();
    }

    // Force deselect when mouse exits while pressed
    private void DeselectInputField()
    {
        isPressed = false;
        inputField.DeactivateInputField();
        EventSystem.current.SetSelectedGameObject(null);
        UpdateAppearance();
    }
    #endregion
}