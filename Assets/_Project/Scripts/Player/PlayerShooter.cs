using DeadZone.Combat;
using DeadZone.Core;
using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Player
{
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerShooter : MonoBehaviour
    {
        [SerializeField] PlayerInputReader input;
        [SerializeField] WeaponData weapon;
        [SerializeField] float ammoMultiplier = 1f;
        [SerializeField] SpriteRenderer heldWeaponRenderer;
        [SerializeField] Transform muzzle;
        [SerializeField] ProjectilePool pool;

        WeaponData startingWeapon;
        PlayerPowerUps powerUps;
        int currentAmmo;
        float nextShotTime;
        bool fireHeldOnEquip;

        public WeaponData CurrentWeapon => weapon;
        public string WeaponName => weapon != null ? weapon.weaponName : string.Empty;
        public int CurrentAmmo => currentAmmo;
        public bool HasInfiniteAmmo => weapon != null && weapon.infiniteAmmo;

        void Awake()
        {
            powerUps = GetComponent<PlayerPowerUps>();
            startingWeapon = weapon;
            SetAmmoFor(weapon);
            if (pool != null && weapon != null) pool.Configure(weapon.projectilePrefab);
            ApplyWeaponVisual(weapon);
        }

        void OnEnable()
        {
            input.FirePressed += OnFirePressed;
        }

        void OnDisable()
        {
            input.FirePressed -= OnFirePressed;
        }

        void Update()
        {
            if (!input.FireHeld) fireHeldOnEquip = false;
            if (weapon != null && weapon.automatic && input.FireHeld && !fireHeldOnEquip) TryFire();
        }

        public void Configure(WeaponData newWeapon, float newAmmoMultiplier)
        {
            if (newWeapon != null) weapon = newWeapon;
            ammoMultiplier = newAmmoMultiplier <= 0f ? 1f : newAmmoMultiplier;

            startingWeapon = weapon;
            SetAmmoFor(weapon);
            nextShotTime = 0f;
            fireHeldOnEquip = false;
            if (pool != null && weapon != null) pool.Configure(weapon.projectilePrefab);
            ApplyWeaponVisual(weapon);
        }

        public void Equip(WeaponData newWeapon)
        {
            if (newWeapon == null) return;

            weapon = newWeapon;
            SetAmmoFor(newWeapon);
            nextShotTime = 0f;
            // Don't let a picked-up weapon fire until the trigger is released and pressed again.
            fireHeldOnEquip = input.FireHeld;
            if (pool != null) pool.Configure(newWeapon.projectilePrefab);
            ApplyWeaponVisual(newWeapon);
        }

        void ApplyWeaponVisual(WeaponData data)
        {
            if (heldWeaponRenderer == null || data == null || data.heldSprite == null) return;

            Sprite sprite = data.heldSprite;
            heldWeaponRenderer.sprite = sprite;
            heldWeaponRenderer.transform.localPosition = new Vector3(sprite.pivot.x / sprite.pixelsPerUnit, 0f, 0f);
            if (muzzle != null) muzzle.localPosition = new Vector3(sprite.bounds.size.x, 0f, 0f);
        }

        void SetAmmoFor(WeaponData data)
        {
            if (data == null)
            {
                currentAmmo = 0;
                return;
            }

            currentAmmo = data.infiniteAmmo
                ? -1
                : Mathf.RoundToInt(data.startingAmmo * Mathf.Max(0.01f, ammoMultiplier));
        }

        void OnFirePressed()
        {
            TryFire();
        }

        void TryFire()
        {
            if (weapon == null || pool == null) return;
            if (Time.time < nextShotTime) return;

            if (!weapon.infiniteAmmo && currentAmmo <= 0)
            {
                Equip(startingWeapon);
                return;
            }

            float damage = weapon.damage * (powerUps != null ? powerUps.DamageMultiplier : 1f);
            float rate = weapon.shotsPerSecond * (powerUps != null ? powerUps.FireRateMultiplier : 1f);
            nextShotTime = Time.time + 1f / rate;
            AudioManager.PlaySfx(AudioManager.Sfx.Shot);

            for (int i = 0; i < weapon.pellets; i++)
            {
                float spread = Random.Range(-weapon.spreadDegrees, weapon.spreadDegrees);
                Quaternion rotation = muzzle.rotation * Quaternion.Euler(0f, 0f, spread);
                Projectile projectile = pool.Get(muzzle.position, rotation);
                if (projectile == null) continue;
                projectile.Initialize(rotation * Vector3.right, weapon.projectileSpeed, damage, weapon.projectileLifetime,
                    weapon.explosionRadius, weapon.burnDuration, weapon.burnDamagePerSecond);
            }

            if (!weapon.infiniteAmmo)
            {
                currentAmmo--;
                if (currentAmmo <= 0) Equip(startingWeapon);
            }
        }
    }
}
