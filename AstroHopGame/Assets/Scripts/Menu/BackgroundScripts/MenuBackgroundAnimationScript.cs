using UnityEngine;

public class MenuBackgroundAnimationScript : MonoBehaviour
{
    #region Variables

    [Header("Spawn Settings")]
    [SerializeField] private float ufoSpawnTimer;     // Time in seconds between UFO spawns
    [SerializeField] private GameObject ufoSprite;    // UFO prefab reference

    [Header("Spawn Area References")]
    // Left screen edge spawn area boundaries
    [SerializeField] private GameObject leftMinY;
    [SerializeField] private GameObject leftMaxY;

    // Right screen edge spawn area boundaries
    [SerializeField] private GameObject rightMinY;
    [SerializeField] private GameObject rightMaxY;

    private float time; // Timer tracking time since last spawn

    #endregion

    #region Unity Methods

    // Spawn first UFO and initialize timer
    void Start()
    {
        SpawnUfo();
        time = 0;
    }

    // Update timer and spawn UFOs at every interval
    void Update()
    {
        time += Time.deltaTime;
        if (time >= ufoSpawnTimer)
        {
            SpawnUfo();
            time = 0;
        }
    }

    #endregion

    #region UFO Spawn Management
    private void SpawnUfo()
    {
        // Randomly choose spawn and target sides
        bool spawnOnLeft = Random.value < 0.5f;
        Vector3 startPos, endPos;

        if (spawnOnLeft)
        {
            // Get random y-position on left side for spawn
            float startY = Random.Range(leftMinY.transform.position.y, leftMaxY.transform.position.y);
            startPos = new Vector3(leftMinY.transform.position.x, startY, 0f);

            // Get random y-positionon right side for target
            float endY = Random.Range(rightMinY.transform.position.y, rightMaxY.transform.position.y);
            endPos = new Vector3(rightMinY.transform.position.x, endY, 0f);
        }
        else
        {
            // Get random y-position on right side for spawn
            float startY = Random.Range(rightMinY.transform.position.y, rightMaxY.transform.position.y);
            startPos = new Vector3(rightMinY.transform.position.x, startY, 0f);

            // Get random y-positionon left side for target
            float endY = Random.Range(leftMinY.transform.position.y, leftMaxY.transform.position.y);
            endPos = new Vector3(leftMinY.transform.position.x, endY, 0f);
        }

        // Instantiate UFO and set its movement towards target
        GameObject spawnedUfo = Instantiate(ufoSprite, startPos, Quaternion.identity);
        MenuUfoScript ufoScript = spawnedUfo.GetComponent<MenuUfoScript>();
        if (ufoScript != null)
        {
            ufoScript.SetTarget(endPos);
        }
    }
    #endregion
}