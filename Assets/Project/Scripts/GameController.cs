using UnityEngine;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        SaveController.Instance.Load();
        Debug.Log(GameObject.FindGameObjectWithTag("Player").name);
        CameraRebinder.BindTo(GameObject.FindGameObjectWithTag("Player").transform);
    }
}
