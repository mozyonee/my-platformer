using UnityEngine;

public class ChaseState : IState
{
    private readonly EnemyController ctx;
    private float lostTimer;

    public ChaseState(EnemyController ctx) => this.ctx = ctx;

    public void Enter() => lostTimer = 0f;

    public void Tick()
    {
        float d = ctx.DistanceToTarget;
        bool lost = d > ctx.Stats.chaseRange * ctx.Stats.chaseExitFactor
            || !ctx.TargetOnSameLevel;

        if (lost)
        {
            lostTimer += Time.deltaTime;
            if (lostTimer >= ctx.Stats.loseTargetTime)
            {
                ctx.Machine.ChangeState(ctx.PatrolState);
                return;
            }
        }
        else
        {
            lostTimer = 0f;
        }

        ctx.FaceTarget();

        bool canAdvance = ctx.DistanceToTarget > ctx.Stats.stopDistance
            && !ctx.AtWall();
        if (canAdvance) ctx.Move(ctx.Stats.chaseSpeed);
        else ctx.Stop();

        if (d <= ctx.Stats.attackRange && ctx.TargetOnSameLevel)
            ctx.Machine.ChangeState(ctx.AttackState);
    }

    public void Exit() => ctx.Stop();
}