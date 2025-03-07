using UnityEngine;

public enum ConnectorType
{
    Sender,
    Receiver
}
public class Connector : MonoBehaviour
{
    public Transform module;
    public ConnectorType type;
    public bool isOccupied = false;

    private void Awake()
    {
        module = transform.parent;
    }
    private void Start()
    {
        if (type == ConnectorType.Sender) transform.up = (transform.position - module.position).normalized;
        else { transform.up = (module.position - transform.position).normalized; }
    }
    public bool IsAttachable(Transform otherModule)
    {
        if ( module != otherModule &&
            (module.TryGetComponent<CoreModule>(out _) || module.GetComponent<ModuleInfo>().IsAttachedToCore)&&
            !module.IsChildOf(otherModule) &&
            type == ConnectorType.Receiver &&
            !isOccupied
         )
        {
            return true;
        }
        return false;
    }
}
