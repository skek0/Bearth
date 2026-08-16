using UnityEngine;
using UnityEngine.EventSystems;

public class UIModule : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] string id;
    [SerializeField] Transform cancelZone; // 여기 위에서 손을 떼면 생성 취소

    BaseModule spawnedModule;

    public void Initialize(string moduleId, Transform cancelZone)
    {
        id = moduleId;
        this.cancelZone = cancelZone;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject go = ModuleMaker.CreateModule(id);
        if (go == null) return;

        if (!go.TryGetComponent<BaseModule>(out spawnedModule))
        {
            Debug.LogError($"[UIModule] '{id}'는 BaseModule이 아니라 드래그 배치를 지원하지 않습니다.");
            Destroy(go);
            return;
        }

        go.transform.SetParent(ModulesContainer.Instance.transform);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0f;
        go.transform.position = worldPos;

        spawnedModule.OnSelected();
    }

    public void OnDrag(PointerEventData eventData)
    {
        spawnedModule?.OnDrag(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (spawnedModule == null) return;

        if (IsOverCancelZone(eventData))
        {
            spawnedModule.CancelSpawn();
        }
        else
        {
            spawnedModule.OnDeselected();
        }

        spawnedModule = null;
    }

    bool IsOverCancelZone(PointerEventData eventData)
    {
        var raycast = eventData.pointerCurrentRaycast;
        if (!raycast.isValid) return false;

        var hit = raycast.gameObject;
        return hit != null && cancelZone != null && hit.transform.IsChildOf(cancelZone);
    }
}