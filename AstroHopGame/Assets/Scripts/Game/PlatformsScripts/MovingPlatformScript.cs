using UnityEngine;

public class MovingPlatformScript : MonoBehaviour
{
    #region Settings and Components
    [Header("References")]
    [SerializeField] private GameObject rocketFuel;          // Fuel prefab to spawn
    [SerializeField] private Transform leftFuelPoint;        // Left fuel spawn boundary
    [SerializeField] private Transform rightFuelPoint;       // Right fuel spawn boundary

    [Header("Behavior Settings")]
    [SerializeField] private float playerJumpForce;   // Force applied to player bounce
    [SerializeField] private float difference;        // Horizontal movement range from start position
    [SerializeField] private float levelWidth;        // Level boundary constraints
    [SerializeField] private float speed;             // Platform movement speed
    #endregion

    #region Runtime State
    private int direction;             // Current movement direction (-1 = left, 1 = right)
    private float startX;              // Initial X position for movement calculations
    private float rightBound;          // Right movement boundary
    private float leftBound;           // Left movement boundary
    private GameObject spawnedFuel;    // Reference to spawned fuel object
    public bool forceFuelSpawn;        // Override for fuel spawning (determined by level genarator)
    #endregion

    #region Unity Lifecycle
    private void Start()
    {       
        InitializeMovement();
        SpawnFuel();
    }

    private void Update()
    {
        if (!PlayerControllerScript.instance.isAlive || MainGameUIScript.instance.paused) return;

        CheckBoundaries();
        HandleMovement();
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
    private void InitializeMovement()
    {
        // Set initial movement parameters
        direction = Random.Range(0, 2) == 0 ? -1 : 1;
        startX = transform.position.x;
        rightBound = startX + difference;
        leftBound = startX - difference;
    }
    #endregion

    #region Fuel Management
    private void SpawnFuel()
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

    #region Movement Logic
    private void HandleMovement()
    {
        // Move platform and attached fuel
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
        if (spawnedFuel != null)
        {
            spawnedFuel.transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
        }
    }

    private void CheckBoundaries()
    {
        // Check both movement constraint systems
        float currentX = transform.position.x;
        CheckDifferenceBounds(currentX);
        CheckLevelWidthBounds(currentX);
    }

    private void CheckDifferenceBounds(float currentX)
    {
        // Handle bounds based on difference from start position
        if (currentX >= rightBound)
        {
            transform.position = new Vector3(rightBound, transform.position.y, transform.position.z);
            direction = -1;
        }
        else if (currentX <= leftBound)
        {
            transform.position = new Vector3(leftBound, transform.position.y, transform.position.z);
            direction = 1;
        }
    }

    private void CheckLevelWidthBounds(float currentX)
    {
        // Handle bounds based on absolute level width
        if (currentX >= levelWidth)
        {
            transform.position = new Vector3(levelWidth, transform.position.y, transform.position.z);
            direction = -1;
        }
        else if (currentX <= -levelWidth)
        {
            transform.position = new Vector3(-levelWidth, transform.position.y, transform.position.z);
            direction = 1;
        }
    }
    #endregion

    #region Collision Handling
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsValidPlayerCollision(collision))
        {
            ApplyPlayerBounce();
        }
    }

    private bool IsValidPlayerCollision(Collider2D collision)
    {
        // Check if collision is valid player feet collision from the top (prevent mid-jump triggers)
        return collision.CompareTag("PlayerFeet") &&
               PlayerControllerScript.instance.rb.linearVelocity.y <= 0.5f;
    }

    private void ApplyPlayerBounce()
    {
        // Apply upward force to player
        Vector2 velocity = PlayerControllerScript.instance.rb.linearVelocity;
        velocity.y = playerJumpForce;
        PlayerControllerScript.instance.rb.linearVelocity = velocity;

        // Play SFX and trigger animations
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.jump, AudioManagerScript.instance.jumpVolume);
        PlayerControllerScript.instance.astronautAnim.SetTrigger("jump");
        PlayerControllerScript.instance.laserAstronautAnim.SetTrigger("laserjump");
    }
    #endregion
}