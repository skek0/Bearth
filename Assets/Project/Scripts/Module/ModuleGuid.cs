using UnityEngine;

public sealed class ModuleGuid : MonoBehaviour
{
    private string guid;
    public string Guid => guid;

    public void SetGuid(string v) => guid = v;

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(guid))
            guid = System.Guid.NewGuid().ToString("N");
    }
}
