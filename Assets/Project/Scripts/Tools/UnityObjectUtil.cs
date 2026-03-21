public static class UnityObjectUtil
{
    public static bool IsNull(this object obj)
    {
        if (obj == null)
            return true;

        if (obj is UnityEngine.Object unityObj)
            return unityObj == null;

        return false;
    }
}