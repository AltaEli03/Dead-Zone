using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Enemies
{
    public class ZombieChase : EnemyAgent
    {
        protected override void Tick()
        {
            if (!TargetVisible)
            {
                Idle();
                return;
            }

            EnemyData data = Data;
            if (DistanceToTarget > data.activationRange)
            {
                Idle();
                return;
            }

            Vector2 delta = (Vector2)target.position - (Vector2)transform.position;

            if (Mathf.Abs(delta.x) > data.attackRange || Mathf.Abs(delta.y) > 1.5f)
            {
                float direction = Mathf.Sign(delta.x);
                SetFacing(direction);
                MoveHorizontal(direction, data.moveSpeed);
            }
            else
            {
                Idle();
                TryMeleeAttack(data);
            }
        }
    }
}
