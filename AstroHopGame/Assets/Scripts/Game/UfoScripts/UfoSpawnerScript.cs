using UnityEngine;
using System.Collections.Generic;

public class UfoSpawnerScript : MonoBehaviour
{
    #region Singleton
    public static UfoSpawnerScript instance;

    void Awake()
    {
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

    #region Components/References
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;     // Normal UFO prefab to spawn
    [SerializeField] private GameObject enemyFastPrefab; // Small, fast UFO prefab to spawn
    [SerializeField] private Transform minPos;           // Left spawn point
    [SerializeField] private Transform maxPos;           // Right spawn point
    #endregion        

    #region Spawn Parameters
    [Header("Spawn Settings")]
    [SerializeField] private float spawnTimer;          // Countdown until next spawn 
    [SerializeField] private float spawnInterval;       // Time between spawns
    [SerializeField] private float minimumUfoSpacing;   // Space between spawns
    private Transform lastSpawnedUfo;

    [Header("After Starting Boost")]
    [SerializeField] private float minUfoSpawnTimeAfterStartingBoost;     // Shortest wait for the first UFO after the boost
    [SerializeField] private float maxUfoSpawnTimeAfterStartingBoost;     // Longest wait for the first UFO after the boost
    [SerializeField] private float afterStartingBoostPadding;             // Player must get this far ABOVE the boost's end height
    private bool waitingForLanding;                                       // UFOs blocked until the player has landed and jumped higher after the boost
    private float releaseScore;                                           // Height the player has to reach
    #endregion

    #region Game State Tracking
    public int aliveUfos;              // Current number of active UFOs in scene
    #endregion

    #region Unity Lifecycle Methods
    void Start()
    {
        aliveUfos = 0;    // Initialize UFO count
        spawnTimer = 0;   // Initialize spawn timer
    }

    void Update()
    {
        CheckAfterBoostRelease();

        if (ShouldSpawn())
        {
            UpdateSpawnTimer();
            TrySpawnEnemy();
        }

        ControlUfoSound();
    }
    #endregion

    #region Spawn Logic
    private bool ShouldSpawn()
    {
        // Only spawn when game is active, phase allows spawning and the player has no active starting boost 
        return PlayerControllerScript.instance.isAlive
            && !MainGameUIScript.instance.paused
            && !PlayerControllerScript.instance.startingBoost
            && !waitingForLanding
            && !MainGameUIScript.instance.playerStill
            && LevelGeneratorScript.instance.CurrentPhase.minUfoSpawnTime < 999.0f;     // 999 in the inspector disables UFO spawning
    }

    private void UpdateSpawnTimer()
    {
        spawnTimer += Time.deltaTime;       // Count time since last spawn
    }

    private void TrySpawnEnemy()
    {
        // If timer counted up to spawn time and spacing is big enough: reset the timer, set a new random spawn time (based on current phase) and spawn enemy 
        if (spawnTimer >= spawnInterval && HasEnoughSpacing())
        {
            spawnTimer = 0;
            spawnInterval = Random.Range(
                LevelGeneratorScript.instance.CurrentPhase.minUfoSpawnTime,
                LevelGeneratorScript.instance.CurrentPhase.maxUfoSpawnTime
            );
            SpawnEnemy();
        }
    }

    private bool HasEnoughSpacing()
    {
        // Last UFO was destroyed (laser, jump, boundary) or none spawned yet: nothing blocks the spawner
        if (lastSpawnedUfo == null) return true;

        return maxPos.position.y - lastSpawnedUfo.position.y >= minimumUfoSpacing;
    }
    #endregion

    #region After Starting Boost
    // Called by the level generator whenever a boost (start of run or revive) is set up
    public void StartAfterStartingBoostDelay(float boostApexY)
    {
        waitingForLanding = true;
        releaseScore = boostApexY * MainGameUIScript.instance.positionToScore + afterStartingBoostPadding;
    }

    private void CheckAfterBoostRelease()
    {
        if (!waitingForLanding) return;

        PlayerControllerScript player = PlayerControllerScript.instance;
        if (player == null || !player.isAlive) return;

        // Still thrusting or coasting upward from the boost: not landed yet
        if (player.startingBoost || player.boostMovement) return;

        // Player's score got higher than the boost's end score + padding -> jumped to a new platform
        float playerScore = player.transform.position.y * MainGameUIScript.instance.positionToScore;
        if (playerScore >= releaseScore)
        {
            waitingForLanding = false;
            spawnTimer = 0f;

            // Phase times limited to the after-boost range. Both below the range -> min, both above -> max
            var phase = LevelGeneratorScript.instance.CurrentPhase;
            float min = Mathf.Clamp(phase.minUfoSpawnTime, minUfoSpawnTimeAfterStartingBoost, maxUfoSpawnTimeAfterStartingBoost);
            float max = Mathf.Clamp(phase.maxUfoSpawnTime, minUfoSpawnTimeAfterStartingBoost, maxUfoSpawnTimeAfterStartingBoost);
            spawnInterval = Random.Range(min, max);
        }
    }
    #endregion

    #region Spawn Utilities
    private void SpawnEnemy()
    {
        // Spawn fast or normal prefab based on current phase chance
        GameObject prefabToSpawn = Random.value < LevelGeneratorScript.instance.CurrentPhase.chanceFastUfo
            ? enemyFastPrefab
            : enemyPrefab;

        GameObject newUfo = Instantiate(prefabToSpawn, RandomSpawnPoint(), transform.rotation);
        lastSpawnedUfo = newUfo.transform;
        aliveUfos++;         // Track new UFO
    }

    private Vector2 RandomSpawnPoint()
    {
        // Random x-position between boundaries, fixed y
        return new Vector2(
            Random.Range(minPos.position.x, maxPos.position.x),
            maxPos.position.y
        );
    }
    #endregion

    #region Audio Management
    private void ControlUfoSound()
    {
        if (aliveUfos <= 0)
        {
            AudioManagerScript.instance.StopUfoSFX();       // No UFOs - stop sound
        }
        else if (!PlayerControllerScript.instance.isAlive || MainGameUIScript.instance.paused)
        {
            AudioManagerScript.instance.PauseUfoSFX();      // Pause sound when game is not active
        }
        else
        {
            AudioManagerScript.instance.PlayUfoSFX();       // Normal playback
        }
    }
    #endregion
}