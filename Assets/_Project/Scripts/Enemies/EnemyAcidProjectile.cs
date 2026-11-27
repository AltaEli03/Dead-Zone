using DeadZone.Core;
using UnityEngine;

namespace DeadZone.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyAcidProjectile : MonoBehaviour
    {
        [SerializeField] LayerMask hitMask;
        [SerializeField] float lifetime = 4f;

        Rigidbody2D body;
        float damage;
        AcidPool poolTemplate;
        float poolDuration;
        float poolDamage;
        float poolRadius;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector2 velocity, float gravityScale, float hitDamage,
            AcidPool poolPrefab, float duration, float poolDps, float radius)
        {
            body.gravityScale = gravityScale;
            body.linearVelocity = velocity;
            damage = hitDamage;
            poolTemplate = poolPrefab;
            poolDuration = duration;
            poolDamage = poolDps;
            poolRadius = radius;
        }

        void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f) Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if ((hitMask.value & (1 << other.gameObject.layer)) == 0) return;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null) damageable.TakeDamage(new DamageInfo(damage, transform.position, true));

            SpawnPool();
        }

        void SpawnPool()
        {
            if (poolTemplate != null)
            {
                AcidPool pool = Instantiate(poolTemplate, transform.position, Quaternion.identity);
                pool.Configure(poolDuration, poolDamage, poolRadius);
            }

            Destroy(gameObject);
        }
    }
}
