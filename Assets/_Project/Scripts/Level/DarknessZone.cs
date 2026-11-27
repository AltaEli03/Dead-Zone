using DeadZone.Player;
using UnityEngine;

namespace DeadZone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class DarknessZone : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            PlayerVisibility visibility = other.GetComponentInParent<PlayerVisibility>();
            if (visibility != null) visibility.EnterDarkness();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            PlayerVisibility visibility = other.GetComponentInParent<PlayerVisibility>();
            if (visibility != null) visibility.ExitDarkness();
        }
    }
}
