using UnityEngine;
using UnityEngine.UIElements;

public class MenuUfoScript : MonoBehaviour
{
    #region Configuration
    [SerializeField] private float speed;                 // Movement speed
    [SerializeField] private float destroyThreshold;      // Threshold for automatic destruction when near target
    private Vector3 targetPosition;
    #endregion

    #region Initialization
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;         // Set target
    }
    #endregion

    #region Movement and Despawn Management
    void Update()
    {
        // Move towards target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Destroy object when next to target
        if (Vector3.Distance(transform.position, targetPosition) < destroyThreshold)
        {
            Destroy(gameObject);
        }
    }
    #endregion
}
