using UnityEngine;

public static class ReviewPromptStats
{
    // Tuning values
    public const int MinRuns = 10;
    public const int MinSessions = 2;
    public const int MinScore = 1500;

    const string SessionsKey = "ReviewSessions";
    const string RunsKey = "ReviewRuns";
    const string DoneKey = "ReviewPromptDone";
    const string LastScoreKey = "ReviewLastRunScore";

    static bool sessionCounted;   // resets on every cold start of the app

    // Call once at app start. The flag makes sure scene changes don't count again.
    public static void CountSessionOnce()
    {
        if (sessionCounted) return;
        sessionCounted = true;

        PlayerPrefs.SetInt(SessionsKey, PlayerPrefs.GetInt(SessionsKey, 0) + 1);
        PlayerPrefs.Save();
    }

    // Call once when a run starts
    public static void CountRun()
    {
        PlayerPrefs.SetInt(RunsKey, PlayerPrefs.GetInt(RunsKey, 0) + 1);
        PlayerPrefs.Save();
    }

    // Read at the start of a run
    public static int GetLastRunScore()
    {
        return PlayerPrefs.GetInt(LastScoreKey, 0);
    }

    // Call at game over, after ShouldPrompt
    public static void SaveRunScore(int score)
    {
        PlayerPrefs.SetInt(LastScoreKey, score);
        PlayerPrefs.Save();
    }

    public static bool ShouldPrompt(int finalScore, int previousRunScore)
    {
        if (PlayerPrefs.GetInt(DoneKey, 0) == 1) return false;

        return PlayerPrefs.GetInt(RunsKey, 0) >= MinRuns
            && PlayerPrefs.GetInt(SessionsKey, 0) >= MinSessions
            && finalScore >= MinScore
            && finalScore > previousRunScore;
    }

    public static void MarkPrompted()
    {
        PlayerPrefs.SetInt(DoneKey, 1);
        PlayerPrefs.Save();
    }
}