using UnityEngine;

namespace DeadZone.Data
{
    public enum ThrowableKind
    {
        Grenade,
        Molotov
    }

    [CreateAssetMenu(fileName = "ThrowableData", menuName = "Dead Zone/Throwable Data")]
    public sealed class ThrowableData : ScriptableObject
    {
        public string label = "Granada";
        public ThrowableKind kind = ThrowableKind.Grenade;
        public float damage = 80f;
        public float radius = 2.5f;
        public float fuseTime = 1.2f;
        public float fireDuration = 2f;
        public float fireDamagePerSecond = 10f;
        public float fireLength = 2.5f;
        public float fireHeight = 0.8f;
        public float fireSpreadDuration = 0.3f;
        public float throwSpeedX = 8f;
        public float throwSpeedY = 6f;
    }
}
