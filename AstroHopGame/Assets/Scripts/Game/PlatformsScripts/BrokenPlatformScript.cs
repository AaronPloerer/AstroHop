using UnityEngine;

public class BrokenPlatformScript : MonoBehaviour
{
    #region Settings and Components
    [Header("References")]
    [SerializeField] private GameObject rocketFuel;          // Fuel prefab to spawn
    [SerializeField] private Transform leftFuelPoint;        // Left fuel spawn boundary
    [SerializeField] private Transform rightFuelPoint;       // Right fuel spawn boundary
    [SerializeField] private Animator breakingPlatformAnim;  // Platform break animation

    [Header("Behavior Settings")]
    [SerializeField] private float playerJumpForce;       // Force applied to player bounce
    [SerializeField] private float fallDownSpeed;         // Speed of destroyed platform descent
    #endregion

    #region Runtime State
    private bool destroyed;            // Track if platform is completly visually destroyed
    private bool broken;               // Track if platform cannot be used anymore
    private GameObject spawnedFuel;   // Reference to spawned fuel object
    public bool forceFuelSpawn;       // Override for fuel spawning (determined by level genarator)
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializePlatform();
        TrySpawnFuel();
    }

    private void Update()
    {
        HandleAnimationState();
    }

    private void FixedUpdate()
    {
        HandleDestroyedMovement();
    }

    private void OnDestroy()
    {
        // Clean up spawned fuel when platform is destroyed
        if (spawnedFuel != null)
        {
            Destroy(spawnedFuel);
        }
    }
    #endregion

    #region Initialization
    private void InitializePlatform()
    {
        // Initialize platform state
        destroyed = false;
        broken = false;
    }
    #endregion

    #region Fuel Management
    private void TrySpawnFuel()
    {
        // Determine fuel spawn based on chance or override
        bool shouldSpawn = (forceFuelSpawn &&
             !PlayerControllerScript.instance.startingBoost) ||
             (Random.value < LevelGeneratorScript.instance.CurrentPhase.fuelSpawnChance &&
             leftFuelPoint != null &&
             rightFuelPoint != null &&
             !PlayerControllerScript.instance.startingBoost);

        if (shouldSpawn)
        {
            // Create fuel instance at random horizontal position
            spawnedFuel = Instantiate(rocketFuel, GetFuelPosition(), Quaternion.identity);
        }
    }

    private Vector2 GetFuelPosition()
    {
        // Calculate random horizontal position between boundaries
        Vector2 spawnPoint;
        spawnPoint.x = Random.Range(leftFuelPoint.position.x, rightFuelPoint.position.x);
        spawnPoint.y = leftFuelPoint.position.y;
        return spawnPoint;
    }
    #endregion

    #region Platform Behavior
    private void HandleAnimationState()
    {
        // Pause animation when game is paused
        breakingPlatformAnim.speed = MainGameUIScript.instance.paused ? 0 : 1;
    }

    private void HandleDestroyedMovement()
    {
        // Move platform down when destroyed
        if (destroyed && !MainGameUIScript.instance.paused)
        {
            transform.Translate(Vector3.down * fallDownSpeed);
        }
    }
    #endregion

    #region Collision Handling
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsValidPlayerCollision(collision))
        {
            HandlePlayerCollision();
        }
    }

    private bool IsValidPlayerCollision(Collider2D collision)
    {
        // Check if collision is valid player feet collision from the top (prevent mid-jump triggers)
        return collision.CompareTag("PlayerFeet") &&
               PlayerControllerScript.instance.rb.linearVelocity.y <= 0.5f;
    }

    private void HandlePlayerCollision()
    {
        ApplyPlayerBounce();
        BreakPlatform();
    }

    private void ApplyPlayerBounce()
    {
        // Only make player jump when platform was not yet used
        if (broken) return;

        // Apply upward force to player
        Vector2 velocity = PlayerControllerScript.instance.rb.linearVelocity;
        velocity.y = playerJumpForce;
        PlayerControllerScript.instance.rb.linearVelocity = velocity;

        // Play SFX and trigger animations
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.jump, AudioManagerScript.instance.jumpVolume);
        PlayerControllerScript.instance.astronautAnim.SetTrigger("jump");
        PlayerControllerScript.instance.laserAstronautAnim.SetTrigger("laserjump");
    }

    private void BreakPlatform()
    {
        if (broken)
        {
            // Disable platform and play SFX and trigger animations
            GetComponent<Collider2D>().enabled = false;
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.breaking, AudioManagerScript.instance.breakingVolume);
            breakingPlatformAnim.SetTrigger("break");
            destroyed = true;
            Destroy(spawnedFuel);      // Remove associated fuel item
        }

        broken = true;
    }
    #endregion
}