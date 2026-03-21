using UnityEngine;

public class AspectPositionScript : MonoBehaviour
{
    [SerializeField] private float xNarrow;              // X position on narrow screens (16:9)
    [SerializeField] private float xWide;                // X position on wide screens (19.5:9)

    void Start()
    {
        ApplyPositions();       // Run once at startup when screen size is known
    }

    void ApplyPositions()
    {
        // Aspect ratio: width/height. Since game is horizontal, wider screen = taller phone
        float aspect = (float)Screen.width / Screen.height;

        // Convert aspect ratio to 0-1: 0 = narrow (16:9), 1 = wide (21:9), smoothly interpolated in between
        float t = Mathf.InverseLerp(16f / 9f, 21f / 9f, aspect);

        RectTransform rect = GetComponent<RectTransform>();            // Get this element's own RectTransform
        Vector2 pos = rect.anchoredPosition;                           // Get current position
        pos.x = Mathf.Lerp(xNarrow, xWide, t);                        // Interpolate X between narrow and wide values based on screen ratio
        rect.anchoredPosition = pos;
    }
}