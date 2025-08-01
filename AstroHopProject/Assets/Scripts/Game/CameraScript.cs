using UnityEngine;

public class CameraScript : MonoBehaviour
{
    #region Singleton
    public static CameraScript instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    #endregion

    #region Camera Settings
    [SerializeField] private Transform target;     // The object the camera should follow
    [SerializeField] private float yOffset;        // Vertical distance between target and camera
    #endregion

    #region Camera Movement
    void LateUpdate()
    {
        // Only follow the player upwards, never downwards
        if (target != null && target.position.y + yOffset > transform.position.y)
        {
            Vector3 newPos = new Vector3(transform.position.x, target.position.y + yOffset, transform.position.z);
            transform.position = newPos;
        }
    }
    #endregion
}
