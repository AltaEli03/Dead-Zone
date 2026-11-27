using UnityEngine;

namespace DeadZone.Combat
{
    public sealed class DamageZone : MonoBehaviour
    {
        [SerializeField, Min(1f)] float damageMultiplier = 2f;

        public float DamageMultiplier => damageMultiplier;

        public void SetMultiplier(float multiplier)
        {
            damageMultiplier = Mathf.Max(1f, multiplier);
        }
    }
}
