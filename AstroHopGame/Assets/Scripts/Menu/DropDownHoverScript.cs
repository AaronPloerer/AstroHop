using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
public class SpriteSwitcher : MonoBehaviour
{
    #region Components
    private Image targetImage;

    [Header("Sprites for each state")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite clickedSprite;

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
        // When mouse exits the collider area
        if (normalSprite != null)
            targetImage.sprite = normalSprite;
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
                targetImage.sprite = hoverSprite;
        }
        else
        {
            // Mouse up outside: revert to normal
            if (normalSprite != null)
                targetImage.sprite = normalSprite;
        }
    }
    #endregion
}
