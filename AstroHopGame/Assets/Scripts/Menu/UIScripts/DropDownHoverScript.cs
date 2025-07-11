using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(BoxCollider2D))]
public class SpriteSwitcher : MonoBehaviour
{
    #region Components
    private Image targetImage;

    [Header("Sprites for each state")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite clickedSprite;

    [Header("Dropdown Reference")]
    [SerializeField] private TMP_Dropdown dropdownToSelect;

    private BoxCollider2D col;
    #endregion

    #region Initialization
    void Start()
    {
        // Configure required components
        targetImage = GetComponent<Image>();
        col = GetComponent<BoxCollider2D>();

        // Srt with normal Image
        if (normalSprite != null)
            targetImage.sprite = normalSprite;
    }
    #endregion

    #region Mouse Interaction Handlers
    void OnMouseEnter()
    {
        // When mouse enters the collider area
        if (hoverSprite != null)
            targetImage.sprite = hoverSprite;
    }

    void OnMouseExit()
    {
        // Only revert to normal if mouse button is not held down when exiting
        if (!Input.GetMouseButton(0))
        {
            if (normalSprite != null)
                targetImage.sprite = normalSprite;
        }
    }

    void OnMouseDown()
    {
        // When mouse button is pressed down
        if (clickedSprite != null)
            targetImage.sprite = clickedSprite;
    }

    void OnMouseUp()
    {
        // Convert mouse position to world point
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Check if that point overlaps our collider
        if (col.OverlapPoint(worldPoint))
        {
            // Still over the image: go back to hover
            if (hoverSprite != null)
            {
                AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
                targetImage.sprite = hoverSprite;
            }
        }
        else
        {
            // Mouse up outside: revert to normal
            if (normalSprite != null)
                targetImage.sprite = normalSprite;
        }

        // Select the dropdown 
        if (dropdownToSelect != null)
        {
            dropdownToSelect.Show();
        }
    }
    #endregion
}
