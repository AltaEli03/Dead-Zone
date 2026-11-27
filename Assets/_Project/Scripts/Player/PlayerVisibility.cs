using UnityEngine;

namespace DeadZone.Player
{
    public sealed class PlayerVisibility : MonoBehaviour
    {
        int darkZones;
        float revealedUntil;

        public bool IsHidden => darkZones > 0 && Time.time >= revealedUntil;

        public void EnterDarkness()
        {
            darkZones++;
        }

        public void ExitDarkness()
        {
            darkZones = Mathf.Max(0, darkZones - 1);
        }

        public void RevealFor(float duration)
        {
            revealedUntil = Mathf.Max(revealedUntil, Time.time + duration);
        }
    }
}
