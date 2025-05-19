using UnityEngine;
using System.Collections;

public class PlayerControllerScript : MonoBehaviour
{
    #region Singleton
    public static PlayerControllerScript instance;

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

    #region Components
    [Header("Components")]
    public Rigidbody2D rb;                     // Player's Rigidbody2D component
    public Animator astronautAnim;             // Primary astronaut animator 
    public Animator laserAstronautAnim;        // Animator for astronaut with laser indicator
    #endregion

    #region Parameters
    [Header("Settings")]
    [SerializeField] private float movementSpeed;                       // Base horizontal movement speed
    [SerializeField] private float sensitivity;                         // Responsiveness of directional changes
    [SerializeField] private float fuelLossRate;                        // Fuel consumption rate during boost
    [SerializeField] private float minFuelLoss;                         // Minimum fuel loss for single boost
    [SerializeField] private float boostForce;                          // Upward force applied during boost
    [SerializeField] private float startingBoostForce;                  // Upward force applied during special initial score-based boost
    [SerializeField] private float relativeStartingBoostDistance;       // Fraction of previous score to reach with initial boost
    [SerializeField] private float scoreForBoostActivation;             // Minimum score to enable starting boost
    [SerializeField] private int failedBoostsForTip;                    // How often to fail a boost until geting a hint
    [SerializeField] private int timeTipText;                           // How long is the tip getting shown before dissapearing
    #endregion

    #region State Flags
    [Header("Player States")]
    public bool isAlive;                       // Player is alive: game started and is not over
    public bool falling;                       // Player fell down out of the screen
    public bool crashing;                      // Player hit a UFO from the bottom or side
    public bool boostKeyDown;                  // Use presses key for boost action
    public bool isBoostingWithKey;             // Player boosts in-game
    public bool boostMovement;                 // Player is moving upwards because of a boost (during and after)
    public bool startingBoost;                 // Initial boost should be activated
    public bool startingBoostEnabled;          // Initial boost is enabled in options panel and can appear in game
    #endregion

    #region Private Variables
    private float movement;                     // Current horizontal movement value
    private float currentDirection;             // Horizontal movement direction
    private bool failedBoostSfxPlayed;          // While pressing boost key and not having enough fuel, the sound effect was played
    private float lostFuel;                     // Fuel loss during single boost
    private int previousScore;                  // Score from previous game
    private bool isPausedPlayer;                // Pause state flag
    private int failedBoostAmount;              // Amount of failed boosts in a row
    private bool failedBoostTipOn;               // The failed boost tip is being shown
    private Vector2 storedVelocity;             // Velocity storage during pause
    private RigidbodyType2D originalBodyType;   // Original rigidbody type
    #endregion

    #region Initialization
    void Start()
    {
        InitializeComponents();
        ResetStates();
        LoadPotentialBoost();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void ResetStates()
    {
        isAlive = true;
        falling = false;
        crashing = false;
        failedBoostSfxPlayed = false;
        isBoostingWithKey = false;
        boostMovement = false;
        isPausedPlayer = false;
        movement = 0f;
        lostFuel = 0f;
        currentDirection = 0f;
        failedBoostAmount = 1;
        failedBoostTipOn = false;
    }

    private void LoadPotentialBoost()
    {
        if (PlayerPrefs.GetInt("StartingBoostEnabled", 1) == 1)
        {
            startingBoostEnabled = true;
        }
        else
        {
            startingBoostEnabled = false;
        }

        // Determine if player should start with a starting boost
        previousScore = PlayerPrefs.GetInt("Score", 0);
        startingBoost = (previousScore >= scoreForBoostActivation) && startingBoostEnabled;
    }
    #endregion

    #region Update Loop
    void Update()
    {
        HandlePauseState();
        HandleBoostAudio();

        // No game input when paused
        if (!isAlive || MainGameUIScript.instance.paused) return;

        HandleMovementInput();
        HandleBoostInput();
    }
    #endregion

    #region Input Management
    private void HandleMovementInput()
    {
        // Get movement direction from input
        float targetDirection = 0f;

        bool leftPressed = Input.GetKey(ManagerScript.instance.keyLeftPrimary) || Input.GetKey(ManagerScript.instance.keyLeftSecondary);
        bool rightPressed = Input.GetKey(ManagerScript.instance.keyRightPrimary) || Input.GetKey(ManagerScript.instance.keyRightSecondary);

        if (leftPressed) targetDirection -= 1f;
        if (rightPressed) targetDirection += 1f;

        // Smoothly transition between directions
        currentDirection = Mathf.MoveTowards(currentDirection, targetDirection, sensitivity * Time.deltaTime);


        // Calculate horizontal movement from direction and speed
        movement = currentDirection * movementSpeed;
    }

    private void HandleBoostInput()
    {
        // Astronaut uses boost íf he has enough fuel and the correct key gets pressed 
        boostKeyDown = Input.GetKey(ManagerScript.instance.keyBoostPrimary) || Input.GetKey(ManagerScript.instance.keyBoostSecondary);
        isBoostingWithKey = (boostKeyDown && MainGameUIScript.instance.currentFuel > 0 && !startingBoost);

        // Play SFX for failed boost when there's no fuel and key is pressed
        if (boostKeyDown && !startingBoost && MainGameUIScript.instance.currentFuel <= 0 && !failedBoostSfxPlayed)
        {
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.failedBoost, AudioManagerScript.instance.failedBoostVolume);
            failedBoostSfxPlayed = true;

            // Give a Hint if to many fails in a row
            if (!failedBoostTipOn)
            {
                failedBoostAmount++;
                if (failedBoostAmount >= failedBoostsForTip)
                {
                    StartCoroutine(SpawnFailedBoostTip());
                }
            }
        }

        // Reset failed boost counter on successful boost
        if (isBoostingWithKey) failedBoostAmount = 0;

        // Handle boost key release by managing minimum fuel consumption and resetting sound flag
        if (Input.GetKeyUp(ManagerScript.instance.keyBoostPrimary) || Input.GetKeyUp(ManagerScript.instance.keyBoostSecondary))
        {
            ApplyMinimumFuelLoss();
            lostFuel = 0f;
            failedBoostSfxPlayed = false;
        }
    }
    private IEnumerator SpawnFailedBoostTip()
    {
        failedBoostTipOn = true;
        MainGameUIScript.instance.failedBoostTip.SetActive(true);
        yield return MainGameUIScript.instance.WaitForSecondsUnpaused(timeTipText);
        MainGameUIScript.instance.failedBoostTip.SetActive(false);
        failedBoostAmount = 0;
        failedBoostTipOn = false;
    }

    private void ApplyMinimumFuelLoss()
    {
        // Ensure minimum fuel is consumed per boost activation
        if (lostFuel < minFuelLoss)
        {
            lostFuel++;                                       // To have rounding to -10% on fuelbar 
            float remainingFuel = minFuelLoss - lostFuel;
            MainGameUIScript.instance.currentFuel -= remainingFuel;

            if (MainGameUIScript.instance.currentFuel < 0)
            {
                MainGameUIScript.instance.currentFuel = 0;
            }
        }
    }
    #endregion

    #region Physics & Movement Management
    void FixedUpdate()
    {
        HandlePauseState();
        if (MainGameUIScript.instance.paused) return;

        Vector2 velocity = rb.linearVelocity;
        UpdateHorizontalMovement(ref velocity);
        HandleBoost(ref velocity);
        HandleInitialBoost(ref velocity);

        rb.linearVelocity = velocity;
    }

    private void UpdateHorizontalMovement(ref Vector2 velocity)
    {
        // Apply horizontal movement from input (movement-key pressed)
        velocity.x = movement;
    }

    private void HandleBoost(ref Vector2 velocity)
    {
        // Apply vertical boost and consume fuel from input (boost-key pressed)
        if (isBoostingWithKey)
        {
            velocity.y = boostForce;
            MainGameUIScript.instance.currentFuel -= fuelLossRate * Time.fixedDeltaTime;
            lostFuel += fuelLossRate * Time.fixedDeltaTime;

            boostMovement = true;   // Set boost upward movement flag...
        }

        if (boostMovement)
        {
            // ... and reset only when falling down again
            if (rb.linearVelocityY <= 0.5f)
            {
                boostMovement = false;
            }
        }
    }

    private void HandleInitialBoost(ref Vector2 velocity)
    {
        if (!startingBoost)
        {
            astronautAnim.SetBool("startboost", false);
            return;
        }

        // Calculate until when the initial boost should last (accounting for physics) to reach desired psoition
        float gravity = Mathf.Abs(rb.gravityScale * Physics2D.gravity.y);
        float physicsDistance = (startingBoostForce * startingBoostForce) / (2 * gravity);
        float physicsScore = physicsDistance * MainGameUIScript.instance.positionToScore;
        float targetScore = (previousScore * relativeStartingBoostDistance) - physicsScore;

        // Boost to calculated position
        float currentScore = transform.position.y * MainGameUIScript.instance.positionToScore;

        if (currentScore < targetScore)
        {
            velocity.y = startingBoostForce;
            astronautAnim.SetBool("startboost", true);
        }
        else
        {
            astronautAnim.SetBool("startboost", false);
            startingBoost = false;
        }

        boostMovement = true;        // Set boost upward movement flag...

        if (boostMovement)
        {
            // ... and reset only when falling down again
            if (rb.linearVelocityY <= 0.5f)
            {
                boostMovement = false;
            }
        }
    }
    #endregion

    #region Pause Management
    private void HandlePauseState()
    {
        if (MainGameUIScript.instance.paused)
        {
            // Freeze animations and store physics state
            astronautAnim.speed = 0;
            laserAstronautAnim.speed = 0;

            if (!isPausedPlayer)
            {
                originalBodyType = rb.bodyType;
                storedVelocity = rb.linearVelocity;

                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;

                isPausedPlayer = true;
            }
        }
        else
        {
            // Restore animations and physics state
            if (isPausedPlayer)
            {
                rb.bodyType = originalBodyType;
                rb.linearVelocity = storedVelocity;

                isPausedPlayer = false;
            }

            astronautAnim.speed = 1;
            laserAstronautAnim.speed = 1;
        }
    }
    #endregion

    #region Audio Management
    private void HandleBoostAudio()
    {
        // Control boost SFX
        if (MainGameUIScript.instance.paused)
        {
            AudioManagerScript.instance.PauseBoostSFX();
        }
        else if (startingBoost || isBoostingWithKey)
        {
            AudioManagerScript.instance.PlayBoostSFX();
        }
        else
        {
            AudioManagerScript.instance.StopBoostSFX();
        }
    }
    #endregion

    #region Collision Handling
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Trigger death sequence and game over
        if (collision.gameObject.CompareTag("End"))
        {
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.fall, AudioManagerScript.instance.fallVolume);
            isAlive = false;
            falling = true;
            Destroy(gameObject);
            ManagerScript.instance.GameOverScreen();
        }
    }
    #endregion
}