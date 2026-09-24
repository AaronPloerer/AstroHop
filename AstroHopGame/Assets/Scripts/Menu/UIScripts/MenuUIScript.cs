using System.Runtime.InteropServices;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Globalization;
using System.Collections.Generic;

[System.Serializable]
public class SkinButtonVisual
{
    public int skinIndex;
    public Button button;
    public TMP_Text lockedText;

    [Header("Sprites")]
    public Sprite notSelectedSprite;
    public Sprite selectedSprite;
    public Sprite notSelectedPressedSprite;
    public Sprite selectedPressedSprite;
    public Sprite lockedSprite;

    [HideInInspector] public Image buttonImage;
    [HideInInspector] public bool isUnlocked;
    [HideInInspector] public bool isPressed;
}

[System.Serializable]
public class ToggleButtonVisual
{
    public Button button;
    public Sprite notSelectedSprite;
    public Sprite selectedSprite;
    public Sprite notSelectedPressedSprite;
    public Sprite selectedPressedSprite;

    [HideInInspector] public bool isPressed;
}

public class MenuUIScript : MonoBehaviour
{
    #region Singleton
    public static MenuUIScript instance;

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
    [Header("Panels and Text")]
    public GameObject optionsPanel;
    public GameObject skinsPanel;
    public GameObject helpPanel;
    public GameObject exitWindowWarningPanel;
    public GameObject deleteProgressPanel;
    public GameObject deleteProgressConfirmPanel;
    public GameObject duplicateKeysWarning;
    public TMP_Text movementHelpText;
    public TMP_Text boostingHelpText;
    public TMP_Text pauseHelpText;
    public GameObject deletedProgressText;

    [Header("Buttons")]
    public Button openOptionsButton;
    public Button openSkinsButton;
    public Button openHelpButton;
    public Button startGameButton;
    public Button exitWindowWarningButton;

    [Header("Skin Buttons")]
    public SkinButtonVisual[] skinButtons = new SkinButtonVisual[6];

    [Header("Inverted Controls Buttons")]
    public ToggleButtonVisual invertedControlsOnButton;
    public ToggleButtonVisual invertedControlsOffButton;

    [Header("Inputs")]
    public TMP_Dropdown languageDropdown;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Image startingBoostToggleGraphic;
    public Image inGameTipsToggleGraphic;
    public Sprite toggleOnSprite;
    public Sprite toggleOffSprite;
    public TMP_InputField boostInputField;
    public TMP_InputField leftInputField;
    public TMP_InputField rightInputField;
    public TMP_InputField pauseInputField;
    public TMP_Text boostingInputFieldText;
    public TMP_Text leftInputFieldText;
    public TMP_Text rightInputFieldText;
    public TMP_Text pauseInputFieldText;
    #endregion

    #region Selected Input
    public TMP_InputField selectedInputField;
    private string selectedSavedKey;
    #endregion

    #region Initialization
    void Start()
    {
        // Set up cursor
        ManagerScript.instance.SetPixelCursor(ManagerScript.instance.basicCursor, 0f, 0f);

        // Set up selected language dropdown item
        InitializeLanguageDropdown();

        // Set up selected volume levels
        InitializeVolumeSliders();

        // Set up starting boost toggle 
        InitializeToggles();

        // Set up skin button visuals
        InitializeSkinButtonVisuals();

        // Set up controls position setup visuals
        InitializeInvertedControlsButtons();
    }
    #endregion

    #region Update
    void Update()
    {
        UpdateSkinSelectionState();
    }
    #endregion

    #region Volume Slider Setup
    private void InitializeVolumeSliders()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", AudioManagerScript.instance.defaultMusicVolume);
        sfxSlider.value = PlayerPrefs.GetFloat("SfxVolume", AudioManagerScript.instance.defaultSfxVolume);
    }
    #endregion

    #region Toggles Setup
    private void InitializeToggles()
    {
        bool startingBoostIsOn = PlayerPrefs.GetInt("StartingBoostEnabled", 1) == 1;

        if (startingBoostIsOn)
        {
            MenuUIScript.instance.startingBoostToggleGraphic.sprite = MenuUIScript.instance.toggleOnSprite;
            MenuUIScript.instance.startingBoostToggleGraphic.enabled = true;
        }
        else
        {
            MenuUIScript.instance.startingBoostToggleGraphic.sprite = MenuUIScript.instance.toggleOffSprite;
            MenuUIScript.instance.startingBoostToggleGraphic.enabled = true;
        }

        bool inGameTipsIsOn = PlayerPrefs.GetInt("InGameTipsEnabled", 1) == 1;

        if (inGameTipsIsOn)
        {
            MenuUIScript.instance.inGameTipsToggleGraphic.sprite = MenuUIScript.instance.toggleOnSprite;
            MenuUIScript.instance.inGameTipsToggleGraphic.enabled = true;
        }
        else
        {
            MenuUIScript.instance.inGameTipsToggleGraphic.sprite = MenuUIScript.instance.toggleOffSprite;
            MenuUIScript.instance.inGameTipsToggleGraphic.enabled = true;
        }
    }
    #endregion

    #region Language Localization
    private void InitializeLanguageDropdown()
    {
        // Initializes language dropdown with saved preference from permanent storage
        int index = PlayerPrefs.GetInt("Language", 0);
        languageDropdown.value = index;
    }
    #endregion

    #region Skin Selection Visuals
    // Runs once on menu load: figures out which skins are unlocked 
    private void InitializeSkinButtonVisuals()
    {
        foreach (var skinButton in skinButtons)
        {
            if (skinButton.button == null)
            {
                Debug.LogWarning("Skin button visual entry for skin " + skinButton.skinIndex + " has no Button assigned.");
                continue;
            }

            skinButton.buttonImage = skinButton.button.GetComponent<Image>();
            skinButton.isUnlocked = IsSkinUnlocked(skinButton.skinIndex);
            skinButton.isPressed = false;

            // The locked-state text starts disabled in the scene; only show it while this skin is locked
            if (skinButton.lockedText != null)
            {
                skinButton.lockedText.gameObject.SetActive(!skinButton.isUnlocked);
            }

            if (!skinButton.isUnlocked)
            {
                // Locked skins: show the locked sprite and disable interaction entirely
                skinButton.buttonImage.sprite = skinButton.lockedSprite;
                skinButton.button.interactable = false;
            }
            else
            {
                skinButton.button.interactable = true;
                SetupPressStateListeners(skinButton);
                UpdateSkinButtonVisual(skinButton);
            }
        }
    }

    // Adds PointerDown/PointerUp listeners so the button can swap to its pressed sprite.
    private void SetupPressStateListeners(SkinButtonVisual skinButton)
    {
        EventTrigger trigger = skinButton.button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = skinButton.button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDownEntry.callback.AddListener((_) =>
        {
            skinButton.isPressed = true;
            UpdateSkinButtonVisual(skinButton);
        });
        trigger.triggers.Add(pointerDownEntry);

        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUpEntry.callback.AddListener((_) =>
        {
            skinButton.isPressed = false;

            UpdateSkinButtonVisual(skinButton);
        });
        trigger.triggers.Add(pointerUpEntry);
    }

    // Picks the correct sprite for the button's current selected/pressed combination.
    private void UpdateSkinButtonVisual(SkinButtonVisual skinButton)
    {
        if (!skinButton.isUnlocked) return;

        int selectedSkin = PlayerPrefs.GetInt("Skin", 0);
        bool isSelected = skinButton.skinIndex == selectedSkin;

        if (isSelected)
        {
            skinButton.buttonImage.sprite = skinButton.isPressed ? skinButton.selectedPressedSprite : skinButton.selectedSprite;
        }
        else
        {
            skinButton.buttonImage.sprite = skinButton.isPressed ? skinButton.notSelectedPressedSprite : skinButton.notSelectedSprite;
        }
    }

    private bool IsSkinUnlocked(int skinIndex)
    {
        if (skinIndex <= 0) return true; 
        return PlayerPrefs.GetInt("SkinUnlocked" + skinIndex, 0) == 1;
    }

    // Runs every frame: reflects whichever skin is currently selected on the buttons,
    private void UpdateSkinSelectionState()
    {
        int selectedSkin = PlayerPrefs.GetInt("Skin", 0);

        if (!IsSkinUnlocked(selectedSkin))
        {
            PlayerPrefs.SetInt("Skin", 0);
            PlayerPrefs.Save();
        }

        foreach (var skinButton in skinButtons)
        {
            UpdateSkinButtonVisual(skinButton);
        }
    }
    #endregion

    #region Inverted Controls Buttons
    private void InitializeInvertedControlsButtons()
    {
        SetupTogglePressListeners(invertedControlsOnButton);
        SetupTogglePressListeners(invertedControlsOffButton);

        UpdateInvertedControlsButtonsVisual();
    }

    private void SetupTogglePressListeners(ToggleButtonVisual toggleButton)
    {
        EventTrigger trigger = toggleButton.button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = toggleButton.button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDownEntry.callback.AddListener((_) =>
        {
            toggleButton.isPressed = true;
            UpdateInvertedControlsButtonsVisual();
        });
        trigger.triggers.Add(pointerDownEntry);

        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUpEntry.callback.AddListener((_) =>
        {
            toggleButton.isPressed = false;
            UpdateInvertedControlsButtonsVisual();
        });
        trigger.triggers.Add(pointerUpEntry);
    }

    public void UpdateInvertedControlsButtonsVisual()
    {
        bool invertedOn = PlayerPrefs.GetInt("InvertedControls", 0) == 1;

        invertedControlsOnButton.button.GetComponent<Image>().sprite = invertedOn
            ? (invertedControlsOnButton.isPressed ? invertedControlsOnButton.selectedPressedSprite : invertedControlsOnButton.selectedSprite)
            : (invertedControlsOnButton.isPressed ? invertedControlsOnButton.notSelectedPressedSprite : invertedControlsOnButton.notSelectedSprite);

        invertedControlsOffButton.button.GetComponent<Image>().sprite = !invertedOn
            ? (invertedControlsOffButton.isPressed ? invertedControlsOffButton.selectedPressedSprite : invertedControlsOffButton.selectedSprite)
            : (invertedControlsOffButton.isPressed ? invertedControlsOffButton.notSelectedPressedSprite : invertedControlsOffButton.notSelectedSprite);
    }
    #endregion

    #region Adaptive Input Tutorial Help Panel 
    public void UpdateInputTutorialText()
    {
        // Select current language; 0 = English default
        int localeID = PlayerPrefs.GetInt("Language", 0);

        // Create tutorial texts based on choesen language and chosen controls
        if (movementHelpText != null)
        {
            movementHelpText.text = localeID switch
            {
                1 => $"Use your left/right movement keys (jetzt: {leftInputFieldText.text}/{rightInputFieldText.text} or Left Arrow/Right Arrow) to move toward your upcoming landing platforms.",
                2 => $"Usa i tasti di movimento sinistra/destra (ora: {leftInputFieldText.text}/{rightInputFieldText.text} o Frecce Sinistra/Destra) per posizionarti sulle piattaforme su cui vuoi atterrare.",
                3 => $"Utilisez les touches de déplacement gauche/droite (maintenant : {leftInputFieldText.text} / {rightInputFieldText.text} ou Flèche gauche/droite) pour vous positionner sous la plateforme sur laquelle vous souhaitez atterrir.",
                _ => $"Use your left/right movement keys (now: {leftInputFieldText.text} / {rightInputFieldText.text} or Left Arrow/Right Arrow) to move toward your upcoming landing platforms."
            };
        }

        if (boostingHelpText != null)
        {
            boostingHelpText.text = localeID switch
            {
                1 => $"Deine Rakete ermöglicht es dir, nach oben zu boosten. Wenn Treibstoff im Tank ist, halte die Boost-Taste gedrückt (jetzt: {boostingInputFieldText.text} oder Pfeiltaste nach oben), um zu fliegen. Springe zum Auftanken auf Plattformen mit Treibstofftanks. \r\nWährend eines Boosts wird dein Treibstoff kontinuierlich verbraucht, aber jede Boost-Phase verbraucht mindestens 10 % deines gesamten Tanks - sind weniger als 10 % übrig, wird der Rest komplett verbraucht. \r\nNutze den Boost, um verpasste Landungen zu korrigieren oder höhere Plattformen zu erreichen - aber tanke auf, um ihn erneut einsetzen zu können.\r\n",
                2 => $"Hai un razzo un boost verticale. Se il razzo contiene carburante, tieni premuto il tasto Boost (ora: {boostingInputFieldText.text} o Freccia Su) per volare verso l’alto. Per fare rifornimento, colpisci i serbatoi di carburante che compaiono su alcune piattaforme.\r\nDurante il boost, il carburante si consuma a un ritmo costante, ma ogni fase di boost consuma almeno il 10% del serbatoio totale. Se rimane meno del 10%, il resto viene consumato completamente.\r\nUsa il boost per correggere atterraggi mancati o raggiungere piattaforme più alte, ma ricorda: devi raccogliere carburante per riutilizzarlo.\r\n",
                3 => $"Vous disposez d’une fusée pour vous propulser verticalement. Si elle contient du carburant, maintenez la touche Boost enfoncée (maintenant : {boostingInputFieldText.text} ou Flèche haut) pour voler vers le haut. Pour refaire le plein, sautez dans les réservoirs de carburant qui apparaissent sur certaines plateformes.\r\nPendant le boost, votre carburant s’épuise à un rythme constant, mais chaque phase de boost consomme au moins 10 % de votre capacité totale ; si moins de 10 % reste, le reste est consommé intégralement.\r\nUtilisez la fusée pour corriger un atterrissage raté ou atteindre des plateformes plus élevées, mais souvenez-vous : vous devez récupérer du carburant pour pouvoir vous reboster à nouveau.\r\n",
                _ => $"You carry a rocket for vertical boosting. If it contains fuel, hold the Boost key (now: {boostingInputFieldText.text} or Up Arrow) to fly upward. To refuel, jump into fuel tanks that spawn on certain platforms. \r\nWhile boosting, your fuel drains at a constant rate, but each boost phase consumes at least 10% of your total tank - if less than 10% is left, the rest is consumed completely.\r\nUse rocket boosts to correct missed landings or reach higher ledges - but remember, you must collect fuel to use it again."
            };
        }

        if (pauseHelpText != null)
        {
            pauseHelpText.text = localeID switch
            {
                1 => $"Pausiere das Spiel jederzeit über das Pausensymbol oben links oder die Pausetaste (jetzt: {pauseInputFieldText.text}).",
                2 => $"Metti in pausa il gioco in qualsiasi momento cliccando l’icona di pausa in alto a sinistra o premendo il tasto Pausa (maintenant: {pauseInputFieldText.text}).",
                3 => $"Le jeu peut être mis en pause à tout moment en cliquant sur le bouton Pause en haut à gauche ou en appuyant sur la touche Pause (ora : {pauseInputFieldText.text}).",
                _ => $"The game can be paused at any time by clicking the pause button in the top-left corner or by pressing the Pause key (now: {pauseInputFieldText.text})."
            };
        }
    }
    #endregion
}