using UnityEngine;
using UnityEngine.UI;

public class MakeShip : SceneSingleton<MakeShip>
{
    [SerializeField] private GameObject shipListViewport; 
    [SerializeField] private Transform cancelZone;

    protected override void Awake()
    {
        base.Awake();
    }

    protected void Start()
    {
        foreach (var modulestat in ModuleSpecDB.BaseStats)
        {
            GameObject uiObj = new(modulestat.Key);

            UIModule uimodule = uiObj.AddComponent<UIModule>();
            uimodule.Initialize(modulestat.Key, cancelZone);

            Image image = uiObj.AddComponent<Image>();
            image.sprite = Resources.Load<Sprite>("Sprites/" + modulestat.Value.ModuleID);

            uiObj.transform.SetParent(shipListViewport.transform, false);
        }
    }

}
