using UnityEngine;
using UnityEngine.InputSystem;

// Attach this to any persistent GameObject in the MenuScene (e.g. the same object as MenuUIScript).
// Handles the Android hardware back button / swipe, which the new Input System reports
// through Keyboard.current.escapeKey.
public class MenuBackButtonScript : MonoBehaviour
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
        var menu = MenuUIScript.instance;
        if (menu == null) return;

        // Check most-nested panels first so only one transition happens per press.
        if (menu.deleteProgressConfirmPanel.activeSelf)
        {
            CloseDeleteConfirmToOptions();
        }
        else if (menu.deleteProgressPanel.activeSelf)
        {
            CloseDeleteProgressToOptions();
        }
        else if (menu.optionsPanel.activeSelf)
        {
            ManagerScript.instance.CloseOptionsPanel();
        }
        else if (menu.skinsPanel.activeSelf)
        {
            ManagerScript.instance.CloseSkinsPanel();
        }
        else if (menu.helpPanel.activeSelf)
        {
            ManagerScript.instance.CloseHelpPanel();
        }
        else if (menu.exitWindowWarningPanel.activeSelf)
        {
            ManagerScript.instance.CloseExitWindowWarning();
        }
        else
        {
            // Nothing open: back button opens the exit confirmation.
            ManagerScript.instance.OpenExitWinodwWarning();
        }
    }

    // Delete Progress Confirm Panel was opened on top of Options Panel (Delete Progress Panel
    // is already inactive by the time Confirm is showing), so closing it just reveals Options again.
    private void CloseDeleteConfirmToOptions()
    {
        var menu = MenuUIScript.instance;
        menu.deleteProgressConfirmPanel.SetActive(false);
        // optionsPanel stays active underneath; the other menu buttons stay disabled
        // because options is still open, matching CloseOptionsPanel's own state.

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
    }

    // Delete Progress Panel was opened on top of Options Panel, so closing it reveals Options again.
    private void CloseDeleteProgressToOptions()
    {
        var menu = MenuUIScript.instance;
        menu.deleteProgressPanel.SetActive(false);

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
    }
}