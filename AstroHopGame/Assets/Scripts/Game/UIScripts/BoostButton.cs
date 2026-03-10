using UnityEngine;
using UnityEngine.EventSystems;

public class BoostButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // Button pressed — start boosting
        PlayerControllerScript.instance.boostButtonDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Button released — stop boosting
        PlayerControllerScript.instance.boostButtonDown = false;
    }
}