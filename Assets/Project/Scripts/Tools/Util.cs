using UnityEngine;

public static class Util
{
    public static bool IsNull(this object obj)
    {
        if (obj == null)
            return true;

        if (obj is UnityEngine.Object unityObj)
            return unityObj == null;

        return false;
    }

    public static GameObject FindOrCache(string name)
    {
        GameObject target = GameObject.Find(name);
        if (target != null)
        {
            return target;
        }
        return new GameObject(name);
    }
}