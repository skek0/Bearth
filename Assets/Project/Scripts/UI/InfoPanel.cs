using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private Image moduleImage;
    [SerializeField] private TextMeshProUGUI moduleName;
    [SerializeField] private TextMeshProUGUI moduleHp;
    [SerializeField] private TextMeshProUGUI moduleSpecialStat;

    private void Start()
    {
        PointerInputHandler.Instance.HoverTargetChanged += UpdatePanel;
    }

    private void UpdatePanel(GameObject panellableObject)
    {
        if(panellableObject.TryGetComponent<IModuleInfoSource>(out var moduleInfo))
        {

            moduleName.text = moduleInfo.DisplayName;
            moduleHp.text = $"{moduleInfo.CurrentHp} / {moduleInfo.MaxHp}";

            if (moduleInfo.TryGetSpecialStat(out string specialStat))
            {
                moduleSpecialStat.text = specialStat;
            }
            else moduleSpecialStat.text = "";
        }
        else
        {
            ClearPanel();
        }
    }

    void ClearPanel()
    {
        moduleImage.sprite = null;
        moduleName.text = "";
        moduleHp.text = "";
        moduleSpecialStat.text = "";
    }


}