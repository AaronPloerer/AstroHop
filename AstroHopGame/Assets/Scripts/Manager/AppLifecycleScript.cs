using UnityEngine;

public class AppLifecycleScript : MonoBehaviour
{
    #region Exit App Management
    // Unity calls this automatically on Android when the app is sent to background
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            AutoPauseIfInGame();
        }
    }

    private void AutoPauseIfInGame()
    {
        // Already paused (e.g. player paused manually before backgrounding) - nothing to do
        if (MainGameUIScript.instance.paused) return;

        // Don't reopen the pause panel over a game-over / not-yet-alive state
        if (PlayerControllerScript.instance == null || !PlayerControllerScript.instance.isAlive) return;

        // Reuses existing pause logic: joystick reset, boost release,
        // cursor reset, score save, pause panel activation, etc.
        ManagerScript.instance.PauseGameOnExit();
    }
    #endregion

    #region Screen Sleep Management
    // Keeps the screen forced on while gameplay is actively running 
    private void Update()
    {
        UpdateScreenSleepTimeout();
    }

    private void UpdateScreenSleepTimeout()
    {
        bool gameplayActive = IsGameplayActive();

        Screen.sleepTimeout = gameplayActive ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
    }

    private bool IsGameplayActive()
    {
        if (MainGameUIScript.instance == null) return false;              // Not in the Game scene
        if (MainGameUIScript.instance.paused) return false;               // Paused
        if (PlayerControllerScript.instance == null) return false;
        if (!PlayerControllerScript.instance.isAlive) return false;       // Dead / game-over

        return true;
    }
    #endregion
}