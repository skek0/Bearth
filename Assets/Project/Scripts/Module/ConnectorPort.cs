using UnityEngine;

public sealed class ConnectorPort : MonoBehaviour
{
    [SerializeField] private string portId;
    public string PortId => portId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(portId))
            portId = gameObject.name;
    }
#endif
}