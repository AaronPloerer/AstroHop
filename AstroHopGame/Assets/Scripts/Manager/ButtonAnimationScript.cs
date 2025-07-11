using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;            // for Button

public class ButtonAnimationScript : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    #region Parameters
    private Animator animator;                        // Reference to the button's Animator component
    private bool isHovered;                           // Track if cursor is over the button 
    private Button _button;                           // Unity UI Button component
    #endregion

    #region Initialization
    void Awake()
    {
        // Get reference to Animator component
        animator = GetComponent<Animator>();

        // Get reference to Button component
        _button = GetComponent<Button>();
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

    public void OnPointerUp(PointerEventData eventData)
    {
        // Always activate button
        if (_button != null)
            _button.onClick.Invoke();
    }
    #endregion
}
