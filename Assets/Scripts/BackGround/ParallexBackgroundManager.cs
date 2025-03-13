using UnityEngine;

public class ParallexBackgroundManager : MonoBehaviour
{
    ParallexBackground[] backgrounds;
    public float[] speeds;
    [SerializeField] Transform virtualCamera;

    private void Awake()
    {
        backgrounds = GetComponentsInChildren<ParallexBackground>();
        SetReferenceTransform();
        RandomizeSpeed();
    }

    private void SetReferenceTransform()
    {
        foreach (var bg in backgrounds)
        {
            bg.target = virtualCamera;
        }
    }

    private void RandomizeSpeed()
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
}
