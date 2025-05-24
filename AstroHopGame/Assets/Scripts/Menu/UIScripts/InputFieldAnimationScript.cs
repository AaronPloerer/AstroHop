using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InputFieldAnimationScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    #region Parameters
    [SerializeField] private Sprite normalSprite;        // Default state visual
    [SerializeField] private Sprite highlightedSprite;   // Hover state visual
    [SerializeField] private Sprite pressedSprite;       // Clicked state visual
    [SerializeField] private GameObject text;            // Text asking to enter key

    private Image image;                  // Reference to UI Image component
    private TMP_InputField inputField;    // Reference to TextMeshPro InputField
    private bool isHovered;               // Track mouse hover state
    private bool isPressed;               // Track mouse press state
    #endregion

    #region Initialization
    private void Awake()
    {
        // Get component references
        inputField = GetComponent<TMP_InputField>();
        image = GetComponent<Image>();

        // Disable Unity's default transitions
        inputField.transition = Selectable.Transition.None;

        // Set initial visual state
        UpdateAppearance();
    }
    #endregion

    #region Visual State Management
    // Updates the input field's appearance based on current state
    // Priority: Pressed > Hovered > Normal
    private void UpdateAppearance()
    {
        if (isPressed)
        {
            image.sprite = pressedSprite;
            text.SetActive(true);
        }
        else if (isHovered)
        {
            image.sprite = highlightedSprite;
            text.SetActive(false);
        }
        else
        {
            image.sprite = normalSprite;
            text.SetActive(false);
        }
    }

    // Called when mouse enters input field area
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        // Only update appearance if this isn't the currently selected field
        if (MenuUIScript.instance.selectedInputField != inputField)
            UpdateAppearance();
    }

    // Called when mouse exits input field area
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        // Only update appearance if this isn't the currently selected field
        if (MenuUIScript.instance.selectedInputField != inputField)
            UpdateAppearance();
    }

    // Called when mouse button is pressed down
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // Claim focus as the currently selected input field
        MenuUIScript.instance.selectedInputField = inputField;

        UpdateAppearance();
    }

    // Called when mouse button is released
    public void OnPointerUp(PointerEventData eventData)
    {
        // If releasing mouse button outside the field, clear selection
        if (!isHovered)
        {
            MenuUIScript.instance.selectedInputField = null;
        }
        else
        {
            isPressed = true;

            // Play interaction sound
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        }
        UpdateAppearance();
    }

    private void Update()
    {
        // Update pressed state if another input field is selected
        if (isPressed && MenuUIScript.instance.selectedInputField != inputField)
        {
            isPressed = false;
            UpdateAppearance();
        }

        // Deselect if clicking outside the selected input field
        if (Input.GetMouseButtonDown(0) && MenuUIScript.instance.selectedInputField == inputField && !isHovered)
        {
            MenuUIScript.instance.selectedInputField = null;
        }
    }
    #endregion
}