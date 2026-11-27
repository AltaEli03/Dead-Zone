using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Enemies
{
    public sealed class MutantEnemy : EnemyAgent
    {
        float nextWanderTime;
        float wanderDirection = 1f;

        protected override void OnEnable()
        {
            base.OnEnable();
            nextWanderTime = 0f;
            wanderDirection = 1f;
        }

        protected override void Tick()
        {
            EnemyData data = Data;

            if (playerVisibility != null && DistanceToTarget <= data.alertRadius)
                playerVisibility.RevealFor(0.4f);

            if (Time.time >= nextWanderTime)
            {
                nextWanderTime = Time.time + data.erraticInterval;
                bool engaged = DistanceToTarget <= data.activationRange;
                if (engaged)
                {
                    float toPlayer = Mathf.Sign(HorizontalDistance);
                    wanderDirection = Random.value < 0.7f ? toPlayer : (Random.value < 0.5f ? -1f : 1f);
                }
                else
                {
                    Vector3 home = health.SpawnPosition;
                    float fromHome = transform.position.x - home.x;
                    wanderDirection = Mathf.Abs(fromHome) > 3f
                        ? Mathf.Sign(home.x - transform.position.x)
                        : (Random.value < 0.5f ? -1f : 1f);
                }

                if (Random.value < 0.15f) wanderDirection = 0f;
            }

            if (wanderDirection == 0f) Idle();
            else
            {
                SetFacing(wanderDirection);
                MoveHorizontal(wanderDirection, data.moveSpeed);
            }

            if (InMeleeRange(data)) TryMeleeAttack(data);
        }
    }
}
