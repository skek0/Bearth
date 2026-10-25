using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnterNamePopup : Popup
{
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;

    public event Action<string> Confirmed;

    protected override void OnEnable()
    {
        base.OnEnable();
        confirmButton.onClick.AddListener(DoAction);
        cancelButton.onClick.AddListener(Cancel);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        confirmButton.onClick.RemoveListener(DoAction);
        cancelButton.onClick.RemoveListener(Cancel);
    }

    void DoAction() 
    {
        Confirmed?.Invoke(nameInputField.text);
        Cancel();
    }
}
