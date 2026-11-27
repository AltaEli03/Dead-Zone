using DeadZone.Enemies;
using DeadZone.Level;
using DeadZone.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadZone.UI
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] PlayerHealth playerHealth;
        [SerializeField] PlayerShooter shooter;
        [SerializeField] PlayerThrower thrower;
        [SerializeField] PlayerPowerUps powerUps;
        [SerializeField] ZombieSpawner spawner;
        [SerializeField] LevelController levelController;
        [SerializeField] Image healthFill;
        [SerializeField] TMP_Text healthText;
        [SerializeField] TMP_Text livesText;
        [SerializeField] TMP_Text weaponText;
        [SerializeField] TMP_Text enemiesText;
        [SerializeField] TMP_Text objectiveText;
        [SerializeField] TMP_Text controlsText;
        [SerializeField] TMP_Text secondaryText;
        [SerializeField] TMP_Text powerUpText;
        [SerializeField] TMP_Text centerMessage;

        void Update()
        {
            if (playerHealth == null || spawner == null) return;

            float max = playerHealth.MaxHealth;
            float current = Mathf.Clamp(playerHealth.CurrentHealth, 0f, max);
            float ratio = max > 0f ? current / max : 0f;

            healthFill.fillAmount = ratio;
            healthFill.color = ratio > 0.5f
                ? new Color(0.35f, 0.8f, 0.35f)
                : ratio > 0.25f ? new Color(0.95f, 0.75f, 0.2f) : new Color(0.85f, 0.25f, 0.2f);

            healthText.text = Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
            livesText.text = "VIDAS: " + playerHealth.Lives;
            enemiesText.text = "ENEMIGOS: " + EnemyHealth.ActiveCount;
            weaponText.text = shooter != null
                ? shooter.WeaponName + (shooter.HasInfiniteAmmo ? " ∞" : " " + shooter.CurrentAmmo)
                : "PISTOLA ∞";

            if (secondaryText != null)
                secondaryText.text = thrower != null ? "SECUNDARIA: " + thrower.SecondaryText : string.Empty;
            if (powerUpText != null)
                powerUpText.text = powerUps != null ? powerUps.ActiveText : string.Empty;

            if (objectiveText != null)
                objectiveText.text = levelController != null ? levelController.ObjectiveText : string.Empty;
            if (controlsText != null)
                controlsText.text = levelController != null && levelController.ShowTutorial ? levelController.ControlsText : string.Empty;

            if (levelController != null && levelController.LevelComplete)
            {
                centerMessage.text = levelController.CompletionMessage + "\nCargando siguiente nivel...";
                centerMessage.color = new Color(0.5f, 0.95f, 0.4f);
            }
            else if (levelController != null && !string.IsNullOrEmpty(levelController.StatusMessage))
            {
                centerMessage.text = levelController.StatusMessage;
                centerMessage.color = new Color(0.95f, 0.85f, 0.35f);
            }
            else if (levelController == null && spawner.WaveCleared)
            {
                centerMessage.text = "¡ZONA DESPEJADA!\nPulsa R o Start para reiniciar";
                centerMessage.color = new Color(0.5f, 0.95f, 0.4f);
            }
            else if (playerHealth.IsDead)
            {
                centerMessage.text = "HAS CAÍDO...";
                centerMessage.color = new Color(0.95f, 0.65f, 0.2f);
            }
            else
            {
                centerMessage.text = string.Empty;
            }
        }
    }
}
