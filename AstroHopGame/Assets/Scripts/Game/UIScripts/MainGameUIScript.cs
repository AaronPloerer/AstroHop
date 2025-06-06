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
    public Button pauseButton;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject warningMainMenuPanel;
    public GameObject warningRetryPanel;
    public GameObject fallingText;
    public GameObject crashingText;
    #endregion

    #region Pause System
    [Header("Pause System")]
    public bool paused;
    [SerializeField] private float pauseCooldownDuration;
    private float pauseCooldownTimer;
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
        InitializeTipSystem();
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
        pauseCooldownTimer = 0;
    }

    private void InitializeFuelSystem()
    {
        // Configure slider range and initial state
        rocketSlider.maxValue = maxFuel;
        currentFuel = 0;
        instance.rocketSlider.value = currentFuel;
    }

    private void InitializeTipSystem()
    {
        SetUpTipVariables();
        SetUpControlsTutorials();
        CheckShownTutorials();
    }

    private void SetUpTipVariables()
    {
        // Initialize variables for tip spawning
        failedBoostAmount = 0;
        failedBoostTipOn = false;
    }

    private void SetUpControlsTutorials()
    {
        // Load customized controls from chosen language from controls panel
        string boostKeyString;
        string leftKeyString;
        string rightKeyString;
        string pauseKeyString;

        try
        {
            KeyCode boostKey = (KeyCode)PlayerPrefs.GetInt("KeyBoostPrimary", (int)KeyCode.W);
            KeyCode leftKey = (KeyCode)PlayerPrefs.GetInt("KeyLeftPrimary", (int)KeyCode.A);
            KeyCode rightKey = (KeyCode)PlayerPrefs.GetInt("KeyRightPrimary", (int)KeyCode.D);
            KeyCode pauseKey = (KeyCode)PlayerPrefs.GetInt("KeyPause", (int)KeyCode.Space);

            boostKeyString = MenuUIScript.instance.GetLocalizedKeyName(boostKey);
            leftKeyString = MenuUIScript.instance.GetLocalizedKeyName(leftKey);
            rightKeyString = MenuUIScript.instance.GetLocalizedKeyName(rightKey);
            pauseKeyString = MenuUIScript.instance.GetLocalizedKeyName(pauseKey);
        }
        catch
        {
            // Fallback to default keys
            if (PlayerPrefs.GetInt("Language", 0) == 3)
            {
                boostKeyString = "Z";
                leftKeyString = "Q";
                rightKeyString = "D";
                pauseKeyString = "Spacebar";
            }
            else
            {
                boostKeyString = "W";
                leftKeyString = "A";
                rightKeyString = "D";
                pauseKeyString = "Spacebar";
            }
        }

        // Select current language; 0 = English default
        int localeID = PlayerPrefs.GetInt("Language", 0);

        // Create tutorial texts based on choesen language and chosen controls
        if (movingTutorial != null)
        {
            movingTutorial.text = localeID switch
            {
                1 => $"Drücke {leftKeyString} und {rightKeyString}, um nach links und rechts zu gehen.\n<size=30><alpha=#80>Drücke [Alt], um das Tutorial zu überspringen.",
                2 => $"Premi {leftKeyString} e {rightKeyString} per andare a sinistra e destra.\n<size=30><alpha=#80>Premi [Alt] per saltare il tutorial.",
                3 => $"Appuyez sur {leftKeyString} et {rightKeyString} pour aller à gauche et à droite.\n<size=30><alpha=#80>Appuie sur [Alt] pour pour passer le tutoriel.",
                _ => $"Press {leftKeyString} and {rightKeyString} to go left and right.\n<size=30><alpha=#80>Press [Alt] to skip the tutorial."
            };
        }

        if (boostTutorial != null)
        {
            boostTutorial.text = localeID switch
            {
                1 => $"Drücke {boostKeyString}, um den Raketenboost zu nutzen und alle UFOs zu zerstören.\n<size=30><alpha=#80>Drücke [Alt], um das Tutorial zu überspringen.",
                2 => $"Premi {boostKeyString} per attivare il boost e distruggere tutti gli UFO.\n<size=30><alpha=#80>Premi [Alt] per saltare il tutorial.",
                3 => $"Appuyez sur {boostKeyString} pour vous propulser et détruire tous les OVNIs.\n<size=30><alpha=#80>Appuie sur [Alt] pour pour passer le tutoriel.",
                _ => $"Press {boostKeyString} to boost up and destroy all UFOs.\n<size=30><alpha=#80>Press [Alt] to skip the tutorial."
            };
        }

        if (pauseTutorial != null)
        {
            pauseTutorial.text = localeID switch
            {
                1 => $"Drücke {pauseKeyString}, um das Spiel zu pausieren.\n<size=30><alpha=#80>Drücke [Alt], um das Tutorial zu überspringen.",
                2 => $"Premi {pauseKeyString} per mettere in pausa il gioco.\n<size=30><alpha=#80>Premi [Alt] per saltare il tutorial.",
                3 => $"Appuyez sur {pauseKeyString} pour mettre le jeu en pause.\n<size=30><alpha=#80>Appuie sur [Alt] pour passer le tutoriel.",
                _ => $"Press {pauseKeyString} to pause the game.\n<size=30><alpha=#80>Press [Alt] to skip the tutorial."
            };
        }
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

        PauseWithKey();
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

    #region Pause Management
    private void PauseWithKey()
    {
        // Count time since last pause 
        pauseCooldownTimer += Time.deltaTime;

        // Check for pause key press
        if (Input.GetKeyDown(ManagerScript.instance.keyPause))
        {
            PauseWithKeyResponse();
        }
    }

    private void PauseWithKeyResponse()
    {
        // Don't process pause input, if game is not playing or if cooldown is still active (to prevent double-clicking and spamming)
        if (!PlayerControllerScript.instance.isAlive ||
        gameOverPanel.activeSelf ||
        pauseCooldownTimer < pauseCooldownDuration)
        {
            return;
        }

        // Inverse current pause panel state
        if (!pausePanel.activeSelf)
        {
            ManagerScript.instance.PauseGame();
        }
        else
        {
            ManagerScript.instance.ContinueGame();
        }

        // Reset cooldown
        pauseCooldownTimer = 0;
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
        float fuelPercentage = (currentFuel / maxFuel) * 100;
        int displayPercentage = Mathf.Clamp(Mathf.FloorToInt(fuelPercentage), 0, 100);
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
        if (PlayerControllerScript.instance.isBoostingWithKey && MainGameUIScript.instance.paused == false)
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
        if (PlayerControllerScript.instance.isBoostingWithKey && MainGameUIScript.instance.paused == false)
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
        // Skip complete tutorial and save it as seen when pressing any Alt key
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            SkipTutorials();
        }

        int currentPhaseIndex = LevelGeneratorScript.instance.currentPhaseIndex;
        Phase[] phases = LevelGeneratorScript.instance.phases;

        // Safety checks for valid phase index and initialization status
        if (currentPhaseIndex >= 0 && currentPhaseIndex < phases.Length && shownTutorial != null)
        {
            // Check from persistent storage if current tutorial was not ýet shown
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
                    //SkipCurrentTutorial(currentPhaseIndex);
                }

                // Start the coroutine using eight separate parameters (plus phaseIndex)
                activeTutorialCoroutine = StartCoroutine(SpawnSequence(
                    currentPhase.firstTutorial,
                    currentPhase.secondTutorial,
                    currentPhase.thirdTutorial,
                    currentPhase.fourthTutorial,
                    currentPhase.fifthTutorial,
                    currentPhase.sixthTutorial,
                    currentPhase.seventhTutorial,
                    currentPhase.eighthTutorial,
                    currentPhaseIndex
                ));
                activeTutorialPhaseIndex = currentPhaseIndex;
            }
        }
    }

    private void SkipTutorials()
    {
        if (activeTutorialCoroutine != null)
        {
            StopCoroutine(activeTutorialCoroutine);
        }

        foreach (Phase phase in LevelGeneratorScript.instance.phases)
        {
            if (phase.firstTutorial != null) phase.firstTutorial.SetActive(false);
            if (phase.secondTutorial != null) phase.secondTutorial.SetActive(false);
            if (phase.thirdTutorial != null) phase.thirdTutorial.SetActive(false);
            if (phase.fourthTutorial != null) phase.fourthTutorial.SetActive(false);
            if (phase.fifthTutorial != null) phase.fifthTutorial.SetActive(false);
            if (phase.sixthTutorial != null) phase.sixthTutorial.SetActive(false);
            if (phase.seventhTutorial != null) phase.seventhTutorial.SetActive(false);
            if (phase.eighthTutorial != null) phase.eighthTutorial.SetActive(false);
        }

        // Mark ALL tutorials as shown
        for (int i = 0; i < shownTutorial.Length; i++)
        {
            shownTutorial[i] = true; // Set in memory
            PlayerPrefs.SetInt("Phase" + i + "TutorialShown", 1); // Set in storage
        }
        PlayerPrefs.Save();

        // Clear references
        activeTutorialCoroutine = null;
        activeTutorialPhaseIndex = -1;
        return;                               // Exit and prevent any further execution
    }

    private void SkipCurrentTutorial(int currentPhaseIndex)
    {
        StopCoroutine(activeTutorialCoroutine);

        foreach (Phase phase in LevelGeneratorScript.instance.phases)
        {
            if (phase.firstTutorial != null) phase.firstTutorial.SetActive(false);
            if (phase.secondTutorial != null) phase.secondTutorial.SetActive(false);
            if (phase.thirdTutorial != null) phase.thirdTutorial.SetActive(false);
            if (phase.fourthTutorial != null) phase.fourthTutorial.SetActive(false);
            if (phase.fifthTutorial != null) phase.fifthTutorial.SetActive(false);
            if (phase.sixthTutorial != null) phase.sixthTutorial.SetActive(false);
            if (phase.seventhTutorial != null) phase.seventhTutorial.SetActive(false);
            if (phase.eighthTutorial != null) phase.eighthTutorial.SetActive(false);
        };

        // Save in persistent storage that the tutorial was shown
        PlayerPrefs.SetInt("Phase" + currentPhaseIndex + "TutorialShown", 1);
        PlayerPrefs.Save();

        activeTutorialCoroutine = null;            // Reset active coroutine reference
        activeTutorialPhaseIndex = -1;             // Reset phase index
    }

    private IEnumerator SpawnSequence(
       GameObject firstText,
       GameObject secondText,
       GameObject thirdText,
       GameObject fourthText,
       GameObject fifthText,
       GameObject sixthText,
       GameObject seventhText,
       GameObject eighthText,
       int phaseIndex)
    {
        // Helper local to show/hide a single text with timing
        IEnumerator ShowTutorialElement(GameObject element)
        {
            element.SetActive(true);
            yield return WaitForSecondsUnpaused(timeTutorialText);
            element.SetActive(false);
        }

        // 1st element
        if (firstText != null)
        {
            yield return ShowTutorialElement(firstText);
        }

        // Buffer
        yield return WaitForSecondsUnpaused(timeTutorialPause);

        // 2nd element
        if (secondText != null)
        {
            yield return ShowTutorialElement(secondText);
        }

        // Buffer
        yield return WaitForSecondsUnpaused(timeTutorialPause);

        // 3rd element
        if (thirdText != null)
        {
            yield return ShowTutorialElement(thirdText);
        }

        // Buffer
        yield return WaitForSecondsUnpaused(timeTutorialPause);

        // 4th element
        if (fourthText != null)
        {
            yield return ShowTutorialElement(fourthText);
        }

        // Buffer
        yield return WaitForSecondsUnpaused(timeTutorialPause);

        // 5th element
        if (fifthText != null)
        {
            yield return ShowTutorialElement(fifthText);
        }

        // Buffer
        yield return WaitForSecondsUnpaused(timeTutorialPause);

        // 6th element
        if (sixthText != null)
        {
            yield return ShowTutorialElement(sixthText);
        }

        // Buffer
        yield return WaitForSecondsUnpaused(timeTutorialPause);

        // 7th element
        if (seventhText != null)
        {
            yield return ShowTutorialElement(seventhText);
        }

        // Buffer
        yield return WaitForSecondsUnpaused(timeTutorialPause);

        // 8th element
        if (eighthText != null)
        {
            yield return ShowTutorialElement(eighthText);
        }

        // Mark tutorial as shown in PlayerPrefs
        PlayerPrefs.SetInt("Phase" + phaseIndex + "TutorialShown", 1);
        PlayerPrefs.Save();

        // Reset active coroutine reference
        activeTutorialCoroutine = null;
        activeTutorialPhaseIndex = -1;
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
