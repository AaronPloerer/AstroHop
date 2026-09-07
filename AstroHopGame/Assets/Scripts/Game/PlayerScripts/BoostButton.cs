using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class BoostButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Image to swap")]
    public Image targetImage;       // assign the button's Image component in Inspector
    public Sprite normalSprite;     // sprite when not pressed
    public Sprite pressedSprite;    // sprite when pressed

    [Header("Cooldown")]
    [SerializeField] private float boostPressCooldown; // unscaled seconds
    private float lastBoostPressTime = -999f;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Ignore new presses while paused or game over
        if (MainGameUIScript.instance.paused || MainGameUIScript.instance.gameOverPanel.activeSelf) return;

        // Prevent accidental rapid re-press
        if (Time.unscaledTime - lastBoostPressTime < boostPressCooldown) return;
        lastBoostPressTime = Time.unscaledTime;

        // Button pressed — start boosting
        PlayerControllerScript.instance.boostButtonDown = true;
        if (targetImage != null && pressedSprite != null)
        {
            targetImage.sprite = pressedSprite;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PlayerControllerScript.instance.boostButtonDown = false;
        if (targetImage != null && normalSprite != null)
        {
            targetImage.sprite = normalSprite;
        }
    }

    public void ForceRelease()
    {
        PlayerControllerScript.instance.boostButtonDown = false;
        if (targetImage != null && normalSprite != null)
        {
            targetImage.sprite = normalSprite;
        }
    }
}