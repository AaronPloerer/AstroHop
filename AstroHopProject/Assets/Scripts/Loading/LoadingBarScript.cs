using UnityEngine;

public class LoadingBarScript : MonoBehaviour
{
    #region Configuration
    [SerializeField] private GameObject flame;        // Animated point of loading bar fill
    [SerializeField] private GameObject fire;         // Expanding loading bar fill
    [SerializeField] private float loadingTime;       // Total duration of loading process
    [SerializeField] private float endingPositionX;   // Flame x-position at end of loading
    #endregion

    #region Private Variables
    private Vector3 finalFireScale;          // Target width of loading bar fill
    private float elapsedTime;               // Time accumulated since loading started
    private Vector3 startingFlamePosition;   // Initial position of the flame object
    #endregion

    #region Initialization
    private void Start()
    {
        // Remamber final loading bar fill size and set it to 0
        finalFireScale = fire.transform.localScale;
        fire.transform.localScale = new Vector3(0, finalFireScale.y, finalFireScale.z);

        // Remember starting position of the flame
        startingFlamePosition = flame.transform.position;

        // Reset loading timer
        elapsedTime = 0f;
    }
    #endregion

    #region Progress Handling
    void Update()
    {
        if (elapsedTime < loadingTime)
        {
            // Accumulate time and calculate progress percentage
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / loadingTime;

            // Move flame horizontally based on progress
            Vector3 newFlamePosition = flame.transform.position;
            newFlamePosition.x = Mathf.Lerp(startingFlamePosition.x, endingPositionX, progress);
            flame.transform.position = newFlamePosition;

            // Expand fire effect based on progress
            float newXScale = Mathf.Lerp(0, finalFireScale.x, progress);
            fire.transform.localScale = new Vector3(newXScale, finalFireScale.y, finalFireScale.z);
        }
        else if (elapsedTime >= loadingTime)
        {
            // Loading complete - transition to game scene
            ManagerScript.instance.LoadGameScene();
        }
    }
    #endregion
}