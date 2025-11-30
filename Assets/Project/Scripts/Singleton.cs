using UnityEngine;

public class S_Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    protected virtual void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

public class G_Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("destroyed");
            Destroy(gameObject);
        }
    }
    protected virtual void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}