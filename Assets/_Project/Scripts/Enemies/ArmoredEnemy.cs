using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Enemies
{
    public sealed class ArmoredEnemy : ZombieChase
    {
        [SerializeField] Shield shield;

        float nextTurnTime;

        protected override void Awake()
        {
            base.Awake();
            if (shield != null) shield.SetBlockAngle(Data != null ? Data.shieldBlockAngle : 90f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            nextTurnTime = 0f;
        }

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
                if (shield != null) shield.SetFacing(facing);
                return;
            }

            Vector2 delta = (Vector2)target.position - (Vector2)transform.position;

            if (Time.time >= nextTurnTime)
            {
                float desired = Mathf.Sign(delta.x);
                if (Mathf.Abs(desired) > 0.01f && desired != facing)
                {
                    SetFacing(desired);
                    nextTurnTime = Time.time + data.turnDelay;
                }
            }

            if (Mathf.Abs(delta.x) > data.attackRange || Mathf.Abs(delta.y) > 1.5f)
                MoveHorizontal(facing, data.moveSpeed);
            else
            {
                Idle();
                TryMeleeAttack(data);
            }

            if (shield != null) shield.SetFacing(facing);
        }
    }
}
