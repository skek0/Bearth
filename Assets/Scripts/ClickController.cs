using UnityEngine;
using UnityEngine.InputSystem;

public class ClickController : MonoBehaviour
{
    [SerializeField] InputAction clickAction;
    private IClickable clickedObject;  // ���� ���õ� ������Ʈ

    private void Awake()
    {
        clickAction = InputSystem.actions.FindAction("Shift");
        clickAction.performed += ctx => OnClickStart();
        clickAction.canceled += ctx => OnClickEnd();
    }

    private void OnEnable() => clickAction.Enable();
    private void OnDisable() => clickAction.Disable();

    private void OnClickStart()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            IClickable selectable = hit.collider.GetComponent<IClickable>();
            if (selectable != null)
            {
                // ���� ������Ʈ ���� ����
                clickedObject?.OnDeselected();
                // ���ο� ������Ʈ ����
                clickedObject = selectable;
                clickedObject.OnSelected();
            }
        }
    }

    private void OnClickEnd()
    {
        // Ŭ���� ������ ���� ����
        clickedObject?.OnDeselected();
        clickedObject = null;
    }
}
