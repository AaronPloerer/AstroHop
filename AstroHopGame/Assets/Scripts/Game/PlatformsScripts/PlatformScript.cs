using UnityEngine;

public class PlatformScript : MonoBehaviour
{
    #region Components/References
    [Header("References")]
    [SerializeField] private GameObject rocketFuel;           // Fuel prefab to spawn
    [SerializeField] private Transform leftFuelPoint;         // Left fuel spawn boundary
    [SerializeField] private Transform rightFuelPoint;        // Right fuel spawn boundary

    [Header("Behavior Settings")]
    [SerializeField] private float playerJumpForce;                 // Force applied to player bounce
    #endregion

    #region Runtime State
    private GameObject spawnedFuel;                           // Reference to spawned fuel object
    public bool forceFuelSpawn;                               // Override for fuel spawning (determined by level genarator)
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        TrySpawnFuel();
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