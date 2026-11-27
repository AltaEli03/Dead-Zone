using DeadZone.Data;
using DeadZone.Player;
using UnityEngine;

namespace DeadZone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PowerUpPickup : MonoBehaviour
    {
        [SerializeField] PowerUpData data;

        void OnTriggerEnter2D(Collider2D other)
        {
            PlayerPowerUps powerUps = other.GetComponentInParent<PlayerPowerUps>();
            if (powerUps == null) return;

            powerUps.Apply(data);
            gameObject.SetActive(false);
        }
    }
}
