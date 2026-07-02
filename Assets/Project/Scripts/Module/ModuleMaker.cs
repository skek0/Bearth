using UnityEngine;

public class ModuleMaker
{
    public static GameObject CreateModule(string moduleID)
    {
        BaseStat baseStat = ModuleSpecDB.BaseStats[moduleID];

        // 프리팹으로 모양 정의
        GameObject prefab = Resources.Load<GameObject>(baseStat.PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"모듈 프리팹을 찾을 수 없습니다: {baseStat.PrefabPath}");
            return null;
        }

        // Core/Base 컴포넌트 추가
        prefab = Object.Instantiate(prefab);
        switch(baseStat.ModuleType)
        {
            case "CoreModule":
                prefab.AddComponent<CoreModule>();
                break;
            case "BaseModule":
                prefab.AddComponent<BaseModule>();
                break;
            default:
                Debug.LogError($"알 수 없는 모듈 타입입니다: {baseStat.ModuleType}");
                Object.Destroy(prefab);
                return null;
        }

        // 능력 컴포넌트 추가
        if(baseStat.Components != null)
        {
            foreach (var item in baseStat.Components.Split(','))
            {
                string componentName = item.Trim();
                if (string.IsNullOrEmpty(componentName))
                    continue;
                System.Type componentType = System.Type.GetType(componentName);
                if (componentType == null)
                {
                    Debug.LogError($"컴포넌트 타입을 찾을 수 없습니다: {componentName}");
                    continue;
                }
                prefab.AddComponent(componentType);
            }
        }

        // 스프라이트 설정
        var spriteTransform = prefab.transform.Find("Skin");
        if (spriteTransform == null || !spriteTransform.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            Debug.LogError($"스프라이트 렌더러를 찾을 수 없습니다: {baseStat.ModuleID}");
            Object.Destroy(prefab);
            return null;
        }
        spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/" + baseStat.ModuleID);

        // 머티리얼 설정
        Material glowMaterial = Resources.Load<Material>("Materials/ModuleGlow");
        if (glowMaterial != null)
        {
            spriteRenderer.material = Object.Instantiate(glowMaterial);
        }
        else
        {
            Debug.LogError("ModuleGlow 머티리얼을 찾을 수 없습니다.");
        }
        // 모듈 기본스탯 적용
        Module module = prefab.GetComponent<Module>();
        module.ApplyBaseStat(baseStat);

        return prefab;
    }
}
