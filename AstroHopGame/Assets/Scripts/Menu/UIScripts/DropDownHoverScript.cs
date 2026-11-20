using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
public class SpriteSwitcher : MonoBehaviour
{
    #region Components
    private Image targetImage;

    [Header("Sprites for each state")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite clickedSprite;

    private BoxCollider2D col;
    private TMP_Dropdown dropdown;
    private bool dropdownWasOpenOnPress;
    #endregion

    #region Initialization
    void Start()
    {
        targetImage = GetComponent<Image>();
        col = GetComponent<BoxCollider2D>();

        // Dropdown must be searched in the parents, not on this object.
        dropdown = GetComponentInParent<TMP_Dropdown>();

        SetNormal();
    }

    void OnDisable()
    {
        // Never stay stuck in the pressed look if the object gets hidden while pressed
        dropdownWasOpenOnPress = false;
        if (targetImage != null) SetNormal();
    }
    #endregion

    #region Mouse / Touch Handlers
    void OnMouseDown()
    {
        // Remember if the dropdown was already open when the press started
        dropdownWasOpenOnPress = dropdown != null && dropdown.IsExpanded;

        // Finger/mouse goes down: pressed look
        if (clickedSprite != null)
            targetImage.sprite = clickedSprite;
    }

    void OnMouseExit()
    {
        // Finger/mouse dragged off the button while held: back to normal
        SetNormal();
    }

    void OnMouseUp()
    {
        // Always return to normal on release
        SetNormal();

        // This press closed an open dropdown: no sound
        if (dropdownWasOpenOnPress)
        {
            dropdownWasOpenOnPress = false;
            return;
        }

        // Click sound only if released while still over the button
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (col.OverlapPoint(worldPoint))
        {
            AudioManagerScript.instance.PlaySFX(
                AudioManagerScript.instance.click,
                AudioManagerScript.instance.clickVolume);
        }
    }

    private void SetNormal()
    {
        if (targetImage != null && normalSprite != null)
            targetImage.sprite = normalSprite;
    }
    #endregion
}