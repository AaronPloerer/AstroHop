using UnityEngine;
using System.Threading;

public class SingleInstanceEnforcer : MonoBehaviour
{
    private static Mutex mutex = null;
    private const string mutexName = "AstroHopMutexName15";
    private static bool ownsMutex = false;

    void Awake()
    {
        mutex = new Mutex(true, mutexName, out bool isNewMutexCreated);
        ownsMutex = isNewMutexCreated;

        if (!ownsMutex)
        {
            Application.Quit();
        }
    }

    void OnDestroy()
    {
        if (mutex != null && ownsMutex)
        {
            mutex.ReleaseMutex();
            mutex.Dispose();
            mutex = null;
            ownsMutex = false;
        }
    }

    void OnApplicationQuit()
    {
        if (mutex != null && ownsMutex)
        {
            mutex.ReleaseMutex();
            mutex.Dispose();
            mutex = null;
            ownsMutex = false;
        }
    }
}
