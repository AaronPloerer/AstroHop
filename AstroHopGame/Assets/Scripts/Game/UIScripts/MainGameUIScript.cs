using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LevelGeneratorScript;

public class MainGameUIScript : MonoBehaviour
{
    #region Singleton
    public static MainGameUIScript instance;
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

    #region UI References
    [Header("Score Elements")]
    public TMP_Text scoreText;
    public TMP_Text highscoreText;
    public TMP_Text finalScore;
    public TMP_Text highScore;

    [Header("Slider Elements")]
    [SerializeField] private Slider rocketSlider;
    [SerializeField] private GameObject rocketSliderFill, rocketSliderBackground;
    [SerializeField] private Sprite lowFuelColor, highFuelColor, fullFuelColor;
    [SerializeField] private TMP_Text percentageText;
    [SerializeField] private Animator sliderBackgroundAnim, sliderFillAnim;

    [Header("Tutorial Elements")]
    [SerializeField] private TMP_Text movingTutorial, boostTutorial, pauseTutorial;
    public GameObject tutorials;

    [Header("Tip Elements")]
    [SerializeField] private int timeTipText;                       
    public GameObject failedBoostTip;
    [SerializeField] private int failedBoostsForTip;                  
    public int failedBoostAmount;
    private bool failedBoostTipOn;
    public GameObject failedPickUpTip;
    [SerializeField] private int failedPickUpForTip;
    public int failedPickUpAmount;
    private bool failedPickUpTipOn;

    [Header("Panel Elements")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject warningMainMenuPanel;
    public GameObject warningRetryPanel;
    public GameObject fallingText;
    public GameObject crashingText;

    [Header("Buttons")]
    public Button pauseButton;
    public GameObject aimConroller;
    public BoostButton boostButton;
    #endregion

    #region Pause System
    [Header("Pause System")]
    public bool paused;
    #endregion

    #region Cursor System
    [Header("Cursor System")]
    public Collider2D validClickArea;
    public bool laserIndicatorActive;
    #endregion

    #region Score System
    [Header("Score Settings")]
    public float positionToScore;
    public int highScoreNumber;
    private float highestPos;
    #endregion

    #region Fuel System
    [Header("Fuel Settings")]
    public float currentFuel, maxFuel, lowFuelWarningValue;
    #endregion

    #region Tutorial System
    [Header("Tutorial System")]
    [SerializeField] private float timeTutorialText, timeTutorialPause;
    private bool[] shownTutorial;
    private Coroutine activeTutorialCoroutine;   // Track the active tutorial coroutine for force stop
    private int activeTutorialPhaseIndex = -1;   // Track the phase index of the active tutorial for force stop
    #endregion

    #region Initialization
    private void Start()
    {
        InitializeGameState();
        InitializeFuelSystem();
        InitializeTipTutorialSystem();
    }

    private void InitializeGameState()
    {
        // Set initial laser cursor 
        ManagerScript.instance.SetPixelCursor(ManagerScript.instance.laserCursor, 0.5f, 0.5f);

        // Initialize score display and highscore display from persistent storage
        int score = 0;
        scoreText.text = score.ToString();
        int highscore = PlayerPrefs.GetInt("HighScore", 0);
        highscoreText.text = highscore.ToString();

        // Track initial player position for scoring
        highestPos = PlayerControllerScript.instance.transform.position.y;

        // Reset flags and timer
        laserIndicatorActive = false;
        paused = false;
    }

    private void InitializeFuelSystem()
    {
        // Configure slider range and initial state
        rocketSlider.maxValue = maxFuel;
        currentFuel = 0;
        instance.rocketSlider.value = currentFuel;
    }

    private void InitializeTipTutorialSystem()
    {
        SetUpTipVariables();;
        CheckShownTutorials();
    }

    private void SetUpTipVariables()
    {
        // Initialize variables for tip spawning
        failedBoostAmount = 0;
        failedBoostTipOn = false;
    }
    private void CheckShownTutorials()
    {
        //  To prevent repeating a tutorial: check the persistent storage, which phase tutorials were already shown
        shownTutorial = new bool[LevelGeneratorScript.instance.phases.Length];
        for (int i = 0; i < shownTutorial.Length; i++)
        {
            shownTutorial[i] = PlayerPrefs.GetInt("Phase" + i + "TutorialShown", 0) == 1;
        }
    }
    #endregion

    #region Main Update
    private void Update()
    {
        // Stop uddating gameplay system if player reference is missing
        if (PlayerControllerScript.instance == null) return;

        UpdateScoreDisplays();
        HandleFuelSystem();
        UpdateCursor();
        HandleTutorials();
    }
    #endregion

    #region Cursor Management
    private void UpdateCursor()
    {
        Vector2 mousePosition = GetMouseWorldPosition2D();

        // Basic cursor during paused/dead states
        if (paused || !PlayerControllerScript.instance.isAlive)
        {
            ManagerScript.instance.SetPixelCursor(ManagerScript.instance.basicCursor, 0f, 0f);
        }
        else
        {
            // Laser cursor when hovering game view area
            if (validClickArea.OverlapPoint(mousePosition))
            {
                if (laserIndicatorActive)
                {
                    // When firing a laser, the cursor turns red to indicate it
                    ManagerScript.instance.SetPixelCursor(ManagerScript.instance.laserCursorActive, 0.5f, 0.5f);
                }
                else
                {
                    ManagerScript.instance.SetPixelCursor(ManagerScript.instance.laserCursor, 0.5f, 0.5f);
                }
            }
            else
            {
                // Basic cursor when not hovering game view area
                ManagerScript.instance.SetPixelCursor(ManagerScript.instance.basicCursor, 0f, 0f);
            }
        }
    }

    private Vector2 GetMouseWorldPosition2D()
    {
        // Convert screen position to game world position
        Vector3 mousePos = Input.mousePosition;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
    #endregion

    #region Score Management
    private void UpdateScoreDisplays()
    {
        // Only update score if camera reaches new height record
        if ((CameraScript.instance.transform.position.y) > highestPos && (CameraScript.instance.transform.position.y) > 0) 
        {
            // Convert height to score using position multiplier
            int score = Mathf.FloorToInt(CameraScript.instance.transform.position.y * positionToScore);
            scoreText.text = score.ToString();

            // Update highscore to current score if surpassed
            int highscore = PlayerPrefs.GetInt("HighScore", 0);
            if (score > highscore)
            {
                PlayerPrefs.SetInt("HighScore", score);
                highscore = PlayerPrefs.GetInt("HighScore", 0);
            }
            highscoreText.text = highscore.ToString();

            // Update new camera height record
            highestPos = (CameraScript.instance.transform.position.y);
        }
    }
    #endregion

    #region Fuel Management
    private void HandleFuelSystem()
    {
        UpdateFuelValues();
        UpdatePercentageDisplay();
        HandleFuelAnimations();
    }

    private void UpdateFuelValues()
    {
        // Update fuel quantity value within valid range
        currentFuel = Mathf.Clamp(currentFuel, 0, maxFuel);
        instance.rocketSlider.value = currentFuel;
    }

    private void UpdatePercentageDisplay()
    {
        // Calculate and format percentage (0-100)
        int displayPercentage;

        if (currentFuel <= 0f)
        {
            displayPercentage = 0;
        }
        else if (currentFuel >= maxFuel)
        {
            displayPercentage = 100;
        }
        else
        {
            float fuelPercentage = (currentFuel / maxFuel) * 100;
            displayPercentage = Mathf.Clamp(Mathf.RoundToInt(fuelPercentage), 1, 99);
        }

        percentageText.text = displayPercentage.ToString() + "%";
    }

    private void HandleFuelAnimations()
    {
        HandleSliderAnimation();
        HandlePlayerAnimation();
    }

    private void HandleSliderAnimation()
    {
        // Change fill animation based on fuel percentage
        if (rocketSlider.value == maxFuel)
        {
            sliderFillAnim.SetTrigger("max");
        }
        else if (rocketSlider.value < lowFuelWarningValue)
        {
            sliderFillAnim.SetTrigger("low");
        }
        else
        {
            sliderFillAnim.SetTrigger("high"); ;
        }

        // Handle boost effect animation while boosting
        if (PlayerControllerScript.instance.isBoostingWithButton && MainGameUIScript.instance.paused == false)
        {
            sliderBackgroundAnim.SetBool("boosting", true);
        }
        else
        {
            sliderBackgroundAnim.SetBool("boosting", false);
        }
    }

    private void HandlePlayerAnimation()
    {
        // Control boost animation for astronaut character
        if (PlayerControllerScript.instance.isBoostingWithButton && MainGameUIScript.instance.paused == false)
        {
            PlayerControllerScript.instance.astronautAnim.SetBool("boost", true);
        }
        else
        {
            PlayerControllerScript.instance.astronautAnim.SetBool("boost", false);
        }

        // Modify boost animation depending of fuel percentage
        if (rocketSlider.value < lowFuelWarningValue)
        {
            PlayerControllerScript.instance.astronautAnim.SetBool("low", true);
        }
        else
        {
            PlayerControllerScript.instance.astronautAnim.SetBool("low", false);
        }
    }
    #endregion

    #region Tutorial Management
    private void HandleTutorials()
    {
        int currentPhaseIndex = LevelGeneratorScript.instance.currentPhaseIndex;
        Phase[] phases = LevelGeneratorScript.instance.phases;

        // Safety checks for valid phase index and initialization status
        if (currentPhaseIndex >= 0 && currentPhaseIndex < phases.Length && shownTutorial != null)
        {
            // Check from persistent storage if current tutorial was not yet shown
            Phase currentPhase = phases[currentPhaseIndex];
            bool tutorialNotSpawned = !shownTutorial[currentPhaseIndex];

            // Spawn tutorial sequence if conditions met
            if (tutorialNotSpawned) 
            {
                // Tutorial is being shown: do not repeat this if for the phase
                shownTutorial[currentPhaseIndex] = true;

                // If there's an active tutorial: stop it, then mark its phase as shown
                if (activeTutorialCoroutine != null)
                {
                    SkipCurrentTutorial(currentPhaseIndex);
                }

                activeTutorialCoroutine = StartCoroutine(SpawnSequence(currentPhase.tutorial, currentPhaseIndex));
                activeTutorialPhaseIndex = currentPhaseIndex;
            }
        }
    }

    private void SkipCurrentTutorial(int currentPhaseIndex)
    {
        StopCoroutine(activeTutorialCoroutine);

        foreach (Phase phase in LevelGeneratorScript.instance.phases)
        {
            if (phase.tutorial != null) phase.tutorial.SetActive(false);
        };

        // Save in persistent storage that the tutorial was shown
        PlayerPrefs.SetInt("Phase" + currentPhaseIndex + "TutorialShown", 1);
        PlayerPrefs.Save();

        activeTutorialCoroutine = null;            // Reset active coroutine reference
        activeTutorialPhaseIndex = -1;             // Reset phase index
    }

    private IEnumerator SpawnSequence(GameObject firsttext, int phaseIndex)
    {
        // Tutorial element sequence
        if (firsttext != null)
        {
            firsttext.SetActive(true);
            yield return WaitForSecondsUnpaused(timeTutorialText);
            firsttext.SetActive(false);
        }

        // Save in persistent storage that the tutorial was shown
        PlayerPrefs.SetInt("Phase" + phaseIndex + "TutorialShown", 1);
        PlayerPrefs.Save();

        activeTutorialCoroutine = null;            // Reset active coroutine reference
        activeTutorialPhaseIndex = -1;             // Reset phase index
    }
    #endregion

    #region Tip Management
    public void FailedBoostTip()
    {
        if (!failedBoostTipOn)
        {
            failedBoostAmount++;
            //
            if (failedBoostAmount >= failedBoostsForTip && !failedPickUpTipOn && (PlayerPrefs.GetInt("InGameTipsEnabled", 1) == 1))
            {
                StartCoroutine(SpawnFailedBoostTip());
            }
        }
    }

    private IEnumerator SpawnFailedBoostTip()
    {
        failedBoostTipOn = true;
        failedBoostTip.SetActive(true);

        yield return WaitForSecondsUnpaused(timeTipText);

        failedBoostTip.SetActive(false);
        failedBoostAmount = 0;
        failedBoostTipOn = false;
    }

    public void FailedPickUpTip()
    {
        if (!failedPickUpTipOn)
        {
            failedPickUpAmount++;
            if (failedPickUpAmount >= failedPickUpForTip && !failedBoostTipOn && (PlayerPrefs.GetInt("InGameTipsEnabled", 1) == 1))
            {
                StartCoroutine(SpawnFailedPickUpTip());
            }
        }
    }

    private IEnumerator SpawnFailedPickUpTip()
    {
        failedPickUpTipOn = true;
        failedPickUpTip.SetActive(true);

        yield return WaitForSecondsUnpaused(timeTipText);

        failedPickUpTip.SetActive(false);
        failedPickUpAmount = 0;
        failedPickUpTipOn = false;
    }
    #endregion

    #region WaitForSecondsUnpaused
    public IEnumerator WaitForSecondsUnpaused(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (!paused) // Paused-Status wird global verwaltet
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
    }
    #endregion
}
