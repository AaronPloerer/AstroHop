using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    #region Variables
    [SerializeField] private float fadeDuration;          // Time until explosion fully disappear
    [SerializeField] private Animator explosionAnim;      // Reference to explosion animatior

    private SpriteRenderer spriteRenderer;                // Reference for visual representation component
    private float timer;                                  // Fade progression tracker
    #endregion

    #region Initialization
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();    // Reference visual representation component
        timer = fadeDuration;                               // Initialize fade timer with fade duration for countdown
    }
    #endregion

    #region Update Methods
    void Update()
    {
        HandleAnimation();
        HandleFadeProcess();
    }
    #endregion

    #region Animation Logic
    private void HandleAnimation()
    {
        // Pause/resume animation based on game state
        explosionAnim.speed = MainGameUIScript.instance.paused ? 0 : 1;
    }
    #endregion

    #region Fade Logic
    private void HandleFadeProcess()
    {
        // Only update timer when game is active
        if (!MainGameUIScript.instance.paused)
        {
            timer -= Time.deltaTime;
        }

        if (timer <= 0)
        {
            // Remove invisible object from scene at the end of the countdown
            Destroy(gameObject);
        }
        else
        {
            // Update alpha based on remaining time and make object invisible at the end of the countdown
            Color color = spriteRenderer.color;
            color.a = timer / fadeDuration;
            spriteRenderer.color = color;
        }
    }
    #endregion
}
