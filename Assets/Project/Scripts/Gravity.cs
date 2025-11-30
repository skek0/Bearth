using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] private Transform mass;
    public float gravityStrength;
    private List<Rigidbody2D> inGravity = new(); // 중력장 내 객체 목록

    private void Start()
    {
        mass = transform.parent;
    }

    private void FixedUpdate()
    {
        foreach (Rigidbody2D rb in inGravity)
        {
            if (rb != null)
            {
                Vector2 direction = (transform.position - rb.transform.position).normalized;
                rb.AddForce(direction * gravityStrength * rb.mass * Time.deltaTime);
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Attractable"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if(rb != null) 
            {
                inGravity.Add(rb);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Attractable"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                inGravity.Remove(rb);
            }
        }
    }
}
