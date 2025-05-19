using UnityEngine;
using System.Collections;

public class LaserSpawnerScript : MonoBehaviour
{
    #region Singleton
    public static LaserSpawnerScript instance;

    void Awake()
    {
        // Maintain single instance
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    #endregion

    #region Settings and Components
    [Header("Settings")]
    [SerializeField] private float laserTimer;          // Cooldown between shots
    [SerializeField] private float indicatorDuration;  // How long shot indicators stay visible

    [Header("Components")]
    [SerializeField] private LaserScript laserPrefab;  // Laser prefab to spawn
    [SerializeField] private Transform spawnPoint;         // Where lasers emerge from
    [SerializeField] private GameObject laserIndicator;    // Visual shot indicator: Light on the atronaut's helmet
    #endregion

    #region Runtime State
    private float laserCurrentTime;     // Time since last shot
    private SpriteRenderer laserRenderer;  // Astronaut indicator's sprite component
    private Coroutine indicatorCoroutine;  // Reference to indicator duration
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeLaserSystem();
    }

    private void Update()
    {
        // Main update loop
        if (ShouldProcessInput())
        {
            UpdateLaserCooldown();
            HandleLaserInput();
        }
    }
    #endregion

    #region Initialization
    private void InitializeLaserSystem()
    {
        // Get components and set initial values
        laserRenderer = laserIndicator.GetComponent<SpriteRenderer>();
        laserRenderer.enabled = false;
        laserCurrentTime = 0;
    }
    #endregion

    #region Input Handling
    private bool ShouldProcessInput()
    {
        // Check if game allows shooting
        return PlayerControllerScript.instance.isAlive && !MainGameUIScript.instance.paused && !PlayerControllerScript.instance.startingBoost;
    }

    private void UpdateLaserCooldown()
    {
        // Track time since last shot
        laserCurrentTime += Time.deltaTime;
    }

    private void HandleLaserInput()
    {
        // Process mouse click input
        if (Input.GetMouseButtonDown(0) && laserCurrentTime > laserTimer)
        {
            Vector2 clickPosition = GetMouseWorldPosition2D();
            if (MainGameUIScript.instance.validClickArea.OverlapPoint(clickPosition))
            {
                FireLaser(clickPosition);
                ResetLaserCooldown();
                ShowLaserIndicator();
            }
        }
    }

    private Vector2 GetMouseWorldPosition2D()
    {
        // Convert screen click to game world position
        Vector3 mousePos = Input.mousePosition;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
    #endregion

    #region Laser Operations
    private void FireLaser(Vector2 targetPosition)
    {
        // Create and launch new laser projectile
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.laserShot, AudioManagerScript.instance.laserShotVolume);
        LaserScript newLaser = Instantiate(laserPrefab, spawnPoint.position, Quaternion.identity);
        newLaser.InitializeDirection(targetPosition);
    }

    private void ResetLaserCooldown()
    {
        // Reset shot timer
        laserCurrentTime = 0;
    }
    #endregion

    #region Indicator Management
    private void ShowLaserIndicator()
    {
        // Start/restart indicator display
        if (indicatorCoroutine != null)
            StopCoroutine(indicatorCoroutine);

        indicatorCoroutine = StartCoroutine(IndicatorDuration());
    }

    private IEnumerator IndicatorDuration()
    {
        // Handle indicator visibility duration
        laserRenderer.enabled = true;
        MainGameUIScript.instance.laserIndicatorActive = true;
        yield return new WaitForSeconds(indicatorDuration);
        laserRenderer.enabled = false;
        MainGameUIScript.instance.laserIndicatorActive = false;
    }
    #endregion
}