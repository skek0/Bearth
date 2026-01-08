using UnityEngine;
public abstract class SceneSingleton<T> : Singleton<T>
    where T : MonoBehaviour
{
}

public abstract class GlobalSingleton<T> : Singleton<T>
    where T : MonoBehaviour
{
    protected sealed override bool IsPersistent => true;
}


public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual bool IsPersistent => false;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;

        if (IsPersistent)
            DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
