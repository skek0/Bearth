using UnityEngine;

public class MeleeAI : EnemyAI
{
    public float approachDist = 1f;
    public override void UpdateAI(EnemyContext ctx)
    {
        float distance = Vector2.Distance(transform.position, ctx.target.transform.position);

        TurnTo(ctx.target.transform.position);
        if (distance > approachDist)
        {
            MoveTo(ctx.target.transform.position);
        }
    }

}