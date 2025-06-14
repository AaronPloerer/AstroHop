using UnityEngine;
using System.Collections;

public class CloudPlatformScript : MonoBehaviour
{
    #region Components/References
    [Header("Behavior Settings")]
    [SerializeField] private float fadeOutTime = 1f;  // Time to fade out completely

    private SpriteRenderer spriteRenderer;            // Reference to the visual component
    private bool isFading;                            // Flag to prevent multiple fade coroutines
    #endregion

    #region Initialization
    private void Start()
    {
        // Get reference to sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    #endregion

    #region Collision Handling
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isFading && IsValidPlayerCollision(collision))
        {
            StartCoroutine(FadeOut());
        }
    }

    private bool IsValidPlayerCollision(Collider2D collision)
    {
        return collision.CompareTag("PlayerFeet") &&
               PlayerControllerScript.instance.rb.linearVelocity.y <= 0.5f;
    }
    #endregion

    #region Fade Logic
    private IEnumerator FadeOut()
    {
        isFading = true;
        float timer = 0f;
        Color startColor = spriteRenderer.color;

        // Gradually reduce alpha over time
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutTime;

            // Create new color with decreasing alpha
            Color newColor = new Color(startColor.r, startColor.g, startColor.b, 1 - progress);
            spriteRenderer.color = newColor;

            yield return null;
        }

        // Ensure complete transparency
        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0);

        // Destroy after fading
        Destroy(gameObject);
    }
    #endregion
}