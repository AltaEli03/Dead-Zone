using DeadZone.Core;
using DeadZone.Player;
using UnityEngine;

namespace DeadZone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class SecondaryPickup : MonoBehaviour
    {
        [SerializeField] bool molotov;
        [SerializeField] int amount = 3;

        void OnTriggerEnter2D(Collider2D other)
        {
            PlayerThrower thrower = other.GetComponentInParent<PlayerThrower>();
            if (thrower == null) return;

            thrower.AddSecondary(molotov, amount);
            AudioManager.PlaySfx(AudioManager.Sfx.Pickup);
            gameObject.SetActive(false);
        }
    }
}
