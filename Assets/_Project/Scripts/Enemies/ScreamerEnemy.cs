using DeadZone.Core;
using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Enemies
{
    public sealed class ScreamerEnemy : EnemyAgent
    {
        [SerializeField] EnemyHealth summonPrefab;
        [SerializeField] EnemyData summonData;

        float nextScreamTime;
        float screamUntil;
        Color baseColor = Color.white;

        protected override void Awake()
        {
            base.Awake();
            if (visual != null) baseColor = visual.color;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            nextScreamTime = 0f;
            screamUntil = 0f;
            if (visual != null) visual.color = baseColor;
        }

        protected override void Tick()
        {
            if (visual != null)
                visual.color = Time.time < screamUntil ? new Color(1f, 0.95f, 0.4f) : baseColor;

            if (!TargetVisible)
            {
                Idle();
                return;
            }

            EnemyData data = Data;
            if (Time.time < screamUntil)
            {
                Idle();
                return;
            }

            if (DistanceToTarget <= data.screamRadius && Time.time >= nextScreamTime)
            {
                screamUntil = Time.time + 0.5f;
                nextScreamTime = Time.time + data.screamCooldown;
                AudioManager.PlaySfx(AudioManager.Sfx.Scream);
                Summon(data);
                if (playerVisibility != null) playerVisibility.RevealFor(1.5f);
            }

            Idle();
        }

        void Summon(EnemyData data)
        {
            if (summonPrefab == null || summonData == null) return;

            for (int i = 0; i < data.summonCount; i++)
            {
                float offset = (i % 2 == 0 ? -1f : 1f) * (1.2f + i * 0.25f);
                EnemyHealth zombie = Instantiate(summonPrefab, transform.position + Vector3.right * offset,
                    Quaternion.identity);
                zombie.Initialize(summonData, null);
            }
        }
    }
}
