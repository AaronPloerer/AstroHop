using UnityEngine;

public class PlatformScript : MonoBehaviour
{
    #region Components/References
    [Header("References")]
    [SerializeField] private GameObject rocketFuel;           // Fuel prefab to spawn
    [SerializeField] private Transform leftFuelPoint;         // Left fuel spawn boundary
    [SerializeField] private Transform rightFuelPoint;        // Right fuel spawn boundary

    [Header("Sprite Settings")]
    [SerializeField] private Sprite otherSprite;             
    [SerializeField] private Sprite otherOtherSprite;                         
    [SerializeField] private float otherSpriteProbability;
    [SerializeField] private float otherOtherSpriteProbability;

    [Header("Behavior Settings")]
    [SerializeField] private float playerJumpForce;                 // Force applied to player bounce
    [SerializeField] private float otherSoundProbability;
    #endregion

    #region Runtime State
    private GameObject spawnedFuel;                           // Reference to spawned fuel object
    public bool forceFuelSpawn;                               // Override for fuel spawning (determined by level genarator)
    private int whatSprite;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        TrySpawnFuel();
        TryChangeSprite();
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

    #region Sprite Management
    private void TryChangeSprite()
    {
        whatSprite = 0;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null) return;

        // First chance: try otherSprite
        if (otherSprite != null && Random.value < otherSpriteProbability)
        {
            renderer.sprite = otherSprite;
            whatSprite = 1;
            return; // Exit after first successful change
        }

        // Second chance: try otherOtherSprite (only if first didn't change)
        if (otherOtherSprite != null && Random.value < otherOtherSpriteProbability)
        {
            renderer.sprite = otherOtherSprite;
            whatSprite = 2;
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
        if (whatSprite == 0)
        {
            if (Random.value < otherSoundProbability)
            {
                AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.jump4, AudioManagerScript.instance.jump4Volume);
            }
            else
            {
                AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.jump, AudioManagerScript.instance.jumpVolume);
            }
        }
        else if (whatSprite == 1)
        {
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.jump2, AudioManagerScript.instance.jump2Volume);
        }
        else
        {
            if (whatSprite == 2)
            {
                AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.jump3, AudioManagerScript.instance.jump3Volume);
            }

            PlayerControllerScript.instance.astronautAnim.SetTrigger("jump");
            PlayerControllerScript.instance.laserAstronautAnim.SetTrigger("laserjump");
        }
    }
    #endregion
}