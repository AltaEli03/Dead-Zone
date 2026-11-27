using System;
using System.Collections;
using DeadZone.Combat;
using DeadZone.Core;
using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Enemies
{
    public sealed class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] EnemyData data;
        [SerializeField] Collider2D[] colliders;
        [SerializeField] Shield shield;

        ZombieSpawner spawner;
        float currentHealth;
        bool isDead;
        Coroutine burnRoutine;

        public event Action<EnemyHealth> Died;

        public static int ActiveCount { get; private set; }
        public EnemyData Data => data;
        public bool IsDead => isDead;
        public float CurrentHealth => currentHealth;
        public Vector3 SpawnPosition { get; private set; }

        void OnEnable()
        {
            ActiveCount++;
        }

        void OnDisable()
        {
            ActiveCount--;
            if (burnRoutine != null)
            {
                StopCoroutine(burnRoutine);
                burnRoutine = null;
            }
        }

        public void ApplyBurn(float damagePerSecond, float duration)
        {
            if (isDead || damagePerSecond <= 0f || duration <= 0f) return;

            if (burnRoutine != null) StopCoroutine(burnRoutine);
            burnRoutine = StartCoroutine(BurnRoutine(damagePerSecond, duration));
        }

        IEnumerator BurnRoutine(float damagePerSecond, float duration)
        {
            const float tick = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration && !isDead)
            {
                yield return new WaitForSeconds(tick);
                elapsed += tick;
                if (isDead) break;
                TakeDamage(damagePerSecond * tick);
            }

            burnRoutine = null;
        }

        void Awake()
        {
            if (colliders == null || colliders.Length == 0)
                colliders = GetComponentsInChildren<Collider2D>(true);
        }

        void Start()
        {
            if (spawner == null && data != null && currentHealth <= 0f)
            {
                currentHealth = data.maxHealth;
                SpawnPosition = transform.position;

                foreach (Collider2D collider2D in colliders) collider2D.enabled = true;
                foreach (DamageZone zone in GetComponentsInChildren<DamageZone>(true))
                    zone.SetMultiplier(data.headshotMultiplier);
            }
        }

        public void Initialize(EnemyData enemyData, ZombieSpawner owner)
        {
            data = enemyData;
            spawner = owner;
            currentHealth = data.maxHealth;
            isDead = false;
            SpawnPosition = transform.position;

            foreach (Collider2D collider2D in colliders) collider2D.enabled = true;
            foreach (DamageZone zone in GetComponentsInChildren<DamageZone>(true))
                zone.SetMultiplier(data.headshotMultiplier);
        }

        public void Heal(float amount)
        {
            if (isDead || data == null || amount <= 0f) return;
            currentHealth = Mathf.Min(data.maxHealth, currentHealth + amount);
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(new DamageInfo(damage, transform.position, true));
        }

        public void TakeDamage(DamageInfo info)
        {
            if (isDead) return;

            if (shield != null && shield.Blocks(info))
            {
                shield.Flash();
                return;
            }

            currentHealth -= info.amount;
            if (currentHealth <= 0f) Die();
        }

        void Die()
        {
            isDead = true;
            currentHealth = 0f;
            AudioManager.PlaySfx(AudioManager.Sfx.Zombie);

            Died?.Invoke(this);
            foreach (Collider2D collider2D in colliders) collider2D.enabled = false;
            if (spawner != null) spawner.OnZombieDied(this);
            else gameObject.SetActive(false);
        }
    }
}
