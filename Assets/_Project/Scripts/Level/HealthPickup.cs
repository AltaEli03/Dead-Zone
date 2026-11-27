using DeadZone.Player;
using UnityEngine;

namespace DeadZone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class HealthPickup : MonoBehaviour
    {
        [SerializeField] float healAmount = 35f;

        void OnTriggerEnter2D(Collider2D other)
        {
            PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
            if (health == null) return;

            health.Heal(healAmount);
            gameObject.SetActive(false);
        }
    }
}
