using DeadZone.Combat;
using UnityEngine;

namespace DeadZone.Data
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Dead Zone/Weapon Data")]
    public sealed class WeaponData : ScriptableObject
    {
        public string weaponName = "Pistola";
        public Sprite heldSprite;

        [Header("Damage")]
        public float damage = 10f;

        [Header("Fire")]
        public bool automatic;
        public float shotsPerSecond = 4f;
        public float spreadDegrees = 0.5f;
        public int pellets = 1;

        [Header("Ammo")]
        public bool infiniteAmmo = true;
        public int startingAmmo = 0;

        [Header("Projectile")]
        public Projectile projectilePrefab;
        public float projectileSpeed = 26f;
        public float projectileLifetime = 1f;

        [Header("Special")]
        [Tooltip("Radio de explosión en unidades. 0 = sin explosión.")]
        public float explosionRadius = 0f;
        public float burnDuration = 0f;
        public float burnDamagePerSecond = 0f;
    }
}
