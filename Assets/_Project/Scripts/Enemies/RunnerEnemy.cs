using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Enemies
{
    public sealed class RunnerEnemy : ZombieChase
    {
        [SerializeField] LayerMask groundMask;
        [SerializeField] float jumpCheckDistance = 0.8f;

        bool awake;
        float nextJumpTime;

        protected override void OnEnable()
        {
            base.OnEnable();
            awake = false;
            nextJumpTime = 0f;
        }

        protected override void Tick()
        {
            if (!TargetVisible)
            {
                Idle();
                return;
            }

            EnemyData data = Data;
            if (!awake)
            {
                if (Mathf.Abs(HorizontalDistance) > data.ambushRange)
                {
                    Idle();
                    return;
                }

                awake = true;
            }

            base.Tick();

            if (Time.time < nextJumpTime || Mathf.Abs(body.linearVelocity.y) > 0.1f) return;

            Vector2 origin = (Vector2)transform.position + Vector2.up * 0.2f;
            bool noGroundAhead = Physics2D.Raycast(origin + Vector2.right * facing * jumpCheckDistance,
                Vector2.down, 0.8f, groundMask).collider == null;
            bool wallAhead = Physics2D.Raycast(origin, Vector2.right * facing, 0.45f, groundMask).collider != null;
            if (!noGroundAhead && !wallAhead) return;

            body.linearVelocity = new Vector2(body.linearVelocity.x, data.jumpVelocity);
            nextJumpTime = Time.time + 0.8f;
        }
    }
}
