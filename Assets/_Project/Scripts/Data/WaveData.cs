using UnityEngine;

namespace DeadZone.Data
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "Dead Zone/Wave Data")]
    public sealed class WaveData : ScriptableObject
    {
        public int zombieCount = 5;
    }
}
