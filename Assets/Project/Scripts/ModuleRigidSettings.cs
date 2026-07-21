using UnityEngine;

[System.Serializable]
public class ModuleRigidSettings
{
    public float mass;
    public float gravityScale;
    public float drag;
    public float angularDrag;
    //public RigidbodyInterpolation2D interpolation;
    //public RigidbodySleepMode2D sleepMode;
    //public CollisionDetectionMode2D collisionDetectionMode;
    //public RigidbodyType2D bodyType;
    //public RigidbodyConstraints2D constraints;

    public void ApplyTo(Rigidbody2D rb)
    {
        rb.mass = mass;
        rb.gravityScale = gravityScale;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        //rb.interpolation = interpolation;
        //rb.sleepMode = sleepMode;
        //rb.collisionDetectionMode = collisionDetectionMode;
        //rb.bodyType = bodyType;
        //rb.constraints = constraints;
    }

    public static ModuleRigidSettings From(Rigidbody2D rb)
    {
        return new ModuleRigidSettings
        {
            mass = rb.mass,
            gravityScale = rb.gravityScale,
            drag = rb.linearDamping,
            angularDrag = rb.angularDamping,
            //interpolation = rb.interpolation,
            //sleepMode = rb.sleepMode,
            //collisionDetectionMode = rb.collisionDetectionMode,
            //bodyType = rb.bodyType,
            //constraints = rb.constraints
        };
    }
}
