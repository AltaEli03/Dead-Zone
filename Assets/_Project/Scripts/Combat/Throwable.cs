using System.Collections.Generic;
using DeadZone.Core;
using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Throwable : MonoBehaviour
    {
        [SerializeField] ThrowableData data;
        [SerializeField] GameObject explosionEffect;
        [SerializeField] FireZone fireZonePrefab;
        [SerializeField] LayerMask damageMask;

        Rigidbody2D body;
        float fuse;
        bool exploded;
        int groundLayer;

        public ThrowableData Data => data;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            groundLayer = LayerMask.NameToLayer("Ground");
        }

        public void Initialize(Vector2 velocity)
        {
            body.linearVelocity = velocity;
            fuse = data != null ? data.fuseTime : 1.2f;
        }

        public void IgnoreCollisionWith(Collider2D other)
        {
            Collider2D own = GetComponent<Collider2D>();
            if (own != null && other != null) Physics2D.IgnoreCollision(own, other, true);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (data != null && data.kind == ThrowableKind.Molotov && collision.collider.gameObject.layer == groundLayer)
                Explode();
        }

        void Update()
        {
            if (exploded) return;

            fuse -= Time.deltaTime;
            if (fuse <= 0f) Explode();
        }

        void Explode()
        {
            if (exploded) return;
            exploded = true;

            AudioManager.PlaySfx(AudioManager.Sfx.Explosion);
            if (data != null && data.kind == ThrowableKind.Grenade)
            {
                if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);

                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, data.radius, damageMask);
                HashSet<IDamageable> damaged = new HashSet<IDamageable>();

                foreach (Collider2D hit in hits)
                {
                    IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                    if (damageable == null || damaged.Contains(damageable)) continue;

                    damaged.Add(damageable);
                    damageable.TakeDamage(new DamageInfo(data.damage, transform.position, true));
                }
            }
            else if (data != null && data.kind == ThrowableKind.Molotov && fireZonePrefab != null)
            {
                FireZone fire = Instantiate(fireZonePrefab, transform.position, Quaternion.identity);
                fire.Configure(data.fireDuration, data.fireDamagePerSecond, data.fireLength, data.fireHeight, data.fireSpreadDuration);
                if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
