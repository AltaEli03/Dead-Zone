using DeadZone.Data;
using DeadZone.Player;
using UnityEngine;

namespace DeadZone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class WeaponPickup : MonoBehaviour
    {
        [SerializeField] WeaponData weapon;
        [SerializeField] LevelController levelController;

        void OnTriggerEnter2D(Collider2D other)
        {
            PlayerShooter shooter = other.GetComponentInParent<PlayerShooter>();
            if (shooter == null) return;

            shooter.Equip(weapon);
            if (levelController != null) levelController.NotifyShotgunPicked();
            gameObject.SetActive(false);
        }
    }
}
