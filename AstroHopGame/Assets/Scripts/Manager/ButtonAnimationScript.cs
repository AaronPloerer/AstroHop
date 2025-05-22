using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnimationScript : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    #region UI Configuration
    [SerializeField] private bool isToggleUI;             // Always reset Animation when it's a toggle
    #endregion

    #region Parameters
    private Animator animator;          // Reference to the button's Animator component
    private bool isHovered;             // Track if cursor is over the button
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
    #endregion

    #region Animation Control Logic
    private void Update()
    {
        // Reset animation if cursor left button during press (unvalid press) to be able to reactivate pressed animation OR instantly when it's a toggle
        if (!isHovered || isToggleUI)
        {
            animator.SetBool("Pressed", false);
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
    #endregion
}
