using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class AspectRatioLetterbox : MonoBehaviour
{
    [Tooltip("Minimum supported aspect ratio (width/height). 19/9 ≈ 2.111")]
    public float targetAspect = 19f / 9f;

    public Color barColor = Color.black;

    private Camera cam;
    private RectTransform topBar, bottomBar;

    void Awake()
    {
        cam = GetComponent<Camera>();
        CreateLetterboxBars();
        UpdateLayout();
    }

    void CreateLetterboxBars()
    {
        GameObject canvasObj = new GameObject("LetterboxCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // draw on top of everything

        topBar = CreateBar(canvasObj.transform, "TopBar");
        bottomBar = CreateBar(canvasObj.transform, "BottomBar");
    }

    RectTransform CreateBar(Transform parent, string name)
    {
        GameObject barObj = new GameObject(name);
        barObj.transform.SetParent(parent, false);

        Image img = barObj.AddComponent<Image>();
        img.color = barColor;

        RectTransform rt = barObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);

        return rt;
    }

    void UpdateLayout()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = cam.rect;

        if (scaleHeight < 1f)
        {
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0f;
            rect.y = (1f - scaleHeight) / 2f;

            float barHeightPixels = Screen.height * ((1f - scaleHeight) / 2f);

            // Bottom bar: sits at y = 0
            bottomBar.anchorMin = new Vector2(0f, 0f);
            bottomBar.anchorMax = new Vector2(1f, 0f);
            bottomBar.sizeDelta = new Vector2(0f, barHeightPixels);
            bottomBar.anchoredPosition = Vector2.zero;

            // Top bar: sits at y = Screen.height, pivoted so it grows downward from top
            topBar.anchorMin = new Vector2(0f, 1f);
            topBar.anchorMax = new Vector2(1f, 1f);
            topBar.pivot = new Vector2(0.5f, 1f);
            topBar.sizeDelta = new Vector2(0f, barHeightPixels);
            topBar.anchoredPosition = Vector2.zero;
        }
        else
        {
            rect.width = 1f;
            rect.height = 1f;
            rect.x = 0f;
            rect.y = 0f;

            topBar.sizeDelta = Vector2.zero;
            bottomBar.sizeDelta = Vector2.zero;
        }

        cam.rect = rect;
    }
}