using UnityEngine;

public sealed class ConnectorPort : MonoBehaviour
{
    [SerializeField] private string portId;
    [SerializeField] private bool isCoreWeaponPort;
    public string PortId => portId;
    public bool IsCoreWeaponPort => isCoreWeaponPort;

    private void Start()
    {
        if(!transform.parent.TryGetComponent(out CoreModule _)) isCoreWeaponPort = false;
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(portId))
            portId = gameObject.name;
    }
#endif
}