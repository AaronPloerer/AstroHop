using UnityEngine;
using System.Threading;

public class SingleInstanceEnforcer : MonoBehaviour
{
    #region Mutex Properties
    private static Mutex mutex = null;
    private const string mutexName = "AstroHopMutexName15"; // Unique identifier
    #endregion

    #region Initialization
    void Awake()
    {
        // Attempt to create/claim the mutex
        mutex = new Mutex(true, mutexName, out bool isNewMutexCreated);

        // If the mutex already exists
        if (!isNewMutexCreated)
        {
            Application.Quit(); // Close immediately
        }
    }
    #endregion

    #region Cleanup
    void OnApplicationQuit()
    {
        // Clean up the mutex
        if (mutex != null)
        {
            mutex.ReleaseMutex();
            mutex.Dispose();
        }
    }
    #endregion
}