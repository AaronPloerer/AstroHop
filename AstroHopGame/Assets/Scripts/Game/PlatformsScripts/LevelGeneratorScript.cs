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
    // Standard, Broken and Moving Platform Templates
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject brokenPlatformPrefab;
    [SerializeField] private GameObject movingPlatformPrefab;

    [Header("Decorative Platforms")]
    [SerializeField] private GameObject[] decorativePrefabs;      // Array of decorative platform prefabs
    [SerializeField] private float decorativeMinY;                // Minimum vertical distance between decorative platforms
    [SerializeField] private float decorativeMaxY;                // Maximum vertical distance between decorative platforms
    [SerializeField] private float spawnBufferDecorative;         // Vertical buffer for platform spawning

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
        public GameObject firstTutorial;                // Possible tutorial element
        public GameObject secondTutorial;               // Possible secondary tutorial element
        public GameObject firstPlatformPrefab;          // Force platform type for phase start
        public bool firstPlatformFuel;                  // Force fuel on first platform

        // UFO spawn settings
        public float minUfoSpawnTime;  // Minimum time between UFO spawns
        public float maxUfoSpawnTime;  // Maximum time between UFO spawns
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

    // Phase management
    public int currentPhaseIndex;         // Index of current phase
    private Phase currentPhase;           // Reference to current phase data
    private bool isFirstPlatformOfPhase;  // Flag for first platform of phase for special customization

    // Decorative platforms
    private float lastDecorativeSpawnY = -10f;    // Y-position of last decorative spawn
    private List<GameObject> decorativePlatforms = new List<GameObject>(); // Track decorative platforms
    #endregion

    #region Unity Lifecycle Methods
    void Start()
    {
        InitializeLevelGeneration();
        CreateInitialPlatform();
    }

    void Update()
    {
        HandlePhaseProgression();
        HandlePlatformSpawning();
        HandleDecorativeSpawning();
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
        lastDecorativeSpawnY = spawnPosition.y;   // Initialize decorative tracking
    }

    private void CreateInitialPlatform()
    {
        // Create safe starting platform at origin and add to tracking list
        GameObject newPlatform = Instantiate(platformPrefab, transform.position, Quaternion.identity);
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

        // Ensure initial platforms aren't too close to center to prevent player movement before first input
        if (spawnPosition.y < 3.0f)
        {
            if (spawnPosition.x >= 0.0f && spawnPosition.x < 1.2f)
            {
                spawnPosition.x = 1.2f;
            }
            else if (spawnPosition.x > -1.2f && spawnPosition.x < 0.0f)
            {
                spawnPosition.x = -1.2f;
            }
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

    #region Decorative Platform System
    private void HandleDecorativeSpawning()
    {
        // Spawn new decorative platforms when camera approaches top buffer
        if (cameraTransform.position.y + spawnBufferDecorative > lastDecorativeSpawnY)
        {
            TrySpawnDecorative();
        }
        CheckDespawnDecoratives();
    }

    private void TrySpawnDecorative()
    {
        if (cameraTransform.position.y + spawnBufferDecorative > lastDecorativeSpawnY)
        {
            Vector3 candidatePosition = CalculateDecorativePosition();
            GameObject prefab = decorativePrefabs[Random.Range(0, decorativePrefabs.Length)];

            if (IsValidDecorativePosition(candidatePosition, prefab))
            {
                SpawnDecorative(candidatePosition, prefab);
            }

            // Update position even if no valid spot found
            lastDecorativeSpawnY = candidatePosition.y;
        }
    }

    private Vector3 CalculateDecorativePosition()
    {
        Vector3 position = Vector3.zero;

        // Random vertical placement upwards
        float yOffset = Random.Range(decorativeMinY, decorativeMaxY);
        position.y = lastDecorativeSpawnY + yOffset;

        // Random horizontal placement within level bounds
        position.x = Random.Range(-levelWidth, levelWidth);

        return position;
    }

    private bool IsValidDecorativePosition(Vector3 position, GameObject prefab)
    {
        // Get the child object with BoxCollider2D
        Transform child = prefab.transform.GetChild(0);

        // Get the BoxCollider2D from the child
        BoxCollider2D decorCollider = child.GetComponent<BoxCollider2D>();

        // Calculate world position of the collider
        Vector3 childOffset = child.localPosition;
        Vector2 worldCenter = position + childOffset + (Vector3)decorCollider.offset;

        // Check for overlaps with functional platforms
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            worldCenter,
            decorCollider.size,
            0
        );

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            return false;

        }
        return true;
    }

    private void SpawnDecorative(Vector3 position, GameObject prefab)
    {
        GameObject decor = Instantiate(prefab, position, Quaternion.identity);
        decorativePlatforms.Add(decor);
        lastDecorativeSpawnY = position.y;
    }

    private void CheckDespawnDecoratives()
    {
        // Calculate removal threshold below camera
        float despawnThreshold = cameraTransform.position.y - despawnBuffer;
        List<GameObject> toRemove = new List<GameObject>();

        // Identify decoratives below threshold
        foreach (GameObject decor in decorativePlatforms)
        {
            if (decor != null && decor.transform.position.y < despawnThreshold)
            {
                toRemove.Add(decor);
            }
        }

        // Remove tracking and destroy old decoratives
        foreach (GameObject decor in toRemove)
        {
            decorativePlatforms.Remove(decor);
            Destroy(decor);
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