using DeadZone.Data;
using DeadZone.Player;
using UnityEngine;

namespace DeadZone.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth))]
    public abstract class EnemyAgent : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer visual;

        protected Rigidbody2D body;
        protected EnemyHealth health;
        protected Transform target;
        protected PlayerHealth targetHealth;
        protected PlayerVisibility playerVisibility;
        protected float facing = 1f;
        protected float nextAttackTime;
        protected float defaultGravityScale;

        protected EnemyData Data => health != null ? health.Data : null;
        protected bool TargetVisible => playerVisibility == null || !playerVisibility.IsHidden;

        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<EnemyHealth>();
            defaultGravityScale = body.gravityScale;
        }

        protected virtual void OnEnable()
        {
            nextAttackTime = 0f;
            target = null;
            targetHealth = null;
            playerVisibility = null;
        }

        protected virtual void OnDisable()
        {
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        protected virtual void Update()
        {
            if (health == null || health.IsDead || Data == null) return;

            // shortcut: fall reset keeps the wave killable if an enemy is pushed through
            // the floor by the group; replace with proper level physics when built.
            if (transform.position.y < -8f)
            {
                transform.position = health.SpawnPosition;
                body.linearVelocity = Vector2.zero;
                return;
            }

            EnsureTarget();
            if (target == null) return;

            Tick();
        }

        protected abstract void Tick();

        protected void EnsureTarget()
        {
            if (target != null) return;

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;

            target = player.transform;
            targetHealth = player.GetComponent<PlayerHealth>();
            playerVisibility = player.GetComponent<PlayerVisibility>();
        }

        protected float HorizontalDistance => target != null ? target.position.x - transform.position.x : 0f;

        protected float DistanceToTarget => target != null
            ? Vector2.Distance(target.position, transform.position)
            : float.MaxValue;

        protected void SetFacing(float direction)
        {
            if (Mathf.Abs(direction) < 0.01f) return;

            facing = Mathf.Sign(direction);
            if (visual != null) visual.flipX = facing < 0f;
        }

        protected void MoveHorizontal(float direction, float speed)
        {
            body.linearVelocity = new Vector2(Mathf.Sign(direction) * speed, body.linearVelocity.y);
        }

        protected void StopHorizontal()
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
        }

        protected bool InMeleeRange(EnemyData data)
        {
            if (target == null) return false;

            Vector2 delta = target.position - transform.position;
            return Mathf.Abs(delta.x) <= data.attackRange && Mathf.Abs(delta.y) <= 1.5f;
        }

        protected void TryMeleeAttack(EnemyData data)
        {
            if (targetHealth == null || Time.time < nextAttackTime) return;

            Vector2 knockDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
            Vector2 knockback = new Vector2(knockDirection.x * data.knockbackForce, data.knockbackUp);
            targetHealth.TakeDamage(data.attackDamage, knockback);
            nextAttackTime = Time.time + data.attackCooldown;
        }

        protected void Idle()
        {
            StopHorizontal();
        }
    }
}
