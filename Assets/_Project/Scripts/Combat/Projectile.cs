using System.Collections.Generic;
using DeadZone.Core;
using DeadZone.Enemies;
using UnityEngine;
using UnityEngine.Pool;

namespace DeadZone.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] LayerMask hitMask;
        [SerializeField] GameObject impactEffect;

        Rigidbody2D body;
        ObjectPool<Projectile> pool;
        float lifeRemaining;
        float explosionRadius;
        float burnDuration;
        float burnDamagePerSecond;
        bool released;

        public float Damage { get; private set; }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public void SetPool(ObjectPool<Projectile> owner)
        {
            pool = owner;
        }

        public void Initialize(Vector2 direction, float speed, float damage, float lifetime,
            float explosionRadius = 0f, float burnDuration = 0f, float burnDamagePerSecond = 0f)
        {
            Damage = damage;
            lifeRemaining = lifetime;
            this.explosionRadius = explosionRadius;
            this.burnDuration = burnDuration;
            this.burnDamagePerSecond = burnDamagePerSecond;
            released = false;
            body.linearVelocity = direction * speed;
        }

        void Update()
        {
            lifeRemaining -= Time.deltaTime;
            if (lifeRemaining > 0f) return;

            if (explosionRadius > 0f) Explode();
            else Release();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (released) return;
            if ((hitMask.value & (1 << other.gameObject.layer)) == 0) return;

            if (explosionRadius > 0f)
            {
                Explode();
                return;
            }

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                DamageZone zone = other.GetComponent<DamageZone>();
                float multiplier = zone != null ? zone.DamageMultiplier : 1f;
                damageable.TakeDamage(new DamageInfo(Damage * multiplier, transform.position));

                if (burnDuration > 0f && damageable is EnemyHealth enemy)
                    enemy.ApplyBurn(burnDamagePerSecond, burnDuration);
            }

            Release();
        }

        void Explode()
        {
            if (released) return;

            if (impactEffect != null) Instantiate(impactEffect, transform.position, Quaternion.identity);

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitMask);
            HashSet<IDamageable> damaged = new HashSet<IDamageable>();

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || damaged.Contains(damageable)) continue;

                damaged.Add(damageable);
                damageable.TakeDamage(new DamageInfo(Damage, transform.position, true));
            }

            Release();
        }

        void Release()
        {
            if (released) return;

            released = true;
            if (pool != null) pool.Release(this);
            else Destroy(gameObject);
        }

        public void ResetForPool()
        {
            body.linearVelocity = Vector2.zero;
            lifeRemaining = 0f;
        }
    }
}
