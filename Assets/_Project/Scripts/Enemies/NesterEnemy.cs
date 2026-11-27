using System.Collections.Generic;
using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Enemies
{
    public sealed class NesterEnemy : ZombieChase
    {
        [SerializeField] EnemyHealth offspringPrefab;
        [SerializeField] EnemyData offspringData;
        [SerializeField] LayerMask enemyMask;

        float nextRegenTime;
        bool spawnedOffspring;

        protected override void OnEnable()
        {
            base.OnEnable();
            spawnedOffspring = false;
            nextRegenTime = 0f;
            if (health != null) health.Died += OnDied;
        }

        protected override void OnDisable()
        {
            if (health != null) health.Died -= OnDied;
            base.OnDisable();
        }

        protected override void Tick()
        {
            if (!TargetVisible)
            {
                Idle();
                return;
            }

            base.Tick();

            if (Time.time < nextRegenTime) return;
            nextRegenTime = Time.time + 0.5f;

            EnemyData data = Data;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, data.regenRadius, enemyMask);
            HashSet<EnemyHealth> healed = new HashSet<EnemyHealth>();

            foreach (Collider2D hit in hits)
            {
                EnemyHealth other = hit.GetComponentInParent<EnemyHealth>();
                if (other == null || other == health || other.IsDead || healed.Contains(other)) continue;

                healed.Add(other);
                other.Heal(data.regenPerSecond * 0.5f);
            }
        }

        void OnDied(EnemyHealth enemy)
        {
            if (spawnedOffspring || offspringPrefab == null || offspringData == null) return;
            spawnedOffspring = true;

            int count = Data != null ? Data.offspringCount : 3;
            for (int i = 0; i < count; i++)
            {
                float offset = (i - (count - 1) * 0.5f) * 0.55f;
                EnemyHealth offspring = Instantiate(offspringPrefab, transform.position + Vector3.right * offset,
                    Quaternion.identity);
                offspring.Initialize(offspringData, null);
            }
        }
    }
}
