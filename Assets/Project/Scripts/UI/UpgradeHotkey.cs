using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 업그레이드 창 열기/닫기 단축키. 씬의 아무 오브젝트(예: UIManager)에 붙여 사용.
/// scrap 획득 방식이 정해지기 전까지 테스트용 scrap 지급 키도 제공한다.
/// </summary>
public class UpgradeHotkey : MonoBehaviour
{
    [SerializeField] InputAction toggleAction = new("ToggleUpgrade", InputActionType.Button, "<Keyboard>/u");

    [Header("테스트용 (에디터/개발 빌드에서만 동작)")]
    [SerializeField] InputAction debugAddScrapAction = new("DebugAddScrap", InputActionType.Button, "<Keyboard>/f9");
    [SerializeField] int debugScrapAmount = 100;

    private void OnEnable()
    {
        toggleAction.performed += OnToggle;
        toggleAction.Enable();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        debugAddScrapAction.performed += OnDebugAddScrap;
        debugAddScrapAction.Enable();
#endif
    }

    private void OnDisable()
    {
        toggleAction.performed -= OnToggle;
        toggleAction.Disable();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        debugAddScrapAction.performed -= OnDebugAddScrap;
        debugAddScrapAction.Disable();
#endif
    }

    private void OnToggle(InputAction.CallbackContext _)
    {
        UIManager.Instance?.TogglePopup(UIManager.PopupType.Upgrade);
    }

    private void OnDebugAddScrap(InputAction.CallbackContext _)
    {
        PlayerProgress.AddScrap(debugScrapAmount);
        Debug.Log($"[Upgrade] Scrap +{debugScrapAmount} (현재 {PlayerProgress.Scrap})");
    }
}
