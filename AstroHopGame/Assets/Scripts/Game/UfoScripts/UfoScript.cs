using System.Linq;
using UnityEngine;

public class UfoScript : MonoBehaviour
{
    #region Serialized Fields

    [Header("Flying Ufo")]
    [SerializeField] private float moveSpeedX;                      // Horizontal movement speed
    [SerializeField] private float moveSpeedY;                      // Vertical movement speed (downward)
    [SerializeField] private float directionCooldown;               // Minumum time between direction changes
    [SerializeField] private SpriteRenderer spriteRenderer;                  // Reference to sprite renderer component
    [SerializeField] private Animator ufoAnim;                      // Reference to animator component

    [Header("Destroyed Ufo")]
    [SerializeField] private float jumpForce;                       // Force applied to player when jumped on
    [SerializeField] private float fallSpeed;                       // Falling speed of jumped on UFO
    [SerializeField] private GameObject explosionPrefab;            // Explosion effect prefab

    [Header("Warning Arrow")]
    [SerializeField] private GameObject ufoWarningArrowPrefab;      // Off-screen UFO indicator prefab
    [SerializeField] private float arrowPositionInViewY;            // Vertical offset for arrow relative to camera 
    [SerializeField] private float maxArrowPositionX;               // Horizontal bounds for arrow 
    #endregion

    #region Private Variables
    private float xActualDirection;      // Current horizontal movement direction (-1 or 1)
    private float directionChangeTimer;  // Timer for direction change cooldown
    private bool broken;                 // UFO destruction state flag 
    private GameObject spawnedArrow;     // Reference to instantiated arrow
    private Vector2 arrowPosition;       // Calculated screen-space arrow position
    private bool killable;               // Flag for laser vulnerability (when in camera view)
    #endregion

    #region Unity Lifecycle Methods
    void Start()
    {
        InitializeUfoState();
        InstantiateArrow();
    }

    void Update()
    {

        // Only update when game is active and unpaused
        if (PlayerControllerScript.instance.isAlive && !MainGameUIScript.instance.paused)
        {
            HandleActiveUpdate();
            ufoAnim.speed = 1;     // Play animation
        }
        else
        {
            ufoAnim.speed = 0;     // Freeze animation
        }

        // Lower Y (closer to bottom) = higher sorting order
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }

    private void LateUpdate()
    {
        UpdateArrow();
    }
    #endregion

    #region Collision & Trigger Handling
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (broken) return;    // Ignore collisions after destruction

        // Handle laser collision
        if (collision.gameObject.CompareTag("Laser"))
        {
            HandleLaserCollision();
        }
        // Handle player jump collision
        else if (collision.collider.CompareTag("PlayerFeet"))
        {
            HandlePlayerJumpCollision(collision);
        }
        // Handle player game over collision
        else if (collision.collider.CompareTag("Player"))
        {
            HandlePlayerCollision();
        }
        // Handle boundary collision
        else if (collision.gameObject.CompareTag("End"))
        {
            HandleBoundaryCollision();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Activate vulnerability when entering game view zone
        if (other.CompareTag("GameViewArea"))
        {
            Destroy(spawnedArrow);
            killable = true;
        }
    }
    #endregion

    #region Initialization
    private void InitializeUfoState()
    {
        // Initialize horizontal travel direction to travel towards the player's position
        xActualDirection = Mathf.Sign(PlayerControllerScript.instance.transform.position.x - transform.position.x);
        broken = false;
        killable = false;
    }

    private void InstantiateArrow()
    {
        // Spawn arrow at UFO's X position and rbased on camera's Y position
        arrowPosition.x = transform.position.x;
        arrowPosition.y = CameraScript.instance.transform.position.y + arrowPositionInViewY;
        spawnedArrow = Instantiate(ufoWarningArrowPrefab, arrowPosition, Quaternion.identity);
    }
    #endregion

    #region Movement
    private void HandleActiveUpdate()
    {
        UpdateDirection();
        MoveUfo();
    }

    private void UpdateDirection()
    {
        // Calculate correct direction to travel towards player
        float targetDirection = Mathf.Sign(PlayerControllerScript.instance.transform.position.x - transform.position.x);

        // Only allow direction change after cooldown ro prevent rapid movement changes
        if (xActualDirection != targetDirection)
        {
            directionChangeTimer += Time.deltaTime;
            if (directionChangeTimer >= directionCooldown)
            {
                xActualDirection = targetDirection;
                directionChangeTimer = 0f;            // Reset cooldown
            }
        }
        else
        {
            directionChangeTimer = 0f;                // Don#t start timer when directions match
        }
    }

    private void MoveUfo()
    {
        if (!broken)
        {
            // Normal horizontal/vertical movement
            float xMovement = xActualDirection * moveSpeedX * Time.deltaTime;
            float yMovement = -moveSpeedY * Time.deltaTime;
            transform.Translate(new Vector3(xMovement, yMovement, 0));
        }
        else
        {
            // Broken UFO falls straight down
            float yMovement = -fallSpeed * Time.deltaTime;
            transform.Translate(new Vector3(0, yMovement, 0));
        }
    }

    private void UpdateArrow()
    {
        if (spawnedArrow == null) return;

        // Keep position arrow relative to camera and UFO
        Vector2 newPosition = spawnedArrow.transform.position;
        newPosition.x = Mathf.Clamp(transform.position.x, -maxArrowPositionX, maxArrowPositionX);
        newPosition.y = CameraScript.instance.transform.position.y + arrowPositionInViewY;
        spawnedArrow.transform.position = newPosition;

        // Toggle arrow visibility based on x-position to hide arrow when not completly in game view area
        bool isOutOfBounds = Mathf.Abs(newPosition.x) >= maxArrowPositionX;
        spawnedArrow.GetComponent<SpriteRenderer>().enabled = !isOutOfBounds;
    }
    #endregion

    #region Collision Handling
    private void HandleLaserCollision()
    {
        if (!killable) return;     // Ignore lasers when outside game view area

        // Play explosion SFX and animation and remove UFO from the UFO counter as well as from the scene (with arrow)
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.explosion, AudioManagerScript.instance.explosionVolume);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        UfoSpawnerScript.instance.aliveUfos--;
        Destroy(spawnedArrow);
        Destroy(gameObject);
    }

    private void HandlePlayerJumpCollision(Collision2D collision)
    {
        // Check if collision came from above
        bool hitFromAbove = collision.contacts.Any(contact => contact.normal.y < -0.1f);
        if (!hitFromAbove) return;

        // If true: play SFX and animation, ...
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.jump, AudioManagerScript.instance.jumpVolume);
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.explosion, AudioManagerScript.instance.explosionVolume);
        GetComponent<Animator>().SetTrigger("broken");

        // ... set destruction state flag,
        broken = true;

        // ... make the player jump from the UFO and
        PlayerControllerScript.instance.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, jumpForce);

        // ... remove the UFO from the counter
        UfoSpawnerScript.instance.aliveUfos--;
    }

    private void HandlePlayerCollision()
    {
        // Check if player has invulnerability from boost upward movement
        if (PlayerControllerScript.instance.boostMovement || PlayerControllerScript.instance.startingBoost)
        {
            // UFO destruction sequence like with laser collision
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.explosion, AudioManagerScript.instance.explosionVolume);
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            UfoSpawnerScript.instance.aliveUfos--;
            Destroy(gameObject);
        }
        else
        {
            // Player death sequence with position-adjusted explosion
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.explosion, AudioManagerScript.instance.explosionVolume);
            Vector3 explosionPos = PlayerControllerScript.instance.transform.position + Vector3.up * 1.07f;
            Instantiate(explosionPrefab, explosionPos, Quaternion.identity);

            // Show game over screen with crash expanation
            PlayerControllerScript.instance.crashing = true;
            ManagerScript.instance.GameOverScreen();

            // Set player state as dead and remove him from the scene
            PlayerControllerScript.instance.isAlive = false;
            Destroy(PlayerControllerScript.instance.gameObject);
        }
    }

    private void HandleBoundaryCollision()
    {
        // Clean up UFO that leaves play area
        UfoSpawnerScript.instance.aliveUfos--;
        Destroy(gameObject);
    }
    #endregion
}