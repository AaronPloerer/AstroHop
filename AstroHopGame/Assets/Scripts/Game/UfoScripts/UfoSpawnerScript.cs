using UnityEngine;

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
    [SerializeField] private GameObject enemyPrefab;     // UFO prefab to spawn
    [SerializeField] private Transform minPos;           // Left spawn point
    [SerializeField] private Transform maxPos;           // Right spawn point
    #endregion        

    #region Spawn Parameters
    [Header("Spawn Settings")]
    [SerializeField] private float spawnTimer;          // Countdown until next spawn 
    [SerializeField] private float spawnInterval;       // Time between spawns
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
            && LevelGeneratorScript.instance.CurrentPhase.minUfoSpawnTime < 999.0f;     // 999 in the inspector disables UFO spawning
    }

    private void UpdateSpawnTimer()
    {
        spawnTimer += Time.deltaTime;       // Count time since last spawn
    }

    private void TrySpawnEnemy()
    {
        // If timer counted up to spawn time: reset the timer, set a new random spawn time (based on current phase) and spawn enemy 
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0;
            spawnInterval = Random.Range(
                LevelGeneratorScript.instance.CurrentPhase.minUfoSpawnTime,
                LevelGeneratorScript.instance.CurrentPhase.maxUfoSpawnTime
            );
            SpawnEnemy();
        }
    }
    #endregion

    #region Spawn Utilities
    private void SpawnEnemy()
    {
        Instantiate(enemyPrefab, RandomSpawnPoint(), transform.rotation);
        aliveUfos++;
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