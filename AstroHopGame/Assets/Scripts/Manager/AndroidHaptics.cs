using UnityEngine;
using System.Collections;

public class AndroidHaptics : MonoBehaviour
{
    private static AndroidHaptics instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void TriggerHapticFeedback(float delay = 0f)
    {
        if (delay > 0f)
            instance.StartCoroutine(instance.DelayedHaptic(delay));
        else
            Haptic();
    }

    private IEnumerator DelayedHaptic(float delay)
    {
        yield return new WaitForSeconds(delay);
        Haptic();
    }

    private static void Haptic()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject window = currentActivity.Call<AndroidJavaObject>("getWindow"))
            using (AndroidJavaObject decorView = window.Call<AndroidJavaObject>("getDecorView"))
            using (AndroidJavaClass constants = new AndroidJavaClass("android.view.HapticFeedbackConstants"))
            {
                int flagIgnoreGlobal = constants.GetStatic<int>("FLAG_IGNORE_GLOBAL_SETTING");
                int virtualKey = constants.GetStatic<int>("VIRTUAL_KEY");
                decorView.Call<bool>("performHapticFeedback", virtualKey, flagIgnoreGlobal);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Haptics failed: " + e.Message);
        }
#endif
    }
}