using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField] protected UIManager.PopupType popupType;
    [SerializeField] PopupBackdrop backdrop;

    protected virtual void OnEnable()
    {
        backdrop.Clicked += Cancel;
    }

    protected virtual void OnDisable()
    {
        backdrop.Clicked -= Cancel;
    }

    protected virtual void Cancel() { UIManager.Instance.HidePopup(popupType); }
}
