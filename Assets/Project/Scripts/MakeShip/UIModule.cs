using UnityEngine;
using UnityEngine.EventSystems;

public class UIModule : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]string id;
    public void Initialize(string moduleId)
    {
        id = moduleId;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        MakeShip.Instance.OnUIModuleClick(id);
    }
}
