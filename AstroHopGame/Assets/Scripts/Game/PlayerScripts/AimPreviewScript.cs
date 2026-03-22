using UnityEngine;
using System.Collections.Generic;

public class AimPreviewScript : MonoBehaviour
{
    #region Singleton
    public static AimPreviewScript instance;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }
    #endregion

    #region Settings
    [Header("Preview Settings")]
    [SerializeField] private Sprite aimDotSprite;                                   // Same sprite as the real laser
    [SerializeField] private float dotSpacing;                                     // Distance between each preview dot
    [SerializeField] private int maxDots;                                          // Max dots before stopping (prevents infinite line)
    [SerializeField] private float transparency;                                   // Transparency of the aiming dots
    [SerializeField] private LayerMask ufoLayer;                                   // Layer mask for UFO collision detection
    [SerializeField] private string sortingLayer; 
    [SerializeField] private int sortingOrder;                                    
    #endregion

    #region Runtime State
    private List<GameObject> dots = new List<GameObject>();                  // All active preview dots
    #endregion

    #region Line Logic

    // Called by AimJoystick every frame while knob is held
    public void ShowPreview(Vector2 aimDirection)
    {
        ClearDots();

        if (aimDirection.magnitude < 0.1f) return;      // No direction yet, show nothing

        Vector2 origin = transform.position;
        Vector2 direction = aimDirection.normalized;

        // Raycast to find if a UFO is in the way and how far
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, dotSpacing * maxDots, ufoLayer);
        float maxDistance = hit.collider != null ? hit.distance : dotSpacing * maxDots;  // Stop at UFO or max distance

        // Place dots along the aim line
        for (int i = 1; i <= maxDots; i++)
        {
            float distance = i * dotSpacing;

            if (distance > maxDistance) break;          // Stop before UFO or at max range

            Vector2 dotPosition = origin + direction * distance;
            SpawnDot(dotPosition);
        }
    }

    // Called by AimJoystick on release to clean up
    public void HidePreview()
    {
        ClearDots();
    }

    #endregion

    #region ´Laser Sprites Logic

    private void SpawnDot(Vector2 position)
    {
        // Create dot with name "AimDt"
        GameObject dot = new GameObject("AimDot");
        dot.transform.position = position;

        // Add and configure sprite renderer
        SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
        sr.sprite = aimDotSprite;
        sr.color = new Color(1f, 1f, 1f, transparency);
        sr.sortingLayerName = sortingLayer;          
        sr.sortingOrder = sortingOrder;           // Render on on right layer

        dots.Add(dot);
    }

    private void ClearDots()
    {
        // Destroy all existing preview dots
        foreach (GameObject dot in dots)
        {
            if (dot != null) Destroy(dot);
        }
        dots.Clear();
    }

    #endregion
}