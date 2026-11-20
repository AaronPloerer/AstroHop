using UnityEngine;
using System.Collections.Generic;
public class LevelGeneratorScript : MonoBehaviour
{
    #region Singleton Instance
    public static LevelGeneratorScript instance;

    void Awake()
    {
        // Singleton pattern implementation
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

    #region Platform Prefabs and Settings
    [Header("Platform Prefabs")]
    // Standard, Broken, Moving and Wide Platform Templates
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject brokenPlatformPrefab;
    [SerializeField] private GameObject movingPlatformPrefab;
    [SerializeField] private GameObject widePlatformPrefab;

    [Header("Wide Platform")]
    [SerializeField] private float widePlatformExclusion;        // No platforms this far below the startingboost platform
    [SerializeField] private float widePlatformYOffset;          // Fine-tuning of the wide startingboost platform height (+ up / - down)
    [SerializeField] private float minXDistanceFromCenter;       // Min x distance from x = 0 for the first platforms (start and after the wide platform)
    [SerializeField] private float centerClearHeight;            // Height range above the first platform / startingboost platform where this applies

    [Header("Level Dimensions")]
    [SerializeField] private float levelWidth;         // Horizontal play area boundary
    [SerializeField] public float spawnBuffer;         // Vertical buffer for platform spawning
    [SerializeField] public float despawnBuffer;       // Vertical buffer for platform removal
    #endregion

    #region Phase Configuration
    [System.Serializable]
    public class Phase
    {
        public float height;                 // Score threshold to reach next phase

        // Platform position
        public float minY;                   // Minimum vertical spacing between platforms
        public float maxY;                   // Maximum vertical spacing between platforms
        public float minX;                   // Minimum horizontal spacing between platforms
        public float maxX;                   // Maximum horizontal spacing between platforms

        public float percentageBrokenPlatform;       // Probability of broken platforms
        public float percantageMovingPlatform;       // Probability of moving platforms that are not broken 

        // Fuel and special settings
        public float fuelSpawnChance;                   // Base fuel spawn probability
        public GameObject tutorial;                // Possible tutorial text
        public GameObject firstPlatformPrefab;          // Force platform type for phase start
        public bool firstPlatformFuel;                  // Force fuel on first platform

        // UFO spawn settings
        public float minUfoSpawnTime;  // Minimum time between UFO spawns
        public float maxUfoSpawnTime;  // Maximum time between UFO spawns
        public float chanceFastUfo;    // Probability for fast UFO

    }

    public Phase[] phases;
    #endregion

    #region Runtime Variables
    private Transform cameraTransform;    // Reference to main camera transform component
    private float highestSpawnY;          // Tracks highest spawned platform y-position for platform spawning based on camera position
    private GameObject chosenPlatform;    // Selected platform for spawn
    private bool mustSpawnFuel;           // Flag for for forced fuel spawn on platform 
    private Vector3 spawnPosition;        // Selected coordinates for spawn
    private List<GameObject> spawnedPlatforms = new List<GameObject>();  // List of all active platforms for despawn check

    private bool startingBoostChecked;                                  // The start-of-run boost is checked once on the first Update
    private float widePlatformY = float.PositiveInfinity;               // Infinity = no wide platform waiting for the cursor (exclusion inactive)
    private float lastWidePlatformY = float.NegativeInfinity;           // Height of the last wide platform (stays set after widePlatformY is reset)

    // Phase management
    public int currentPhaseIndex;         // Index of current phase
    private Phase currentPhase;           // Reference to current phase data
    private bool isFirstPlatformOfPhase;  // Flag for first platform of phase for special customization
    #endregion

    #region Unity Lifecycle Methods
    void Start()
    {
        InitializeLevelGeneration();
        CreateInitialPlatform();
    }

    void Update()
    {
        CheckStartingBoostWidePlatform();
        HandlePhaseProgression();
        HandlePlatformSpawning();
    }
    #endregion

    #region Initialization
    private void InitializeLevelGeneration()
    {
        // Set initial values
        cameraTransform = Camera.main.transform;  // Reference camera transforn
        spawnPosition = transform.position;       // First spawn at object position
        currentPhaseIndex = 0;                    // Start at first phase
        currentPhase = phases[0];                 // Set initial phase
        highestSpawnY = spawnPosition.y;          // Initialize tracking value
        isFirstPlatformOfPhase = false;           // Turn off phase-start flag
        mustSpawnFuel = false;                    // Turn off force-fuel-spawn flag
    }

    private void CreateInitialPlatform()
    {
        // Create safe wide starting platform at origin and add to tracking list
        GameObject newPlatform = Instantiate(widePlatformPrefab, transform.position, Quaternion.identity);
        spawnedPlatforms.Add(newPlatform);
    }
    #endregion

    #region Phase Management
    private void HandlePhaseProgression()
    {
        // Progress to next phase index when player reaches phase height and set flag for first platform of next phase
        if (PlayerControllerScript.instance != null &&
            currentPhaseIndex < phases.Length &&
            PlayerControllerScript.instance.transform.position.y * MainGameUIScript.instance.positionToScore >= phases[currentPhaseIndex].height)
        {
            currentPhaseIndex++;
            isFirstPlatformOfPhase = true;
        }
    }
    #endregion

    #region Platform Spawning System
    private void HandlePlatformSpawning()
    {
        // Spawn new platforms when camera approaches top buffer
        if (cameraTransform.position.y + spawnBuffer > highestSpawnY)
        {
            CreatePlatform();
        }
        CheckDespawnPlatforms();
    }

    private void CheckDespawnPlatforms()
    {
        // Calculate removal threshold below camera
        float despawnThreshold = cameraTransform.position.y - despawnBuffer;
        List<GameObject> toRemove = new List<GameObject>();

        // Identify platforms below threshold
        foreach (GameObject platform in spawnedPlatforms)
        {
            if (platform != null && platform.transform.position.y < despawnThreshold)
            {
                toRemove.Add(platform);
            }
        }

        // Remove tracking and destroy old platforms
        foreach (GameObject platform in toRemove)
        {
            spawnedPlatforms.Remove(platform);
            Destroy(platform);
        }
    }

    private void CreatePlatform()
    {
        UpdateCurrentPhase();
        CalculateNewPosition();

        // Cursor reached the exclusion zone below the wide platform:
        // skip this platform and continue from the wide platform's position
        if (spawnPosition.y >= widePlatformY - widePlatformExclusion)
        {
            spawnPosition = new Vector3(0f, widePlatformY, spawnPosition.z);
            highestSpawnY = widePlatformY;
            widePlatformY = float.PositiveInfinity;   // Done, exclusion is inactive again
            return;
        }

        HandlePositionConstraints();
        DeterminePlatformType();
        InstantiatePlatform();
    }

    private void UpdateCurrentPhase()
    {
        // Define reference for current phase information. Use last phase if index exceeded array list
        currentPhase = currentPhaseIndex < phases.Length
            ? phases[currentPhaseIndex]
            : phases[phases.Length - 1];
    }

    private void CalculateNewPosition()
    {
        // Random vertical placement upwards within phase parameters
        float yOffset = Random.Range(currentPhase.minY, currentPhase.maxY);
        spawnPosition.y += yOffset;
        highestSpawnY = spawnPosition.y;

        // Random horizontal placement within phase parameters with 50/50 direction chance
        if (Random.value < 0.5f)
        {
            spawnPosition.x += Random.Range(currentPhase.minX, currentPhase.maxX);
        }
        else
        {
            spawnPosition.x -= Random.Range(currentPhase.minX, currentPhase.maxX);
        }
    }

    private void HandlePositionConstraints()
    {
        // Keep horizontal position within level bounds
        if (spawnPosition.x < -levelWidth)
        {
            float absPos = Mathf.Abs(spawnPosition.x);
            float difference = absPos - levelWidth;
            spawnPosition.x += (difference * 2.0f);
        }
        else if (spawnPosition.x > levelWidth)
        {
            float difference = spawnPosition.x - levelWidth;
            spawnPosition.x -= (difference * 2.0f);
        }

        // Platforms right above the first platform or above the starting boost wide platform (both at x = 0)
        // must not be too close to the center, so the player doesn't move before the first input
        bool nearStartPlatform = spawnPosition.y < centerClearHeight;
        bool nearWidePlatform = spawnPosition.y >= lastWidePlatformY &&
                                spawnPosition.y <= lastWidePlatformY + centerClearHeight;

        if (nearStartPlatform || nearWidePlatform)
        {
            KeepAwayFromCenter();
        }
    }

    private void KeepAwayFromCenter()
    {
        // Push the platform out to at least minXDistanceFromCenter from x = 0
        if (spawnPosition.x >= 0.0f && spawnPosition.x < minXDistanceFromCenter)
        {
            spawnPosition.x = minXDistanceFromCenter;
        }
        else if (spawnPosition.x > -minXDistanceFromCenter && spawnPosition.x < 0.0f)
        {
            spawnPosition.x = -minXDistanceFromCenter;
        }
    }

    private void DeterminePlatformType()
    {
        // Force platform type and fuel spawn or make it random
        mustSpawnFuel = false;

        if (isFirstPlatformOfPhase)
        {
            HandleFirstPlatformOfPhase();   //First platform selection
        }
        else
        {
            ChooseRandomPlatform();    // Normal platform selection
        }
    }

    private void HandleFirstPlatformOfPhase()
    {
        // Use custom platform if specified
        if (currentPhase.firstPlatformPrefab != null)
        {
            chosenPlatform = currentPhase.firstPlatformPrefab;
        }
        else
        {
            ChooseRandomPlatform();    // Fallback to normal selection
        }
        mustSpawnFuel = currentPhase.firstPlatformFuel;      // Set force-fuel-spawn flag for first platform of phase if specified
        isFirstPlatformOfPhase = false;                      // Reset phase-start flag
    }

    private void InstantiatePlatform()
    {
        // Create platform instance at calculated position
        GameObject newPlatform = Instantiate(chosenPlatform, spawnPosition, Quaternion.identity);
        ConfigurePlatformFuel(newPlatform);
        spawnedPlatforms.Add(newPlatform);
    }

    private void ConfigurePlatformFuel(GameObject newPlatform)
    {
        // Check all possible platform types
        var standardPlatform = newPlatform.GetComponent<PlatformScript>();
        var movingPlatform = newPlatform.GetComponent<MovingPlatformScript>();
        var brokenPlatform = newPlatform.GetComponent<BrokenPlatformScript>();

        // Set forceFuelSpawn based on foce-fuel-spawn flag on whichever component exists
        if (standardPlatform != null) standardPlatform.forceFuelSpawn = mustSpawnFuel;
        if (movingPlatform != null) movingPlatform.forceFuelSpawn = mustSpawnFuel;
        if (brokenPlatform != null) brokenPlatform.forceFuelSpawn = mustSpawnFuel;
    }
    #endregion

    #region Platform Selection Logic
    private void ChooseRandomPlatform()
    {
        // Random selection using phase probabilities
        float randomValue = Random.value;

        if (randomValue < currentPhase.percentageBrokenPlatform)
        {
            chosenPlatform = brokenPlatformPrefab;
        }
        else if (randomValue < currentPhase.percantageMovingPlatform)
        {
            chosenPlatform = movingPlatformPrefab;
        }
        else
        {
            chosenPlatform = platformPrefab;
        }
    }
    #endregion

    #region Wide Platform After Starting Boost
    private void CheckStartingBoostWidePlatform()
    {
        // Runs once on the first frame, when all Start() methods are done and the player knows if the run begins with a starting boost
        if (startingBoostChecked) return;
        startingBoostChecked = true;

        if (PlayerControllerScript.instance != null &&
            PlayerControllerScript.instance.TryGetStartingBoostApexY(out float apexY))
        {
            RequestWidePlatform(apexY);
        }
    }

    // Called with the height where a boost ends up. The wide platform is spawned one jump below that height.
    public void RequestWidePlatform(float boostApexY)
    {
        PlatformScript widePlatformScript = widePlatformPrefab.GetComponent<PlatformScript>();
        if (widePlatformScript == null)
        {
            Debug.LogWarning("widePlatformPrefab has no PlatformScript: jump height cannot be calculated");
            return;
        }

        // Height reached by one bounce from the wide platform
        float gravity = Mathf.Abs(PlayerControllerScript.instance.rb.gravityScale * Physics2D.gravity.y);
        float jumpForce = widePlatformScript.playerJumpForce;
        float jumpHeight = (jumpForce * jumpForce) / (2f * gravity);

        // After landing and jumping, the player reaches the same height as the boost did
        float targetY = boostApexY - jumpHeight + widePlatformYOffset;

        // Safety: never inside the exclusion zone of platforms that were already spawned
        widePlatformY = Mathf.Max(targetY, highestSpawnY + widePlatformExclusion);
        lastWidePlatformY = widePlatformY;

        // Spawn it right away, centered in x. The cursor is not touched.
        GameObject widePlatform = Instantiate(widePlatformPrefab, new Vector3(0f, widePlatformY, transform.position.z), Quaternion.identity);
        spawnedPlatforms.Add(widePlatform);

        // UFOs wait until the player has landed and jumped higher after the boost
        if (UfoSpawnerScript.instance != null)
        {
            UfoSpawnerScript.instance.StartAfterStartingBoostDelay(boostApexY);
        }
    }
    #endregion

    #region Public Properties
    public Phase CurrentPhase
    {
        // Safely return current phase
        get
        {
            return currentPhaseIndex < phases.Length
                ? phases[currentPhaseIndex]
                : phases[phases.Length - 1];
        }
    }
    #endregion
}