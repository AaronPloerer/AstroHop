using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using UnityEngine.EventSystems;
public class ManagerScript : MonoBehaviour
{
    #region Singleton
    public static ManagerScript instance;

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

    #region Initialization
    private void Start()
    {
        InitializeInput();
        InitializeLanguage();
    }
    #endregion

    #region Escape Button Logic
    private float escapeCooldownTimer = 0f;
    [SerializeField] private float escapeCooldownTime;

    private void Update()
    {
        // Update cooldown timer
        escapeCooldownTimer += Time.deltaTime;

        // Ignore key press if cooldowndown timer is not done
        if (escapeCooldownTimer <= escapeCooldownTime) return;

        // Close current interface when Escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (MainGameUIScript.instance != null && MainGameUIScript.instance.pausePanel.activeSelf)
            {
                ContinueGame();
            }
            else if (MainGameUIScript.instance != null && MainGameUIScript.instance.warningMainMenuPanel.activeSelf)
            {
                PauseGame();
            }
            else if (MainGameUIScript.instance != null && MainGameUIScript.instance.warningRetryPanel.activeSelf)
            {
                PauseGame();
            }
            else if (MenuUIScript.instance != null && MenuUIScript.instance.deleteProgressConfirmPanel.activeSelf)
            {
                CloseDeleteProgressConfirmPanel();
            }
            else if (MenuUIScript.instance != null && MenuUIScript.instance.deleteProgressPanel.activeSelf)
            {
                CloseDeleteProgressPanel();
            }
            else if (MenuUIScript.instance != null && MenuUIScript.instance.optionsPanel.activeSelf)
            {
                CloseOptionsPanel();
            }
            else if (MenuUIScript.instance != null && MenuUIScript.instance.helpPanel.activeSelf)
            {
                CloseHelpPanel();
            }
            else if (MenuUIScript.instance != null && MenuUIScript.instance.exitWindowWarningPanel.activeSelf)
            {
                CloseExitWindowWarning();
            }
            else if (MainGameUIScript.instance != null)
            {
                PauseGame();
            }
            else if (MenuUIScript.instance != null)
            {
                OpenExitWinodwWarning();
            }

            // Reset cooldown time
            escapeCooldownTimer = 0f;
        }
    }
    #endregion

    #region Input Management
    [Header("Input Bindings")]
    public KeyCode keyBoostPrimary;        // Primary key binding for boost
    public KeyCode keyBoostSecondary;      // Secondary key binding for boost
    public KeyCode keyLeftPrimary;         // Primary key binding for left movement
    public KeyCode keyLeftSecondary;       // Secondary key binding for left movement
    public KeyCode keyRightPrimary;        // Primary key binding for right movement
    public KeyCode keyRightSecondary;      // Secondary key binding for right movement
    public KeyCode keyPause;               // Key binding for pause

    void InitializeInput()
    {
        // Load saved key bindings from permanent storage or use defaults
        keyBoostPrimary = (KeyCode)PlayerPrefs.GetInt("KeyBoostPrimary", (int)KeyCode.W);
        keyBoostSecondary = (KeyCode)PlayerPrefs.GetInt("KeyBoostSecondary", (int)KeyCode.UpArrow);
        keyLeftPrimary = (KeyCode)PlayerPrefs.GetInt("KeyLeftPrimary", (int)KeyCode.A);
        keyLeftSecondary = (KeyCode)PlayerPrefs.GetInt("KeyLeftSecondary", (int)KeyCode.LeftArrow);
        keyRightPrimary = (KeyCode)PlayerPrefs.GetInt("KeyRightPrimary", (int)KeyCode.D);
        keyRightSecondary = (KeyCode)PlayerPrefs.GetInt("KeyRightSecondary", (int)KeyCode.RightArrow);
        keyPause = (KeyCode)PlayerPrefs.GetInt("KeyPause", (int)KeyCode.Space);
    }
    #endregion

    #region Localization
    private bool localizationActive = false;     // Flag to prevent overlapping locale changes

    void InitializeLanguage()
    {
        // Load saved language preference or default to first option
        int localeID = PlayerPrefs.GetInt("Language", 0);
        StartCoroutine(SetLocale(localeID));
    }

    public void LanguageDropdown()
    {
        // Add click sound when selecting language
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);

        // Load language from UI language dropdown
        int selectedLocale = MenuUIScript.instance.languageDropdown.value;

        if (localizationActive == true)  return;     // Prevent multiple simultaneous locale changes

        StartCoroutine(SetLocale(selectedLocale));
    }

    private IEnumerator SetLocale(int localeID)
    {
        localizationActive = true;                                                                                      // Set lock flag
        yield return LocalizationSettings.InitializationOperation;                                                      // Wait until the localization system is ready 
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];                  // Change lactive language
        PlayerPrefs.SetInt("Language", localeID);                                                                       // Save selected language in permanenet storage
        localizationActive = false;                                                                                     // Release lock flag
        if (MenuUIScript.instance != null)
        {
            MenuUIScript.instance.RefreshKeyDisplay(MenuUIScript.instance.boostInputField, "KeyBoostPrimary", KeyCode.W);      // Localize control displays inputs  
            MenuUIScript.instance.RefreshKeyDisplay(MenuUIScript.instance.leftInputField, "KeyLeftPrimary", KeyCode.A);
            MenuUIScript.instance.RefreshKeyDisplay(MenuUIScript.instance.rightInputField, "KeyRightPrimary", KeyCode.D);
            MenuUIScript.instance.RefreshKeyDisplay(MenuUIScript.instance.pauseInputField, "KeyPause", KeyCode.Space);
        }
    }
    #endregion

    #region Cursor Management
    [Header("Cursor Configuration")]
    [SerializeField] private float cursorScale;      // Multiplier for cursor texture scaling
    public Sprite basicCursor;                       // Default cursor appearence
    public Sprite laserCursor;                       // Cursor appearence when playing of game view area
    public Sprite laserCursorActive;                 // Game cursor when laser indicator is on (clicked to shoot)
    public void SetPixelCursor(Sprite cursorSprite, float hotspotRight, float hotspotDown)
    {
        if (cursorSprite == null) return;

        // Get original sprite dimensions
        int originalWidth = (int)cursorSprite.textureRect.width;
        int originalHeight = (int)cursorSprite.textureRect.height;

        // Calculate scaled dimensions
        int scaledWidth = Mathf.RoundToInt(originalWidth * cursorScale);
        int scaledHeight = Mathf.RoundToInt(originalHeight * cursorScale);

        // Create pixel-perfect texture
        Texture2D cursorTexture = new Texture2D(scaledWidth, scaledHeight, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,        // Keep pixels sharp
            wrapMode = TextureWrapMode.Clamp      // Prevent edge bleeding
        };

        // Get original pixels
        Color[] basePixels = cursorSprite.texture.GetPixels((int)cursorSprite.textureRect.x, (int)cursorSprite.textureRect.y, originalWidth, originalHeight);

        // Get pixel list for future upscaled cursor
        Color[] scaledPixels = new Color[scaledWidth * scaledHeight];

        // Select every original pixel for upscaling
        for (int y = 0; y < originalHeight; y++)
        {
            for (int x = 0; x < originalWidth; x++)
            {
                Color pixel = basePixels[x + y * originalWidth];

                // Replicate each pixel (cursorScale x cursorScale) times to scale
                for (int sy = 0; sy < cursorScale; sy++)
                {
                    for (int sx = 0; sx < cursorScale; sx++)
                    {
                        int scaledX = x * Mathf.RoundToInt(cursorScale) + sx;
                        int scaledY = y * Mathf.RoundToInt(cursorScale) + sy;

                        // Prevent out-of-bounds array access
                        if (scaledX < scaledWidth && scaledY < scaledHeight)
                        {
                            scaledPixels[scaledX + scaledY * scaledWidth] = pixel;
                        }
                    }
                }
            }
        }

        // Apply scaled pixels on texture
        cursorTexture.SetPixels(scaledPixels);
        cursorTexture.Apply();

        // Calculate hotspot (click point) position
        Vector2 scaledHotspot = new Vector2(scaledWidth * hotspotRight, scaledHeight * hotspotDown);

        // Set new texture and hotspot on cursor
        Cursor.SetCursor(cursorTexture, scaledHotspot, CursorMode.ForceSoftware);
    }
    #endregion

    #region Audio Management
    public void ChangeMusicVolume()
    {
        // Set and save music volume to permanent storage
        float selectedVolume = MenuUIScript.instance.musicSlider.value;
        PlayerPrefs.SetFloat("MusicVolume", selectedVolume);

        // Update music source immediately
        PlayerPrefs.Save();
    }

    public void ChangeSFXVolume()
    {
        // Set and save SFX volume to permanent storage
        float selectedVolume = MenuUIScript.instance.musicSlider.value;
        PlayerPrefs.SetFloat("SfxVolume", selectedVolume);

        // Update SFX source immediately
        PlayerPrefs.Save();
    }
    #endregion

    #region Toggles Management
    public void ChangeStartingBoostToggle()
    {
        if (PlayerPrefs.GetInt("StartingBoostEnabled", 1) == 1)
        {
            PlayerPrefs.SetInt("StartingBoostEnabled", 0);
            MenuUIScript.instance.startingBoostToggleGraphic.sprite = MenuUIScript.instance.toggleOffSprite;
            MenuUIScript.instance.startingBoostToggleGraphic.enabled = true;
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
        }
        else
        {
            PlayerPrefs.SetInt("StartingBoostEnabled", 1);
            MenuUIScript.instance.startingBoostToggleGraphic.sprite = MenuUIScript.instance.toggleOnSprite;
            MenuUIScript.instance.startingBoostToggleGraphic.enabled = true;
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        }
    }

    public void ChangeInGameTipsToggle()
    {
        if (PlayerPrefs.GetInt("InGameTipsEnabled", 1) == 1)
        {
            PlayerPrefs.SetInt("InGameTipsEnabled", 0);
            MenuUIScript.instance.inGameTipsToggleGraphic.sprite = MenuUIScript.instance.toggleOffSprite;
            MenuUIScript.instance.inGameTipsToggleGraphic.enabled = true;
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
        }
        else
        {
            PlayerPrefs.SetInt("InGameTipsEnabled", 1);
            MenuUIScript.instance.inGameTipsToggleGraphic.sprite = MenuUIScript.instance.toggleOnSprite;
            MenuUIScript.instance.inGameTipsToggleGraphic.enabled = true;
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        }
    }
    #endregion

    #region Scene Management
    public void LoadLoadingSceneOnClick()
    {
        // Start coroutine to handle sound and scene change
        StartCoroutine(LoadSceneAfterTimer());
    }

    public void LoadGameScene()
    {
        // Start coroutine to handle sound and scene change
        StartCoroutine(LoadSceneAfterSound("GameScene"));
    }

    public void LoadMenuSceneOnClick()
    {        
        // Start coroutine to handle sound and scene change
        StartCoroutine(LoadSceneAfterSound("MenuScene"));
    }

    private IEnumerator LoadSceneAfterSound(string sceneName)
    {
        // Play click sound
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);

        // Disable input
        EventSystem.current.enabled = false;

        // Wait for the duration of the click sound
        yield return new WaitForSeconds(AudioManagerScript.instance.click.length);

        // Load the scene after waiting
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator LoadSceneAfterTimer()
    {
        // Play click sound
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);

        // Disable input
        EventSystem.current.enabled = false;

        // Wait for the duration of the click sound
        yield return new WaitForSeconds(AudioManagerScript.instance.click.length);

        // Wait for loading time
        yield return new WaitForSeconds(5.0f);

        // Load the scene after waiting
        SceneManager.LoadScene("GameScene");
    }
    #endregion

    #region Button Management
    public void OpenOptionsPanel()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        MenuUIScript.instance.optionsPanel.SetActive(true);
        MenuUIScript.instance.startGameButton.interactable = false;
        MenuUIScript.instance.openOptionsButton.interactable = false;
        MenuUIScript.instance.openHelpButton.interactable = false;
        MenuUIScript.instance.exitWindowWarningButton.interactable = false;
    }

    public void CloseOptionsPanel()
    {
        // Check if all keys are unique using a HashSet
        HashSet<KeyCode> keySet = new HashSet<KeyCode> { keyBoostPrimary, keyLeftPrimary, keyRightPrimary, keyPause };

        // Only proceed if all 4 keys are distinct
        if (keySet.Count != 4)
        {
            // Show duplicate key warning
            MenuUIScript.instance.duplicateKeysWarning.SetActive(true);
            return;
        }

        // Hide warning if it was shown
        MenuUIScript.instance.duplicateKeysWarning.SetActive(false);

        MenuUIScript.instance.deletedProgressText.SetActive(false);

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
        MenuUIScript.instance.optionsPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openHelpButton.interactable = true;
        MenuUIScript.instance.exitWindowWarningButton.interactable = true;
    }

    public void OpenHelpPanel()
    {
        // Update UI in Help panel
        MenuUIScript.instance.UpdateInputTutorialTexts();

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        MenuUIScript.instance.helpPanel.SetActive(true);
        MenuUIScript.instance.startGameButton.interactable = false;
        MenuUIScript.instance.openOptionsButton.interactable = false;
        MenuUIScript.instance.openHelpButton.interactable = false;
        MenuUIScript.instance.exitWindowWarningButton.interactable = false;
    }

    public void CloseHelpPanel()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
        MenuUIScript.instance.helpPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openHelpButton.interactable = true;
        MenuUIScript.instance.exitWindowWarningButton.interactable = true;
    }

    public void OpenWarningToMenu()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        MainGameUIScript.instance.warningMainMenuPanel.SetActive(true);
        MainGameUIScript.instance.pausePanel.SetActive(false);
    }

    public void OpenWarningRetry()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        MainGameUIScript.instance.warningRetryPanel.SetActive(true);
        MainGameUIScript.instance.pausePanel.SetActive(false);
    }

    public void GameOverScreen()
    {
        // Reset cursor to default appearance
        ManagerScript.instance.SetPixelCursor(ManagerScript.instance.basicCursor, 0f, 0f);

        // Disable pause functionality
        MainGameUIScript.instance.pauseButton.interactable = false;

        // Save score
        int finalScore = int.Parse(MainGameUIScript.instance.scoreText.text);
        PlayerPrefs.SetInt("Score", finalScore);
        MainGameUIScript.instance.finalScore.text = finalScore.ToString();

        // Update highscore if needed
        int storedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        int storedScore = PlayerPrefs.GetInt("Score", 0);
        if (storedScore > storedHighScore)
        {
            PlayerPrefs.SetInt("HighScore", storedScore);
            storedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        }
        MainGameUIScript.instance.highScore.text = storedHighScore.ToString();


        StartCoroutine(SpawnGameOverScreen());
    }

    public IEnumerator SpawnGameOverScreen()
    {
        // Wait time to show game over panel based on cause
        if (PlayerControllerScript.instance.falling == true)
        {
            yield return new WaitForSeconds(0.2f);
        }
        else if (PlayerControllerScript.instance.crashing == true)
        {
            yield return new WaitForSeconds(0.8f);
        }

        MainGameUIScript.instance.tutorials.SetActive(false);
        MainGameUIScript.instance.gameOverPanel.SetActive(true);

        // Display appropriate failure message
        if (PlayerControllerScript.instance.falling)
        {
            MainGameUIScript.instance.crashingText.SetActive(false);
            MainGameUIScript.instance.fallingText.SetActive(true);
        }
        else if (PlayerControllerScript.instance.crashing)
        {
            MainGameUIScript.instance.crashingText.SetActive(true);
            MainGameUIScript.instance.fallingText.SetActive(false);
        }
        else
        {
            MainGameUIScript.instance.crashingText.SetActive(false);
            MainGameUIScript.instance.fallingText.SetActive(false);
        }
    }

    public void OpenExitWinodwWarning()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        MenuUIScript.instance.exitWindowWarningPanel.SetActive(true);
        MenuUIScript.instance.startGameButton.interactable = false;
        MenuUIScript.instance.openOptionsButton.interactable = false;
        MenuUIScript.instance.openHelpButton.interactable = false;
        MenuUIScript.instance.exitWindowWarningButton.interactable = false;
    }

    public void CloseExitWindowWarning()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
        MenuUIScript.instance.exitWindowWarningPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openHelpButton.interactable = true;
        MenuUIScript.instance.exitWindowWarningButton.interactable = true;
    }

    public void ExitProgram()
    {
        // Close active window
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public void OpenDeleteProgressPanel()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        MenuUIScript.instance.deleteProgressPanel.SetActive(true);
        MenuUIScript.instance.startGameButton.interactable = false;
        MenuUIScript.instance.openOptionsButton.interactable = false;
        MenuUIScript.instance.openHelpButton.interactable = false;
        MenuUIScript.instance.exitWindowWarningButton.interactable = false;
    }

    public void CloseDeleteProgressPanel()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
        MenuUIScript.instance.deleteProgressPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openHelpButton.interactable = true;
        MenuUIScript.instance.exitWindowWarningButton.interactable = true;
    }

    public void OpenDeleteProgressConfirmPanel()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        MenuUIScript.instance.deleteProgressConfirmPanel.SetActive(true);
        MenuUIScript.instance.deleteProgressPanel.SetActive(false);
    }

    public void CloseDeleteProgressConfirmPanel()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.closeClickVolume);
        MenuUIScript.instance.deleteProgressConfirmPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openHelpButton.interactable = true;
        MenuUIScript.instance.exitWindowWarningButton.interactable = true;
    }

    public void DeleteProgress()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        MenuUIScript.instance.deletedProgressText.SetActive(false);
        MenuUIScript.instance.deleteProgressConfirmPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openHelpButton.interactable = true;
        MenuUIScript.instance.exitWindowWarningButton.interactable = true;

        // Wipe progression data and phase tutorial frags
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.DeleteKey("Score");

        int i = 0;
        while (true)
        {
            string key = "Phase" + i + "TutorialShown";
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                i++;
            }
            else
            {
                break;
            }
        }
        PlayerPrefs.Save();
    }
    #endregion

    #region Pause Management
    public void PauseGame()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        MainGameUIScript.instance.pauseButton.interactable = false;
        MainGameUIScript.instance.paused = true;
        MainGameUIScript.instance.tutorials.SetActive(false);
        MainGameUIScript.instance.pausePanel.SetActive(true);
        MainGameUIScript.instance.warningMainMenuPanel.SetActive(false);
        MainGameUIScript.instance.warningRetryPanel.SetActive(false);

        // Change to default cursor

        ManagerScript.instance.SetPixelCursor(ManagerScript.instance.basicCursor, 0f, 0f);
        // Save score
        int finalScore = int.Parse(MainGameUIScript.instance.scoreText.text);
        PlayerPrefs.SetInt("Score", finalScore);
    }

    public void ContinueGame()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
        MainGameUIScript.instance.tutorials.SetActive(true);
        MainGameUIScript.instance.pausePanel.SetActive(false);
        MainGameUIScript.instance.pauseButton.interactable = true;
        MainGameUIScript.instance.paused = false;

        // Change to game cursor
        ManagerScript.instance.SetPixelCursor(ManagerScript.instance.laserCursor, 0.5f, 0.5f);
    }
    #endregion

    #region Extra Click Sounds
    // Methods for when only a click sound is needed
    public void ClickSound()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
    }

    public void CloseClickSound()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
    }
    #endregion
}