using System.Collections.Generic;
using DeadZone.Core;
using UnityEngine;

namespace DeadZone.Enemies
{
    public sealed class AcidPool : MonoBehaviour
    {
        [SerializeField] float radius = 0.9f;
        [SerializeField] float duration = 3f;
        [SerializeField] float damagePerSecond = 8f;
        [SerializeField] LayerMask damageMask;

        float endTime;
        float nextTick;

        public void Configure(float newDuration, float newDamagePerSecond, float newRadius)
        {
            duration = newDuration;
            damagePerSecond = newDamagePerSecond;
            radius = newRadius;
        }

        void Start()
        {
            endTime = Time.time + duration;
            nextTick = Time.time;
        }

        void Update()
        {
            if (Time.time >= endTime)
            {
                Destroy(gameObject);
                return;
            }

            if (Time.time < nextTick) return;

            nextTick = Time.time + 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, damageMask);
            HashSet<IDamageable> damaged = new HashSet<IDamageable>();

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || damaged.Contains(damageable)) continue;

                damaged.Add(damageable);
                damageable.TakeDamage(new DamageInfo(damagePerSecond * 0.5f, transform.position, true));
            }
        }
    }
}
