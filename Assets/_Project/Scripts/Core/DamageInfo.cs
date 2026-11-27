using UnityEngine;

namespace DeadZone.Core
{
    public readonly struct DamageInfo
    {
        public readonly float amount;
        public readonly Vector2 sourcePosition;
        public readonly bool bypassShield;

        public DamageInfo(float amount, Vector2 sourcePosition, bool bypassShield = false)
        {
            this.amount = amount;
            this.sourcePosition = sourcePosition;
            this.bypassShield = bypassShield;
        }
    }
}
