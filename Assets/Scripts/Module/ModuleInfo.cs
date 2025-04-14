using UnityEngine;

public class ModuleInfo : MonoBehaviour
{
    protected Connector connectedTo;
    protected Connector sender;
    [SerializeField]protected Connector[] receivers;
    protected Rigidbody2D rigid;
    public bool IsAttachedToCore {  get { return connectedTo != null; } }
    /// <summary>
    /// Is attached to other core?
    /// </summary>
    protected bool isControllable = true;

    protected virtual void Awake()
    {
        Connector[] allConnectors = GetComponentsInChildren<Connector>();
        sender = System.Array.Find(allConnectors, c => c.type == ConnectorType.Sender);
        receivers = System.Array.FindAll(allConnectors, c => c.type == ConnectorType.Receiver);
        rigid = GetComponent<Rigidbody2D>();
        enabled = false;
    }
}
