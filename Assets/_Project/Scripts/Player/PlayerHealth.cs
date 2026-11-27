using System.Collections;
using DeadZone.Core;
using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Player
{
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] PlayerConfig config;
        [SerializeField] PlayerMotor motor;
        [SerializeField] PlayerAim aim;
        [SerializeField] PlayerShooter shooter;

        SpriteRenderer[] renderers;
        float invulnerableUntil;
        bool isDead;
        bool isGameOver;
        int enemyLayer;
        bool enemyCollisionIgnored;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => config.maxHealth;
        public int Lives { get; private set; }
        public bool IsDead => isDead;
        public bool IsGameOver => isGameOver;

        void Awake()
        {
            CurrentHealth = config.maxHealth;
            Lives = config.lives;
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
            enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0) Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, false);
        }

        void Update()
        {
            if (enemyLayer < 0) return;

            bool invulnerable = isDead || Time.time < invulnerableUntil || (motor != null && motor.IsDodging);
            if (invulnerable == enemyCollisionIgnored) return;

            Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, invulnerable);
            enemyCollisionIgnored = invulnerable;
        }

        public void Heal(float amount)
        {
            if (isDead || amount <= 0f) return;
            CurrentHealth = Mathf.Min(config.maxHealth, CurrentHealth + amount);
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(damage, Vector2.zero);
        }

        public void TakeDamage(DamageInfo info)
        {
            TakeDamage(info.amount);
        }

        public void TakeDamage(float damage, Vector2 knockback)
        {
            if (isDead || Time.time < invulnerableUntil || (motor != null && motor.IsDodging)) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            invulnerableUntil = Time.time + config.invulnerabilityTime;

            if (knockback.sqrMagnitude > 0.01f && motor != null) motor.ApplyKnockback(knockback);
            if (CurrentHealth <= 0f) Die();
        }

        void Die()
        {
            isDead = true;
            CurrentHealth = 0f;
            Lives--;
            SetPlayerEnabled(false);
            SetVisualsVisible(false);

            if (Lives <= 0)
            {
                isGameOver = true;
                return;
            }

            StartCoroutine(RespawnRoutine());
        }

        IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(config.respawnDelay);

            motor.Respawn();
            CurrentHealth = config.maxHealth;
            isDead = false;
            invulnerableUntil = Time.time + config.respawnInvulnerability;
            SetVisualsVisible(true);
            SetPlayerEnabled(true);
        }

        void SetPlayerEnabled(bool enabled)
        {
            motor.enabled = enabled;
            aim.enabled = enabled;
            shooter.enabled = enabled;
        }

        void SetVisualsVisible(bool visible)
        {
            foreach (SpriteRenderer renderer in renderers) renderer.enabled = visible;
        }
    }
}
