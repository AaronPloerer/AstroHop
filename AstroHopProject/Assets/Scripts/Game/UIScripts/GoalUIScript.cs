using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoalTextScript : MonoBehaviour
{
    #region Configuration
    [SerializeField] private GameObject goalTextPrefab;         // Prefab for goal text 
    [SerializeField] private GameObject goalLinePrefab;         // Prefab for goal line 
    [SerializeField] private float textSpawnSpacing;            // Vertical distance between goals
    [SerializeField] private float xPosition;                   // X-position for line goals
    [SerializeField] private float textOffsetX;                 // X-offset for line goals
    [SerializeField] private float textOffsetY;                 // Y-offset for line goals
    [SerializeField] private float minSpacefromHighscoreText;   // Min distance from highscore text 
    #endregion

    #region Runtime State
    private Transform cameraTransform;                                       // Reference to main camera transform component
    private float nextTextSpawnY;                                            // Y-position for upcoming text spawn
    private float highscorePosition;                                         // Stored highscore value
    private List<GameObject> spawnedTexts = new List<GameObject>();          // Active text goals
    private List<GameObject> spawnedLines = new List<GameObject>();          // Active line goals
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        InitializeReferences();
        CreateHighscoreGoal();
    }

    void Update()
    {
        HandleGoalSpawning();
        HandleGoalDespawning();
    }
    #endregion

    #region Initialization
    private void InitializeReferences()
    {
        cameraTransform = Camera.main.transform;     // Reference camera transforn
        nextTextSpawnY = textSpawnSpacing;           // Start spawning from first interval
    }

    private void CreateHighscoreGoal()
    {
        // Get highscore position through persistant storage
        float highscore = PlayerPrefs.GetInt("HighScore", 0);
        highscorePosition = highscore / MainGameUIScript.instance.positionToScore;
        
        if (highscorePosition > 0)
        {
            // Create highscore text object at correct place
            Vector3 textPosition = new Vector3(xPosition + textOffsetX, highscorePosition + textOffsetY, cameraTransform.position.z + 1f);
            GameObject goalText = Instantiate(goalTextPrefab, textPosition, Quaternion.identity, transform);

            // Configure highscore text
            TextMeshProUGUI tmpComponent = goalText.GetComponentInChildren<TextMeshProUGUI>();
            if (!tmpComponent) return;

            int localeID = PlayerPrefs.GetInt("Language", 0);
            tmpComponent.text = localeID switch
            {
                1 => "Rekord!",
                2 => "Record!",
                3 => "Meilleur\nScore !",
                _ => "High\nScore!"
            };

            tmpComponent.textWrappingMode = TextWrappingModes.NoWrap;
            tmpComponent.overflowMode = TextOverflowModes.Overflow;

            // Add text in list of active goal texts
            spawnedTexts.Add(goalText);

            // Create highscore line object at correct place
            Vector3 linePosition = new Vector3(xPosition, highscorePosition, cameraTransform.position.z + 1f);
            GameObject goallLine = Instantiate(goalLinePrefab, linePosition, Quaternion.identity, transform);

            // Add line in list of active goal lines
            spawnedLines.Add(goallLine);
        }
    }
    #endregion

    #region Goal Spawning
    private void HandleGoalSpawning()
    {
        // Spawn new markers when camera moves up enough
        if (cameraTransform.position.y + LevelGeneratorScript.instance.spawnBuffer > nextTextSpawnY)
        {
            SpawnGoal();
            nextTextSpawnY += textSpawnSpacing;    // Set up next goal position
        }
    }

    private void SpawnGoal()
    {
        if (!goalTextPrefab || !goalLinePrefab) return;

        // Prevent spawning too close to highscore text
        if (nextTextSpawnY < highscorePosition + minSpacefromHighscoreText && nextTextSpawnY > highscorePosition - minSpacefromHighscoreText) return;

        CreateGoalText();
        CreateGoalLine();
    }

    private void CreateGoalText()
    {
        // Spawn text marker at calculated world position
        Vector3 textPosition = new Vector3(xPosition + textOffsetX, nextTextSpawnY + textOffsetY,cameraTransform.position.z + 1f);
        GameObject goalText = Instantiate(goalTextPrefab, textPosition, Quaternion.identity, transform);

        ConfigureTextComponent(goalText);      // Set score text 
        spawnedTexts.Add(goalText);            // Add text in list of active goal texts
    }

    private void CreateGoalLine()
    {
        // Spawn line marker at calculated world position
        Vector3 linePosition = new Vector3(xPosition, nextTextSpawnY,cameraTransform.position.z + 1f);
        GameObject goalLine = Instantiate(goalLinePrefab, linePosition, Quaternion.identity, transform);

        spawnedLines.Add(goalLine);          // Add line in list of active goal lines
    }

    private void ConfigureTextComponent(GameObject goalText)
    {
        TextMeshProUGUI tmpComponent = goalText.GetComponentInChildren<TextMeshProUGUI>();
        if (!tmpComponent) return;

        tmpComponent.text = (nextTextSpawnY * MainGameUIScript.instance.positionToScore).ToString("0");   // Transform position to score and display as integer
        tmpComponent.textWrappingMode = TextWrappingModes.NoWrap;
        tmpComponent.overflowMode = TextOverflowModes.Overflow;
    }
    #endregion

    #region Goal Despawning
    private void HandleGoalDespawning()
    {
        // Check texts and lines for despawning
        CheckDespawn(spawnedTexts);
        CheckDespawn(spawnedLines);
    }

    private void CheckDespawn(List<GameObject> activeObjects)
    {
        // Calculate y-position below which markers should despawn
        float despawnThreshold = cameraTransform.position.y - LevelGeneratorScript.instance.despawnBuffer;

        // When under threshold: destroy object and remove from it's list
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeObjects[i];
            if (obj == null || obj.transform.position.y < despawnThreshold)
            {
                if (obj != null) Destroy(obj);
                activeObjects.RemoveAt(i);
            }
        }
    }
    #endregion
}