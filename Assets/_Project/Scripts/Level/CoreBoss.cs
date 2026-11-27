using DeadZone.Core;
using DeadZone.Enemies;
using UnityEngine;

namespace DeadZone.Level
{
    public sealed class CoreBoss : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealth = 1000f;
        [SerializeField] ZombieSpawner waveSpawner;
        [SerializeField] LevelController levelController;
        [SerializeField] SpriteRenderer visual;
        [SerializeField] float openDuration = 5f;
        [SerializeField] float activationRange = 14f;
        [SerializeField] Color closedColor = new Color(0.25f, 0.55f, 0.3f);
        [SerializeField] Color openColor = new Color(0.6f, 1f, 0.45f);

        float currentHealth;
        float closeTime;
        bool open;
        bool waveActive;
        bool started;
        bool dead;
        Transform target;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsOpen => open;
        public bool IsDead => dead;

        void Start()
        {
            currentHealth = maxHealth;
            if (visual != null) visual.color = closedColor;
        }

        void Update()
        {
            if (dead) return;

            if (!started)
            {
                FindTarget();
                if (target != null && Vector2.Distance(target.position, transform.position) <= activationRange)
                {
                    started = true;
                    StartNextWave();
                }
                return;
            }

            if (waveActive && waveSpawner != null && waveSpawner.WaveCleared)
            {
                waveActive = false;
                Open();
            }

            if (open && Time.time >= closeTime)
            {
                open = false;
                if (visual != null) visual.color = closedColor;
                StartNextWave();
            }
        }

        void FindTarget()
        {
            if (target != null) return;
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }

        void StartNextWave()
        {
            if (waveSpawner != null) waveSpawner.SpawnWave();
            waveActive = true;
            open = false;
            if (visual != null) visual.color = closedColor;
            if (levelController != null) levelController.ShowStatus("NÚCLEO PROTEGIDO");
        }

        void Open()
        {
            open = true;
            closeTime = Time.time + openDuration;
            if (visual != null) visual.color = openColor;
            if (levelController != null) levelController.ShowStatus("NÚCLEO VULNERABLE");
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(new DamageInfo(damage, transform.position, true));
        }

        public void TakeDamage(DamageInfo info)
        {
            if (dead || !open) return;

            currentHealth -= info.amount;
            if (currentHealth <= 0f) Die();
        }

        void Die()
        {
            dead = true;
            open = false;
            currentHealth = 0f;
            if (visual != null) visual.enabled = false;
            if (levelController != null) levelController.NotifyCoreDestroyed();
        }
    }
}
