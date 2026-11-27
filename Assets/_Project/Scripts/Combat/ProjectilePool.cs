using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace DeadZone.Combat
{
    public sealed class ProjectilePool : MonoBehaviour
    {
        readonly Dictionary<Projectile, ObjectPool<Projectile>> pools = new Dictionary<Projectile, ObjectPool<Projectile>>();
        ObjectPool<Projectile> pool;

        public void Configure(Projectile projectilePrefab)
        {
            if (projectilePrefab == null) return;
            if (pools.TryGetValue(projectilePrefab, out pool)) return;

            // One pool per projectile type so a pooled bullet never returns into another weapon's pool.
            ObjectPool<Projectile> created = null;
            created = new ObjectPool<Projectile>(
                () => Create(projectilePrefab, created),
                OnGet, OnRelease, DestroyProjectile, true, 16, 64);
            pools.Add(projectilePrefab, created);
            pool = created;
        }

        public Projectile Get(Vector3 position, Quaternion rotation)
        {
            if (pool == null) return null;

            Projectile projectile = pool.Get();
            projectile.transform.SetPositionAndRotation(position, rotation);
            return projectile;
        }

        Projectile Create(Projectile projectilePrefab, ObjectPool<Projectile> owner)
        {
            Projectile projectile = Instantiate(projectilePrefab);
            projectile.gameObject.SetActive(false);
            projectile.SetPool(owner);
            return projectile;
        }

        void OnGet(Projectile projectile)
        {
            projectile.gameObject.SetActive(true);
        }

        void OnRelease(Projectile projectile)
        {
            projectile.ResetForPool();
            projectile.gameObject.SetActive(false);
        }

        void DestroyProjectile(Projectile projectile)
        {
            if (projectile != null) Destroy(projectile.gameObject);
        }
    }
}
