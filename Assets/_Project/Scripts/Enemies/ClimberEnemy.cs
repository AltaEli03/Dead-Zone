using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Enemies
{
    public sealed class ClimberEnemy : ZombieChase
    {
        [SerializeField] LayerMask groundMask;
        [SerializeField] bool startsClimbing;

        bool climbing;
        float nextClimbCheck;

        protected override void OnEnable()
        {
            base.OnEnable();
            climbing = startsClimbing;
            body.gravityScale = climbing ? 0f : defaultGravityScale;
            body.linearVelocity = Vector2.zero;
            nextClimbCheck = 0f;
        }

        protected override void Tick()
        {
            if (!TargetVisible)
            {
                Idle();
                return;
            }

            EnemyData data = Data;
            if (climbing)
            {
                SetFacing(HorizontalDistance);
                float vertical = target.position.y - transform.position.y;
                if (Mathf.Abs(HorizontalDistance) <= data.attackRange + 0.4f && vertical < -0.4f)
                {
                    climbing = false;
                    body.gravityScale = defaultGravityScale;
                    return;
                }

                body.linearVelocity = vertical > 0.5f
                    ? new Vector2(0f, data.climbSpeed)
                    : Vector2.zero;
                return;
            }

            base.Tick();

            if (Time.time < nextClimbCheck) return;
            nextClimbCheck = Time.time + 0.25f;

            if (target.position.y <= transform.position.y + 0.6f) return;
            RaycastHit2D wall = Physics2D.Raycast(transform.position, Vector2.right * facing, 0.45f, groundMask);
            if (wall.collider == null) return;

            climbing = true;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
        }
    }
}
