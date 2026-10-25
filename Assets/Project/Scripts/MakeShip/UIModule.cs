using UnityEngine;
using UnityEngine.EventSystems;

public class UIModule : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerEnterHandler
{
    [SerializeField] string id;
    static Transform cancelZone; // 여기 위에서 손을 떼면 생성 취소

    BaseModule spawnedBaseModule;

    public void Initialize(string moduleId, Transform cancelZone)
    {
        id = moduleId;
        UIModule.cancelZone = cancelZone; // this.cancelZone -> 타입 이름으로 변경
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject go = ModuleMaker.CreateModule(id);
        if (go == null) return;

        if (go.TryGetComponent<CoreModule>(out _))
        {
            go.transform.SetParent(ShipContainer.Instance.transform);
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return;
        }

        if (!go.TryGetComponent<BaseModule>(out spawnedBaseModule))
        {
            Debug.LogError($"[UIModule] '{id}'는 BaseModule이 아니라 드래그 배치를 지원하지 않습니다.");
            Destroy(go);
            return;
        }

        go.transform.SetParent(ModulesContainer.Instance.transform);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0f;
        go.transform.position = worldPos;

        spawnedBaseModule.OnSelected();
    }

    public void OnDrag(PointerEventData eventData)
    {
        spawnedBaseModule?.OnDrag(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (spawnedBaseModule == null) return;

        if (IsOverCancelZone(eventData))
        {
            spawnedBaseModule.CancelSpawn();
        }
        else
        {
            spawnedBaseModule.OnDeselected();
        }

        spawnedBaseModule = null;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log(gameObject.name);
    }

    bool IsOverCancelZone(PointerEventData eventData)
    {
        var raycast = eventData.pointerCurrentRaycast;
        if (!raycast.isValid) return false;

        var hit = raycast.gameObject;
        return hit != null && cancelZone != null && hit.transform.IsChildOf(cancelZone);
    }

}