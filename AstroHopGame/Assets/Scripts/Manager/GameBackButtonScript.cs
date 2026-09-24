using UnityEngine;
using UnityEngine.InputSystem;

// Attach this to any persistent GameObject in the GameScene (e.g. the same object as MainGameUIScript).
// Handles the Android hardware back button / swipe, which the new Input System reports
// through Keyboard.current.escapeKey.
public class GameBackButtonScript : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleBackButton();
        }
    }

    private void HandleBackButton()
    {
        var ui = MainGameUIScript.instance;
        if (ui == null) return;

        if (ui.gameOverPanel.activeSelf)
        {
            // Game over: back button goes straight to the main menu.
            ManagerScript.instance.LoadMenuSceneOnClick();
        }
        else if (ui.warningRetryPanel.activeSelf || ui.warningMainMenuPanel.activeSelf)
        {
            ReturnToPausePanel();
        }
        else if (ui.paused)
        {
            // paused is only true here because pausePanel is the panel currently showing
            // (the warning-panel and game-over cases above are already handled).
            ManagerScript.instance.ContinueGame();
        }
        else
        {
            // Nothing open, player is playing: back button pauses.
            ManagerScript.instance.PauseGame();
        }
    }

    // Warning Retry / Warning Main Menu were opened on top of the Pause Panel
    // (both OpenWarningToMenu and OpenWarningRetry set pausePanel inactive when opening),
    // so closing them just reveals Pause again.
    private void ReturnToPausePanel()
    {
        var ui = MainGameUIScript.instance;
        ui.warningRetryPanel.SetActive(false);
        ui.warningMainMenuPanel.SetActive(false);
        ui.pausePanel.SetActive(true);

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
    }
}