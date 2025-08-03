using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class Connection : MonoBehaviour
{
    List<GameObject> nearConnectors = new();
    public Transform ClosestConnector { get; private set; }

    void Update()
    {
        Transform closest = null;
        float minSqrDistance = float.MaxValue;
        Vector3 myPos = transform.position;

        foreach (var connector in nearConnectors)
        {
            Vector3 targetPos = connector.transform.position;
            float sqrDist = (targetPos - myPos).sqrMagnitude;

            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                closest = connector.transform;
            }
        }
        ClosestConnector = closest;
        if (closest != null)
        {
            // 가장 가까운 대상에 대해 처리 (이펙트 호출 등)
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject collidedObject = collision.gameObject;
        if(collidedObject.layer == LayerMask.NameToLayer("Connector") && IsConnectableConnector(collidedObject))
        {
            if (!nearConnectors.Contains(collidedObject))
                nearConnectors.Add(collidedObject);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject exitingObject = collision.gameObject;
        if (exitingObject.layer == LayerMask.NameToLayer("Connector"))
        {
            if (nearConnectors.Contains(exitingObject))
                nearConnectors.Remove(exitingObject);
        }
    }

    bool IsConnectableConnector(GameObject connector)
    {
        connector.transform.parent.TryGetComponent(out Module module);
        if(module != null)
        {
            return module.Connectable;
        }
        return false;
    }
}
