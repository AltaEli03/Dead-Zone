using UnityEngine;

namespace DeadZone.Data
{
    public enum PowerUpType
    {
        Damage,
        Speed,
        FireRate
    }

    [CreateAssetMenu(fileName = "PowerUpData", menuName = "Dead Zone/Power Up Data")]
    public sealed class PowerUpData : ScriptableObject
    {
        public PowerUpType type = PowerUpType.Damage;
        public float multiplier = 1.5f;
        public float duration = 10f;
        public string label = "DAÑO";
        public Color color = Color.white;
    }
}
