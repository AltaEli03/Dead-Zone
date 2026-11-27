using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Enemies
{
    public sealed class SpitterEnemy : EnemyAgent
    {
        [SerializeField] EnemyAcidProjectile projectilePrefab;
        [SerializeField] AcidPool poolPrefab;
        [SerializeField] Transform firePoint;

        float nextFireTime;

        protected override void OnEnable()
        {
            base.OnEnable();
            nextFireTime = 0f;
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
                return;
            }

            Vector2 delta = (Vector2)target.position - (Vector2)transform.position;
            SetFacing(delta.x);

            float distance = Mathf.Abs(delta.x);
            if (distance > data.preferredRange + 0.8f) MoveHorizontal(delta.x, data.moveSpeed);
            else if (distance < data.preferredRange - 1.2f) MoveHorizontal(-delta.x, data.moveSpeed);
            else Idle();

            if (Time.time < nextFireTime) return;

            nextFireTime = Time.time + data.attackCooldown;
            if (InMeleeRange(data) || projectilePrefab == null)
            {
                TryMeleeAttack(data);
                return;
            }

            Fire(data);
        }

        void Fire(EnemyData data)
        {
            Vector2 origin = firePoint != null
                ? (Vector2)firePoint.position
                : (Vector2)transform.position + Vector2.up * 0.6f;
            Vector2 destination = (Vector2)target.position + Vector2.up * 0.4f;
            Vector2 delta = destination - origin;

            float gravity = Mathf.Abs(Physics2D.gravity.y) * data.projectileGravity;
            float time = Mathf.Clamp(Mathf.Abs(delta.x) / Mathf.Max(data.projectileSpeed, 0.1f), 0.45f, 1.2f);
            Vector2 velocity = new Vector2(
                delta.x / time,
                (delta.y + 0.5f * gravity * time * time) / time);

            EnemyAcidProjectile projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
            projectile.Initialize(velocity, data.projectileGravity, data.attackDamage, poolPrefab,
                data.acidDuration, data.acidDamage, data.acidRadius);
        }
    }
}
