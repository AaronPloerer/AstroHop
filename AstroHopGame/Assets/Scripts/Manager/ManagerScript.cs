using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
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
        InitializeLanguage();
    }
    #endregion

    #region Localization
    private bool localizationActive = false;     // Flag to prevent overlapping locale changes

    void InitializeLanguage()
    {
        int localeID;

        if (PlayerPrefs.HasKey("Language"))
        {
            // User has already picked (or had auto-detected) a language before
            localeID = PlayerPrefs.GetInt("Language", 0);
        }
        else
        {
            // First ever launch: try to match system language, otherwise default to English
            localeID = GetLocaleIDFromSystemLanguage();
            PlayerPrefs.SetInt("Language", localeID);
            PlayerPrefs.Save();
        }

        StartCoroutine(SetLocale(localeID));
    }

    private int GetLocaleIDFromSystemLanguage()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.German:
                return 1;
            case SystemLanguage.Italian:
                return 2;
            case SystemLanguage.French:
                return 3;
            default:
                return 0; // English fallback for everything else
        }
    }

    public void LanguageDropdown()
    {
        // Load language from UI language dropdown
        int selectedLocale = MenuUIScript.instance.languageDropdown.value;

        if (localizationActive == true)  return;     // Prevent multiple simultaneous locale changes

        // Add click sound when selecting language
        if (MenuUIScript.instance.optionsPanel.activeSelf)
        {
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        }

        StartCoroutine(SetLocale(selectedLocale));
    }

    private IEnumerator SetLocale(int localeID)
    {
        localizationActive = true;                                                                                      // Set lock flag
        yield return LocalizationSettings.InitializationOperation;                                                      // Wait until the localization system is ready 
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];                  // Change lactive language
        PlayerPrefs.SetInt("Language", localeID);                                                                       // Save selected language in permanenet storage
        localizationActive = false;                                                                                     // Release lock flag
    }
    #endregion

    #region Revive Settings
    [Header("Revive Settings")]
    [SerializeField] private GameObject playerPrefab;          // Drag your new Player Prefab here
    [SerializeField] private float respawnBelowCameraOffset;   // How far below the camera's current Y to respawn (positive number)
    [SerializeField] private float reviveBoostDuration;        // How long the revive boost lasts, in seconds
    private bool reviveUsedThisSession = false;
    #endregion

    #region Cursor Management
    [Header("Cursor Configuration")]
    [SerializeField] private float cursorScale;      // Multiplier for cursor texture scaling
    public Sprite basicCursor;                       // Default cursor appearence
    public Sprite laserCursor;                       // Cursor appearence when playing of game view area
    public Sprite laserCursorActive;                 // Game cursor when laser indicator is on (clicked to shoot)
    private Sprite currentCursorSprite;
    public void SetPixelCursor(Sprite cursorSprite, float hotspotRight, float hotspotDown)
    {
        if (cursorSprite == null) return;

        if (cursorSprite == currentCursorSprite) return; // nothing changed, skip

        currentCursorSprite = cursorSprite;

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
        float selectedVolume = MenuUIScript.instance.sfxSlider.value;
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
        StartCoroutine(LoadSceneAfterSound("LoadingScene"));
    }

    public void LoadGameScene()
    {
        // Directly load game scene
        SceneManager.LoadScene("GameScene");
    }

    public void LoadGameSceneOnClick()
    {
        // Start coroutine to handle sound and scene change
        StartCoroutine(LoadSceneAfterSound("GameScene"));
    }

    public void LoadMenuScene()
    {
        // Directly load game scene
        SceneManager.LoadScene("MenuScene");
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

        // Wait for the duration of the click sound
        yield return new WaitForSeconds(AudioManagerScript.instance.click.length);

        // Load the scene after waiting
        SceneManager.LoadScene(sceneName);
    }
    #endregion

    #region Pause Management
    [SerializeField] private float pauseToggleCooldown; // unscaled seconds
    private float lastPauseToggleTime = -999f;

    public void TogglePauseGame()
    {
        // Prevent double-toggle from spam/double taps
        if (Time.unscaledTime - lastPauseToggleTime < pauseToggleCooldown) return;
        lastPauseToggleTime = Time.unscaledTime;

        if (MainGameUIScript.instance.paused)
            ContinueGame();
        else
            PauseGame();
    }
    #endregion

    #region Button Management
    [Header("Help/Exit Button Cooldowns")]
    [SerializeField] private float helpButtonCooldown;
    [SerializeField] private float exitWarningButtonCooldown;
    private float lastHelpButtonClickTime = -999f;
    private float lastExitWarningButtonClickTime = -999f;

    public void OpenOptionsPanel()
    {
        MenuUIScript.instance.optionsPanel.SetActive(true);
        MenuUIScript.instance.startGameButton.interactable = false;
        MenuUIScript.instance.openOptionsButton.interactable = false;
        MenuUIScript.instance.openSkinsButton.interactable = false;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
    }

    public void CloseOptionsPanel()
    {
        // Force close dropdown first and clear its focus
        MenuUIScript.instance.languageDropdown.Hide();
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

        MenuUIScript.instance.deletedProgressText.SetActive(false);

        MenuUIScript.instance.optionsPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openSkinsButton.interactable = true;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
    }

    public void OpenSkinsPanel()
    {
        // Update UI in Help panel
        MenuUIScript.instance.UpdateInputTutorialText();

        MenuUIScript.instance.skinsPanel.SetActive(true);
        MenuUIScript.instance.startGameButton.interactable = false;
        MenuUIScript.instance.openOptionsButton.interactable = false;
        MenuUIScript.instance.openSkinsButton.interactable = false;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
    }

    public void CloseSkinsPanel()
    {
        MenuUIScript.instance.skinsPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openSkinsButton.interactable = true;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
    }

    public void OpenHelpPanel() 
    {
        // Cooldown to prevent double-click spam
        if (Time.unscaledTime - lastHelpButtonClickTime < helpButtonCooldown) return;
        lastHelpButtonClickTime = Time.unscaledTime;

        // Toggle: if it's already open, close it instead of reopening
        if (MenuUIScript.instance.helpPanel.activeSelf)
        {
            CloseHelpPanel();
            return;
        }

        // Update UI in Help panel
        MenuUIScript.instance.UpdateInputTutorialText();

        MenuUIScript.instance.helpPanel.SetActive(true);
        MenuUIScript.instance.exitWindowWarningPanel.SetActive(false);
        MenuUIScript.instance.skinsPanel.SetActive(false);
        MenuUIScript.instance.optionsPanel.SetActive(false);
        MenuUIScript.instance.deleteProgressPanel.SetActive(false);
        MenuUIScript.instance.deleteProgressConfirmPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = false;
        MenuUIScript.instance.openOptionsButton.interactable = false;
        MenuUIScript.instance.openSkinsButton.interactable = false;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
    }

    public void CloseHelpPanel()
    {
        MenuUIScript.instance.helpPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openSkinsButton.interactable = true;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
    }

    public void OpenWarningToMenu()
    {
        MainGameUIScript.instance.warningMainMenuPanel.SetActive(true);
        MainGameUIScript.instance.pausePanel.SetActive(false);

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
    }

    public void OpenWarningRetry()
    {
        MainGameUIScript.instance.warningRetryPanel.SetActive(true);
        MainGameUIScript.instance.pausePanel.SetActive(false);

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
    }

    public void GameOverScreen()
    {
        // Reset cursor to default appearance
        ManagerScript.instance.SetPixelCursor(ManagerScript.instance.basicCursor, 0f, 0f);

        // Disable pause functionality
        MainGameUIScript.instance.pauseButton.interactable = false;

        // Reset and disable joystick
        var joystick = MainGameUIScript.instance.aimConroller.GetComponent<AimJoystick>();
        joystick.ForceReset();         // Snap knob to center and hide preview 
        joystick.enabled = false;      // Then disable

        // Force-release boost
        MainGameUIScript.instance.boostButton.GetComponent<BoostButton>().ForceRelease();

        // Save score
        int finalScore = int.Parse(MainGameUIScript.instance.scoreText.text);
        PlayerPrefs.SetInt("Score", finalScore);
        MainGameUIScript.instance.finalScore.text = finalScore.ToString();

        // Check skin unlocks
        MainGameUIScript.instance.CheckSkinUnlocks(finalScore);

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

        // Only show the revive button if a rewarded ad is actually ready AND the player hasn't already revived this session
        bool adReady = !reviveUsedThisSession
            && AdManagerScript.instance != null
            && AdManagerScript.instance.IsAdActuallyReady();

        MainGameUIScript.instance.reviveButton.gameObject.SetActive(adReady);

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

    public void WatchAdToRevive()
    {
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);

        AdManagerScript.instance.ShowRewardedAd(() =>
        {
            reviveUsedThisSession = true;             // Lock out further revives this session
            StartCoroutine(RevivePlayerRoutine());
        });
    }

    private IEnumerator RevivePlayerRoutine()
    {
        yield return new WaitForSecondsRealtime(0.3f);

        // Restore UI/controls to their pre-game-over state
        MainGameUIScript.instance.gameOverPanel.SetActive(false);
        MainGameUIScript.instance.tutorials.SetActive(true);
        MainGameUIScript.instance.pauseButton.interactable = true;

        var joystick = MainGameUIScript.instance.aimConroller.GetComponent<AimJoystick>();
        joystick.enabled = true;

        ManagerScript.instance.SetPixelCursor(ManagerScript.instance.laserCursor, 0.5f, 0.5f);

        // Respawn position: centered horizontally, a fixed distance below the camera's current position
        Vector3 respawnPosition = new Vector3(
            0f,
            CameraScript.instance.transform.position.y - respawnBelowCameraOffset,
            0f
        );

        GameObject newPlayer = Instantiate(playerPrefab, respawnPosition, Quaternion.identity);

        yield return null;

        CameraScript.instance.SetTarget(newPlayer.transform);
        PlayerControllerScript.instance.ActivateReviveBoost(reviveBoostDuration);
    }

    public void OpenExitWinodwWarning()
    {
        // Cooldown to prevent double-click spam
        if (Time.unscaledTime - lastExitWarningButtonClickTime < exitWarningButtonCooldown) return;
        lastExitWarningButtonClickTime = Time.unscaledTime;

        // Toggle: if it's already open, close it instead of reopening
        if (MenuUIScript.instance.exitWindowWarningPanel.activeSelf)
        {
            CloseExitWindowWarning();
            return;
        }

        MenuUIScript.instance.exitWindowWarningPanel.SetActive(true);
        MenuUIScript.instance.helpPanel.SetActive(false);
        MenuUIScript.instance.skinsPanel.SetActive(false);
        MenuUIScript.instance.optionsPanel.SetActive(false);
        MenuUIScript.instance.deleteProgressPanel.SetActive(false);
        MenuUIScript.instance.deleteProgressConfirmPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = false;
        MenuUIScript.instance.openOptionsButton.interactable = false;
        MenuUIScript.instance.openSkinsButton.interactable = false;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
    }

    public void CloseExitWindowWarning()
    {
        MenuUIScript.instance.exitWindowWarningPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openSkinsButton.interactable = true;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
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
        MenuUIScript.instance.deleteProgressPanel.SetActive(true);
        MenuUIScript.instance.startGameButton.interactable = false;
        MenuUIScript.instance.openOptionsButton.interactable = false;
        MenuUIScript.instance.openSkinsButton.interactable = false;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
    }

    public void CloseDeleteProgressPanel()
    {
        MenuUIScript.instance.deleteProgressPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openSkinsButton.interactable = true;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
    }

    public void OpenDeleteProgressConfirmPanel()
    {
        MenuUIScript.instance.deleteProgressConfirmPanel.SetActive(true);
        MenuUIScript.instance.deleteProgressPanel.SetActive(false);

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
    }

    public void CloseDeleteProgressConfirmPanel()
    {
        MenuUIScript.instance.deleteProgressConfirmPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openSkinsButton.interactable = true;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.closeClickVolume);
    }

    public void DeleteProgress()
    {
        MenuUIScript.instance.deletedProgressText.SetActive(true);
        MenuUIScript.instance.deleteProgressConfirmPanel.SetActive(false);
        MenuUIScript.instance.startGameButton.interactable = true;
        MenuUIScript.instance.openOptionsButton.interactable = true;
        MenuUIScript.instance.openSkinsButton.interactable = true;
        MenuUIScript.instance.openHelpButton.interactable = true;
        MenuUIScript.instance.exitWindowWarningButton.interactable = true;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);

        // Wipe progression data and reshow tutorials
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

        // Wipe unlocked skins
        foreach (var skinButton in MenuUIScript.instance.skinButtons)
        {
            int skinIndex = skinButton.skinIndex;

            if (PlayerPrefs.GetInt("SkinUnlocked" + skinIndex, 0) == 1)
            {
                PlayerPrefs.DeleteKey("SkinUnlocked" + skinIndex);
            }
        }

        // Reset selected skin to defaul
        PlayerPrefs.SetInt("Skin", 0);

        PlayerPrefs.Save();
    }

    public void SetInvertedControlsOn()
    {
        if (PlayerPrefs.GetInt("InvertedControls", 0) == 1) return; // already on, no-op

        PlayerPrefs.SetInt("InvertedControls", 1);
        PlayerPrefs.Save();

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        MenuUIScript.instance.UpdateInvertedControlsButtonsVisual();
    }

    public void SetInvertedControlsOff()
    {
        if (PlayerPrefs.GetInt("InvertedControls", 0) == 0) return; // already off, no-op

        PlayerPrefs.SetInt("InvertedControls", 0);
        PlayerPrefs.Save();

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
        MenuUIScript.instance.UpdateInvertedControlsButtonsVisual();
    }

    public void SelectSkin(int skinIndex)
    {
        // Ignore the click if this skin hasn't been unlocked yet
        if (!IsSkinUnlocked(skinIndex)) return;

        PlayerPrefs.SetInt("Skin", skinIndex);
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        PlayerPrefs.Save();
    }

    private bool IsSkinUnlocked(int skinIndex)
    {
        if (skinIndex <= 0) return true;

        return PlayerPrefs.GetInt("SkinUnlocked" + skinIndex, 0) == 1;
    }
    #endregion

    #region Pause Management
    public void PauseGame()
    {
        MainGameUIScript.instance.paused = true;
        MainGameUIScript.instance.tutorials.SetActive(false);
        MainGameUIScript.instance.pausePanel.SetActive(true);
        MainGameUIScript.instance.warningMainMenuPanel.SetActive(false);
        MainGameUIScript.instance.warningRetryPanel.SetActive(false);

        // Reset and disable joystick
        var joystick = MainGameUIScript.instance.aimConroller.GetComponent<AimJoystick>();
        joystick.ForceReset();         // Snap knob to center and hide preview 
        joystick.enabled = false;      // Then disable

        // Force-release boost
        MainGameUIScript.instance.boostButton.GetComponent<BoostButton>().ForceRelease();

        // Change to default cursor

        ManagerScript.instance.SetPixelCursor(ManagerScript.instance.basicCursor, 0f, 0f);
        // Save score
        int finalScore = int.Parse(MainGameUIScript.instance.scoreText.text);
        PlayerPrefs.SetInt("Score", finalScore);

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
    }

    public void PauseGameOnExit()
    {
        MainGameUIScript.instance.paused = true;
        MainGameUIScript.instance.tutorials.SetActive(false);
        MainGameUIScript.instance.pausePanel.SetActive(true);
        MainGameUIScript.instance.warningMainMenuPanel.SetActive(false);
        MainGameUIScript.instance.warningRetryPanel.SetActive(false);

        // Reset and disable joystick
        var joystick = MainGameUIScript.instance.aimConroller.GetComponent<AimJoystick>();
        joystick.ForceReset();         // Snap knob to center and hide preview 
        joystick.enabled = false;      // Then disable

        // Force-release boost
        MainGameUIScript.instance.boostButton.GetComponent<BoostButton>().ForceRelease();

        // Change to default cursor

        ManagerScript.instance.SetPixelCursor(ManagerScript.instance.basicCursor, 0f, 0f);
        // Save score
        int finalScore = int.Parse(MainGameUIScript.instance.scoreText.text);
        PlayerPrefs.SetInt("Score", finalScore);
    }

    public void ContinueGame()
    {
        MainGameUIScript.instance.tutorials.SetActive(true);
        MainGameUIScript.instance.pausePanel.SetActive(false);
        MainGameUIScript.instance.warningMainMenuPanel.SetActive(false);
        MainGameUIScript.instance.warningRetryPanel.SetActive(false);
        MainGameUIScript.instance.paused = false;

        // Enable aim functionality
        MainGameUIScript.instance.aimConroller.GetComponent<AimJoystick>().enabled = true;

        // Change to game cursor
        ManagerScript.instance.SetPixelCursor(ManagerScript.instance.laserCursor, 0.5f, 0.5f);

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
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