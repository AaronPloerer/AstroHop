using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnimationScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
{
    #region UI Configuration
    [SerializeField] private bool isDropdownUI;           // Dropdown field needs extra an extra click sound 
    #endregion

    #region Parameters
    private Animator animator;                        // Reference to the button's Animator component
    private bool isHovered;                           // Track if cursor is over the button
    #endregion

    #region Initialization
    void Awake()
    {
        // Get reference to Animator component
        animator = GetComponent<Animator>();
    }
    #endregion

    #region Unity Event Handlers
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;         // Mark cursor as inside button
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;       // Mark cursor as outside button
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        animator.SetBool("Pressed", true);          // Activate pressed animation state
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Play click sound when relaseing mouse on open dropdown click
        if (isDropdownUI && isHovered)
        {
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        }
    }
    #endregion

    #region Animation Control Logic
    private void Update()
    {
        // Reset animation if cursor leaves button during press (unvalid press) to reactivate pressed animation
        if (!isHovered)
        {
            animator.SetBool("Pressed", false);
            if (EventSystem.current != null)    
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
    #endregion
}
