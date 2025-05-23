using UnityEngine;

public class ParallexBackgroundManager : MonoBehaviour
{
    ParallexBackground[] backgrounds;
    public float[] speeds;
    public float zoomOffset;
    [SerializeField] Transform virtualCamera;

    private void Awake()
    {
        backgrounds = GetComponentsInChildren<ParallexBackground>();
        SetReferenceTransform();
        ArrangeSpeed();
    }

    private void SetReferenceTransform()
    {
        foreach (var bg in backgrounds)
        {
            bg.target = virtualCamera;
            bg.zoomOffset = zoomOffset;
        }
    }

    private void ArrangeSpeed()
    {
        if(speeds.Length >= backgrounds.Length)
        {
            int i = 0;
            foreach (var bg in backgrounds)
            {
                bg.parallaxSpeed = speeds[i++];
            }
            return;
        }
        foreach(var bg in backgrounds)
        {
            bg.parallaxSpeed = Random.Range(0.2f, 0.8f);
        }
    }

    public void ZoomBackgrounds(float zoomSize)
    {
        Debug.Log(zoomSize);
        foreach(var bg in backgrounds)
        {
            bg.ZoomImage(zoomSize);
        }    
    }
}
