using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LoadingUIScript : MonoBehaviour
{
    #region Configuration
    [SerializeField] private TMP_Text tipText;                               // Reference to the text component displaying tips
    [SerializeField] private List<string> englishTips = new List<string>();  // English loading screen tips
    [SerializeField] private List<string> germanTips = new List<string>();   // German loading screen tips
    [SerializeField] private List<string> italianTips = new List<string>();  // Italian loading screen tips
    [SerializeField] private List<string> frenchTips = new List<string>();   // French loading screen tips
    #endregion

    #region Tip Handling
    void Start()
    {
        // Set up cursor
        ManagerScript.instance.SetPixelCursor(ManagerScript.instance.basicCursor, 0f, 0f);

        // Get language from permanent storage (default = englsiH)
        int localeID = PlayerPrefs.GetInt("Language", 0);

        // Select random tip in seleceted language
        if (localeID == 0)
        {
            if (englishTips.Count > 0 && tipText != null)
            {
                int randomIndex = Random.Range(0, englishTips.Count);
                tipText.text = englishTips[randomIndex];
            }
        }
        else if (localeID == 1)
        {
            if (germanTips.Count > 0 && tipText != null)
            {
                int randomIndex = Random.Range(0, germanTips.Count);
                tipText.text = germanTips[randomIndex];
            }
        }
        else if (localeID == 2)
        {
            if (italianTips.Count > 0 && tipText != null)
            {
                int randomIndex = Random.Range(0, italianTips.Count);
                tipText.text = italianTips[randomIndex];
            }
        }
        else if (localeID == 3)
        {
            if (frenchTips.Count > 0 && tipText != null)
            {
                int randomIndex = Random.Range(0, frenchTips.Count);
                tipText.text = frenchTips[randomIndex];
            }
        }
    }
    #endregion
}
