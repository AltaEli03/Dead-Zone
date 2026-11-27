using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Player
{
    public sealed class PlayerPowerUps : MonoBehaviour
    {
        float damageUntil;
        float speedUntil;
        float fireRateUntil;
        float damageMultiplier = 1f;
        float speedMultiplier = 1f;
        float fireRateMultiplier = 1f;
        string damageLabel = string.Empty;
        string speedLabel = string.Empty;
        string fireRateLabel = string.Empty;

        public float DamageMultiplier => Time.time < damageUntil ? damageMultiplier : 1f;
        public float SpeedMultiplier => Time.time < speedUntil ? speedMultiplier : 1f;
        public float FireRateMultiplier => Time.time < fireRateUntil ? fireRateMultiplier : 1f;
        public string ActiveText { get; private set; } = string.Empty;
        public float ActiveTime { get; private set; }

        public void Apply(PowerUpData data)
        {
            if (data == null) return;

            switch (data.type)
            {
                case PowerUpType.Damage:
                    damageUntil = Time.time + data.duration;
                    damageMultiplier = data.multiplier;
                    damageLabel = data.label;
                    break;
                case PowerUpType.Speed:
                    speedUntil = Time.time + data.duration;
                    speedMultiplier = data.multiplier;
                    speedLabel = data.label;
                    break;
                case PowerUpType.FireRate:
                    fireRateUntil = Time.time + data.duration;
                    fireRateMultiplier = data.multiplier;
                    fireRateLabel = data.label;
                    break;
            }

            UpdateText();
        }

        void Update()
        {
            UpdateText();
        }

        void UpdateText()
        {
            string text = string.Empty;
            float remaining = 0f;

            if (Time.time < damageUntil)
            {
                float time = damageUntil - Time.time;
                text += damageLabel + " " + Mathf.CeilToInt(time) + "s  ";
                remaining = Mathf.Max(remaining, time);
            }

            if (Time.time < speedUntil)
            {
                float time = speedUntil - Time.time;
                text += speedLabel + " " + Mathf.CeilToInt(time) + "s  ";
                remaining = Mathf.Max(remaining, time);
            }

            if (Time.time < fireRateUntil)
            {
                float time = fireRateUntil - Time.time;
                text += fireRateLabel + " " + Mathf.CeilToInt(time) + "s  ";
                remaining = Mathf.Max(remaining, time);
            }

            ActiveText = text.TrimEnd();
            ActiveTime = remaining;
        }
    }
}
