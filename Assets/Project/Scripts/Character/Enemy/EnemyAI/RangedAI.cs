using UnityEngine;

public class RangedAI : EnemyAI
{
    public float approachDist = 10f;
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
