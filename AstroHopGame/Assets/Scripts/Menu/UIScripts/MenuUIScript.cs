using System.Runtime.InteropServices;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Globalization;
using System.Collections.Generic;

public class MenuUIScript : MonoBehaviour
{
    #region Windows API
    // Gets the keyboard layout for a given thread
    [DllImport("user32.dll")]
    static extern IntPtr GetKeyboardLayout(uint idThread);

    // Gets the thread ID for a given window handle
    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

    // Gets the handle of the currently active window
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    #endregion

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
    public Button openHelpButton;
    public Button startGameButton;
    public Button exitWindowWarningButton;

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

        // Set up control input fields
        InitializeControlsInputField(boostInputField, "KeyBoostPrimary", KeyCode.W);
        InitializeControlsInputField(leftInputField, "KeyLeftPrimary", KeyCode.A);
        InitializeControlsInputField(rightInputField, "KeyRightPrimary", KeyCode.D);
        InitializeControlsInputField(pauseInputField, "KeyPause", KeyCode.Space);
    }
    #endregion

    #region Input Field Setup
    private void InitializeControlsInputField(TMP_InputField field, string savedKey, KeyCode defaultKey)
    {
        // Remove visual elements
        field.selectionColor = new Color(0f, 0f, 0f, 0f);  // Selection highlight
        field.caretColor = new Color(0, 0, 0, 0);          // Blinking cursor (transparent)
        field.caretWidth = 0;                              // Ensure no caret space

        // Initialize input field for custom controls
        RefreshKeyDisplay(field, savedKey, defaultKey);
        field.onSelect.AddListener(delegate { OnControlsInputFieldSelected(field, savedKey); });
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

    #region Key Rebinding Logic
    public void RefreshKeyDisplay(TMP_InputField field, string savedKey, KeyCode defaultKey)
    {
        // Update displayed key name in selected language
        KeyCode currentKey = (KeyCode)PlayerPrefs.GetInt(savedKey, (int)defaultKey);
        field.text = GetLocalizedKeyName(currentKey);
    }

    void OnControlsInputFieldSelected(TMP_InputField field, string savedKey)
    {
        selectedInputField = field;
        selectedSavedKey = savedKey;
    }

    private void OnGUI()
    {
        // Update Input when input field is selected and key is pressed
        if (selectedInputField != null)
        {
            Event e = Event.current;
            if (e.isKey && e.type == EventType.KeyDown)
            {
                // Ignore mouse buttons or reserved keys
                if ((e.keyCode is >= KeyCode.Mouse0 and <= KeyCode.Mouse6) ||
                e.keyCode is KeyCode.Escape or KeyCode.LeftArrow or KeyCode.RightArrow or KeyCode.UpArrow or KeyCode.DownArrow)
                    return;

                KeyCode key = e.keyCode;

                // Get localized key
                string keyName = GetLocalizedKeyName(key);

                // Key should be ignored if not in A-Z or explicitly named
                string defaultName = key.ToString().ToUpper();
                bool isExplicitlyNamed = keyName != defaultName;
                bool isAlphabetSingleChar = keyName.Length == 1 && char.IsLetter(keyName[0]);

                if (!isExplicitlyNamed && !isAlphabetSingleChar)
                {
                    return;
                }

                // Update UI and save key
                selectedInputField.text = keyName;
                PlayerPrefs.SetInt(selectedSavedKey, (int)key);
                UpdateKey(selectedSavedKey, key);
                PlayerPrefs.Save();

                // Reset selection
                AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
                selectedInputField.DeactivateInputField();
                EventSystem.current.SetSelectedGameObject(null);
                selectedInputField = null;
            }
        }
    }

    void UpdateKey(string savedKey, KeyCode key)
    {
        // Existing key update logic
        if (savedKey == "KeyBoostPrimary") ManagerScript.instance.keyBoostPrimary = key;
        else if (savedKey == "KeyLeftPrimary") ManagerScript.instance.keyLeftPrimary = key;
        else if (savedKey == "KeyRightPrimary") ManagerScript.instance.keyRightPrimary = key;
        else if (savedKey == "KeyPause") ManagerScript.instance.keyPause = key;
    }

    public string GetLocalizedKeyName(KeyCode key)
    {
        if (IsSpecialKey(key))
        {
            // Special key: determine name by language (key with same possition but different names)
            int gameLanguage = PlayerPrefs.GetInt("Language", 0);
            switch (gameLanguage)
            {
                case 1: // German
                    return GetGermanKeyName(key);
                case 2: // Italian
                    return GetItalianKeyName(key);
                case 3: // French
                    return GetFrenchKeyName(key);
                default: // English (0 or other)
                    return GetEnglishKeyName(key);
            }
        }
        else
        {
            // Other key: determine name by keyboard layout (key with same name but different position)
            switch (GetActiveKeyboardLayoutLanguage())
            {
                case "de": return GetGermanKeyName(key);
                case "fr": return GetFrenchKeyName(key);
                case "it": return GetItalianKeyName(key);
                default: return GetEnglishKeyName(key);
            }
        }
    }

    private bool IsSpecialKey(KeyCode key)
    {
        var specialKeys = new List<KeyCode>
    {
        KeyCode.Home,
        KeyCode.End,
        KeyCode.PageUp,
        KeyCode.PageDown,
        KeyCode.Delete,
        KeyCode.LeftShift,
        KeyCode.RightShift,
        KeyCode.CapsLock,
        KeyCode.Space,
        KeyCode.Return,
        KeyCode.KeypadEnter,
        KeyCode.Escape,
        KeyCode.Tab,
        KeyCode.Backspace,
        KeyCode.Numlock,
    };
        return specialKeys.Contains(key);
    }

    private static string GetActiveKeyboardLayoutLanguage()
    {
        // Get the handle of the window currently in the foreground
        IntPtr hwnd = GetForegroundWindow();
        uint threadId = GetWindowThreadProcessId(hwnd, IntPtr.Zero);

        // Get the thread ID associated with that foreground window
        IntPtr hkl = GetKeyboardLayout(threadId);

        // Get the input locale identifier (keyboard layout) for that thread
        int langId = hkl.ToInt32() & 0xFFFF;

        // Extract the language ID (lower 16 bits) from the input locale identifier
        CultureInfo ci = new CultureInfo(langId);
        return ci.TwoLetterISOLanguageName;

    }

    // Language-specific key name translations
    public string GetGermanKeyName(KeyCode key)
    {
        switch (key)
        {
            // Character Keys (QWERTZ)
            case KeyCode.BackQuote: return "Ö";
            case KeyCode.LeftBracket: return "ß";
            case KeyCode.RightBracket: return "´";
            case KeyCode.Semicolon: return "Ü";
            case KeyCode.Quote: return "Ä";
            case KeyCode.Minus: return "-";
            case KeyCode.Equals: return "+";
            case KeyCode.Comma: return ",";
            case KeyCode.Period: return ".";
            case KeyCode.Slash: return "#";

            // Numbers
            case KeyCode.Alpha1: return "1";
            case KeyCode.Alpha2: return "2";
            case KeyCode.Alpha3: return "3";
            case KeyCode.Alpha4: return "4";
            case KeyCode.Alpha5: return "5";
            case KeyCode.Alpha6: return "6";
            case KeyCode.Alpha7: return "7";
            case KeyCode.Alpha8: return "8";
            case KeyCode.Alpha9: return "9";
            case KeyCode.Alpha0: return "0";

            // Function Keys
            case KeyCode.F1: return "F1";
            case KeyCode.F2: return "F2";
            case KeyCode.F3: return "F3";
            case KeyCode.F4: return "F4";
            case KeyCode.F5: return "F5";
            case KeyCode.F6: return "F6";
            case KeyCode.F7: return "F7";
            case KeyCode.F8: return "F8";
            case KeyCode.F9: return "F9";
            case KeyCode.F10: return "F10";
            case KeyCode.F11: return "F11";
            case KeyCode.F12: return "F12";

            // Numpad
            case KeyCode.Insert: return "0 (NUM)";
            case KeyCode.Keypad1: return "1 (NUM)";
            case KeyCode.Keypad2: return "2 (NUM)";
            case KeyCode.Keypad3: return "3 (NUM)";
            case KeyCode.Keypad4: return "4 (NUM)";
            case KeyCode.Keypad5: return "5 (NUM)";
            case KeyCode.Keypad6: return "6 (NUM)";
            case KeyCode.Keypad7: return "7 (NUM)";
            case KeyCode.Keypad8: return "8 (NUM)";
            case KeyCode.Keypad9: return "9 (NUM)";
            case KeyCode.KeypadPeriod: return ". (NUM)";
            case KeyCode.KeypadMinus: return "- (NUM)";
            case KeyCode.KeypadPlus: return "+ (NUM)";
            case KeyCode.KeypadMultiply: return "* (NUM)";
            case KeyCode.KeypadDivide: return "/ (NUM)";
            case KeyCode.KeypadEquals: return "= (NUM)";
            case KeyCode.Numlock: return "NUM LOCK";

            // Spacial
            case KeyCode.Home: return "POS1";
            case KeyCode.End: return "ENDE";
            case KeyCode.PageUp: return "BILD RAUF";
            case KeyCode.PageDown: return "BILD RUNTER";
            case KeyCode.Delete: return "ENTF";
            case KeyCode.LeftShift: return "UMSCHALT LINKS";
            case KeyCode.RightShift: return "UMSCHALT RECHTS";
            case KeyCode.CapsLock: return "FESTSTELLTASTE";
            case KeyCode.Space: return "LEERTASTE";
            case KeyCode.Return: return "EINGABETASTE";
            case KeyCode.KeypadEnter: return "EINGABETASTE (NUM)";
            case KeyCode.Escape: return "ESC";
            case KeyCode.Tab: return "TAB";
            case KeyCode.Backspace: return "RÜCKTASTE";

            default: return key.ToString().ToUpper();
        }
    }

    public string GetItalianKeyName(KeyCode key)
    {
        switch (key)
        {
            // Character Keys (QWERTY)
            case KeyCode.BackQuote: return "ò";
            case KeyCode.LeftBracket: return "'";
            case KeyCode.RightBracket: return "ì";
            case KeyCode.Semicolon: return "è";  
            case KeyCode.Quote: return "à";
            case KeyCode.Minus: return "-";
            case KeyCode.Equals: return "+";
            case KeyCode.Comma: return ",";
            case KeyCode.Period: return ".";
            case KeyCode.Slash: return "ù";

            // Numbers
            case KeyCode.Alpha1: return "1";
            case KeyCode.Alpha2: return "2";
            case KeyCode.Alpha3: return "3";
            case KeyCode.Alpha4: return "4";
            case KeyCode.Alpha5: return "5";
            case KeyCode.Alpha6: return "6";
            case KeyCode.Alpha7: return "7";
            case KeyCode.Alpha8: return "8";
            case KeyCode.Alpha9: return "9";
            case KeyCode.Alpha0: return "0";

            // Function Keys
            case KeyCode.F1: return "F1";
            case KeyCode.F2: return "F2";
            case KeyCode.F3: return "F3";
            case KeyCode.F4: return "F4";
            case KeyCode.F5: return "F5";
            case KeyCode.F6: return "F6";
            case KeyCode.F7: return "F7";
            case KeyCode.F8: return "F8";
            case KeyCode.F9: return "F9";
            case KeyCode.F10: return "F10";
            case KeyCode.F11: return "F11";
            case KeyCode.F12: return "F12";

            // Numpad
            case KeyCode.Insert: return "0 (NUM)";
            case KeyCode.Keypad1: return "1 (NUM)";
            case KeyCode.Keypad2: return "2 (NUM)";
            case KeyCode.Keypad3: return "3 (NUM)";
            case KeyCode.Keypad4: return "4 (NUM)";
            case KeyCode.Keypad5: return "5 (NUM)";
            case KeyCode.Keypad6: return "6 (NUM)";
            case KeyCode.Keypad7: return "7 (NUM)";
            case KeyCode.Keypad8: return "8 (NUM)";
            case KeyCode.Keypad9: return "9 (NUM)";
            case KeyCode.KeypadPeriod: return ". (NUM)";
            case KeyCode.KeypadMinus: return "- (NUM)";
            case KeyCode.KeypadPlus: return "+ (NUM)";
            case KeyCode.KeypadMultiply: return "* (NUM)";
            case KeyCode.KeypadDivide: return "/ (NUM)";
            case KeyCode.KeypadEquals: return "= (NUM)";
            case KeyCode.Numlock: return "NUM LOCK";

            // Special
            case KeyCode.Home: return "HOME";
            case KeyCode.End: return "FINE";
            case KeyCode.PageUp: return "PAG SU";
            case KeyCode.PageDown: return "PAG GIÙ";
            case KeyCode.Delete: return "CANC";
            case KeyCode.LeftShift: return "MAIUSC SINISTRA"; 
            case KeyCode.RightShift: return "MAIUSC DESTRA"; 
            case KeyCode.CapsLock: return "BLOC MAIUSC";
            case KeyCode.Space: return "BARRA SPAZIATRICE";
            case KeyCode.Return: return "INVIO";
            case KeyCode.KeypadEnter: return "INVIO (NUM)";
            case KeyCode.Escape: return "ESC";
            case KeyCode.Tab: return "TAB";
            case KeyCode.Backspace: return "CANCELLARE";

            default: return key.ToString().ToUpper();
        }
    }

    public string GetFrenchKeyName(KeyCode key)
    {
        switch (key)
        {
            // Character Keys (AZERTY)
            case KeyCode.BackQuote: return "M";
            case KeyCode.LeftBracket: return "'";
            case KeyCode.RightBracket: return "=";
            case KeyCode.Semicolon: return "^";
            case KeyCode.Quote: return "ù";
            case KeyCode.Minus: return "!";
            case KeyCode.Equals: return "$";
            case KeyCode.Comma: return ";";
            case KeyCode.Period: return ":";
            case KeyCode.Slash: return "*";

            // Numbers
            case KeyCode.Alpha1: return "&";
            case KeyCode.Alpha2: return "é";
            case KeyCode.Alpha3: return "\"";
            case KeyCode.Alpha4: return "'";
            case KeyCode.Alpha5: return "(";
            case KeyCode.Alpha6: return "-";
            case KeyCode.Alpha7: return "è";
            case KeyCode.Alpha8: return "_";
            case KeyCode.Alpha9: return "ç";
            case KeyCode.Alpha0: return "à";

            // Function Keys
            case KeyCode.F1: return "F1";
            case KeyCode.F2: return "F2";
            case KeyCode.F3: return "F3";
            case KeyCode.F4: return "F4";
            case KeyCode.F5: return "F5";
            case KeyCode.F6: return "F6";
            case KeyCode.F7: return "F7";
            case KeyCode.F8: return "F8";
            case KeyCode.F9: return "F9";
            case KeyCode.F10: return "F10";
            case KeyCode.F11: return "F11";
            case KeyCode.F12: return "F12";

            // Numpad
            case KeyCode.Insert: return "0 (NUM)";
            case KeyCode.Keypad1: return "1 (NUM)";
            case KeyCode.Keypad2: return "2 (NUM)";
            case KeyCode.Keypad3: return "3 (NUM)";
            case KeyCode.Keypad4: return "4 (NUM)";
            case KeyCode.Keypad5: return "5 (NUM)";
            case KeyCode.Keypad6: return "6 (NUM)";
            case KeyCode.Keypad7: return "7 (NUM)";
            case KeyCode.Keypad8: return "8 (NUM)";
            case KeyCode.Keypad9: return "9 (NUM)";
            case KeyCode.KeypadPeriod: return ". (NUM)";
            case KeyCode.KeypadMinus: return "- (NUM)";
            case KeyCode.KeypadPlus: return "+ (NUM)";
            case KeyCode.KeypadMultiply: return "* (NUM)";
            case KeyCode.KeypadDivide: return "/ (NUM)";
            case KeyCode.KeypadEquals: return "= (NUM)";
            case KeyCode.Numlock: return "NUM LOCK";


            // Special
            case KeyCode.Home: return "ACCUEIL";
            case KeyCode.End: return "FIN";
            case KeyCode.PageUp: return "PAGE PRÉCÉDENTE";
            case KeyCode.PageDown: return "PAGE SUIVANTE";
            case KeyCode.Delete: return "SUPPR";
            case KeyCode.LeftShift: return "MAJ GAUCHE";
            case KeyCode.RightShift: return "MAJ DROITE";
            case KeyCode.CapsLock: return "VERR MAJ";
            case KeyCode.Space: return "ESPACE";
            case KeyCode.Return: return "ENTRÉE";
            case KeyCode.KeypadEnter: return "ENTRÉE (NUM)";
            case KeyCode.Escape: return "ÉCHAP";
            case KeyCode.Tab: return "TAB";
            case KeyCode.Backspace: return "RETOUR";

            default: return key.ToString().ToUpper();
        }
    }

    public string GetEnglishKeyName(KeyCode key)
    {
        switch (key)
        {
            // Character Keys (QWERTY)
            case KeyCode.BackQuote: return ";";
            case KeyCode.LeftBracket: return "-";
            case KeyCode.RightBracket: return "=";
            case KeyCode.Semicolon: return "[";
            case KeyCode.Quote: return "'";
            case KeyCode.Minus: return "/";
            case KeyCode.Equals: return "]";
            case KeyCode.Comma: return ",";
            case KeyCode.Period: return ".";
            case KeyCode.Slash: return "\\";

            // Numbers
            case KeyCode.Alpha1: return "1";
            case KeyCode.Alpha2: return "2";
            case KeyCode.Alpha3: return "3";
            case KeyCode.Alpha4: return "4";
            case KeyCode.Alpha5: return "5";
            case KeyCode.Alpha6: return "6";
            case KeyCode.Alpha7: return "7";
            case KeyCode.Alpha8: return "8";
            case KeyCode.Alpha9: return "9";
            case KeyCode.Alpha0: return "0";

            // Function Keys
            case KeyCode.F1: return "F1";
            case KeyCode.F2: return "F2";
            case KeyCode.F3: return "F3";
            case KeyCode.F4: return "F4";
            case KeyCode.F5: return "F5";
            case KeyCode.F6: return "F6";
            case KeyCode.F7: return "F7";
            case KeyCode.F8: return "F8";
            case KeyCode.F9: return "F9";
            case KeyCode.F10: return "F10";
            case KeyCode.F11: return "F11";
            case KeyCode.F12: return "F12";

            // Numpad
            case KeyCode.Insert: return "0 (NUM)";
            case KeyCode.Keypad1: return "1 (NUM)";
            case KeyCode.Keypad2: return "2 (NUM)";
            case KeyCode.Keypad3: return "3 (NUM)";
            case KeyCode.Keypad4: return "4 (NUM)";
            case KeyCode.Keypad5: return "5 (NUM)";
            case KeyCode.Keypad6: return "6 (NUM)";
            case KeyCode.Keypad7: return "7 (NUM)";
            case KeyCode.Keypad8: return "8 (NUM)";
            case KeyCode.Keypad9: return "9 (NUM)";
            case KeyCode.KeypadPeriod: return ", (NUM)";
            case KeyCode.KeypadMinus: return "- (NUM)";
            case KeyCode.KeypadPlus: return "+ (NUM)";
            case KeyCode.KeypadMultiply: return "* (NUM)";
            case KeyCode.KeypadDivide: return "/ (NUM)";
            case KeyCode.KeypadEquals: return "= (NUM)";
            case KeyCode.Numlock: return "NUM LOCK";

            // Special
            case KeyCode.Home: return "HOME";
            case KeyCode.End: return "END";
            case KeyCode.PageUp: return "PAGE UP";
            case KeyCode.PageDown: return "PAGE DOWN";
            case KeyCode.Delete: return "DELETE";
            case KeyCode.LeftShift: return "LEFT SHIFT";
            case KeyCode.RightShift: return "RIGHT SHIFT";
            case KeyCode.CapsLock: return "CAPS LOCK";
            case KeyCode.Space: return "SPACEBAR";
            case KeyCode.Return: return "ENTER";
            case KeyCode.KeypadEnter: return "ENTER (NUM)";
            case KeyCode.Escape: return "ESC";
            case KeyCode.Tab: return "TAB";
            case KeyCode.Backspace: return "BACKSPACE";

            // Default fallthrough
            default: return key.ToString().ToUpper();
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

    #region Adaptive Input Tutorial Help Panel 
    public void UpdateInputTutorialTexts()
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
                1 => $"Deine Rakete ermöglicht es dir, nach oben zu boosten. Wenn Treibstoff im Tank ist, halte die Boost-Taste gedrückt (jetzt: {boostingInputFieldText.text} oder Pfeiltaste nach oben), um zu fliegen. Springe zum Auftanken auf Plattformen mit Treibstofftanks. \r\nWährend eines Boosts wird dein Treibstoff kontinuierlich verbraucht, aber jede Boost-Phase verbraucht mindestens 10 % deines gesamten Tanks - sind weniger als 10 % übrig, wird der Rest komplett verbraucht. \r\nNutze den Boost, um verpasste Landungen zu korrigieren oder höhere Plattformen zu erreichen - aber tanke auf, um ihn erneut einsetzen zu können.\r\n",
                2 => $"Hai un razzo un boost verticale. Se il razzo contiene carburante, tieni premuto il tasto Boost (ora: {boostingInputFieldText.text} o Freccia Su) per volare verso l’alto. Per fare rifornimento, colpisci i serbatoi di carburante che compaiono su alcune piattaforme.\r\nDurante il boost, il carburante si consuma a un ritmo costante, ma ogni fase di boost consuma almeno il 10% del serbatoio totale. Se rimane meno del 10%, il resto viene consumato completamente.\r\nUsa il boost per correggere atterraggi mancati o raggiungere piattaforme più alte, ma ricorda: devi raccogliere carburante per riutilizzarlo.\r\n",
                3 => $"Vous disposez d’une fusée pour vous propulser verticalement. Si elle contient du carburant, maintenez la touche Boost enfoncée (maintenant : {boostingInputFieldText.text} ou Flèche haut) pour voler vers le haut. Pour refaire le plein, sautez dans les réservoirs de carburant qui apparaissent sur certaines plateformes.\r\nPendant le boost, votre carburant s’épuise à un rythme constant, mais chaque phase de boost consomme au moins 10 % de votre capacité totale ; si moins de 10 % reste, le reste est consommé intégralement.\r\nUtilisez la fusée pour corriger un atterrissage raté ou atteindre des plateformes plus élevées, mais souvenez-vous : vous devez récupérer du carburant pour pouvoir vous reboster à nouveau.\r\n",
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