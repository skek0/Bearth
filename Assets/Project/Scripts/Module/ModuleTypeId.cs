using UnityEngine;

[ExecuteAlways]
public sealed class ModuleTypeId : MonoBehaviour
{
    [SerializeField] private string typeId;
    public string TypeId => typeId;

    private void Awake() => SyncFromName();

#if UNITY_EDITOR
    private void OnValidate() => SyncFromName();
#endif

    private void SyncFromName()
    {
        // prefab/instance 이름에서 "(Clone)" 제거
        string n = gameObject.name;
        int idx = n.IndexOf("(Clone)");
        if (idx >= 0) n = n.Substring(0, idx);
        typeId = n.Trim();
    }
}
