using DeadZone.Player;
using UnityEngine;

namespace DeadZone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Checkpoint : MonoBehaviour
    {
        [SerializeField] LevelController levelController;
        [SerializeField] Transform spawnPoint;
        [SerializeField] SpriteRenderer flagRenderer;
        [SerializeField] Color activeColor = new Color(0.5f, 0.95f, 0.4f);

        bool activated;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (activated) return;

            PlayerMotor playerMotor = other.GetComponentInParent<PlayerMotor>();
            if (playerMotor == null) return;

            activated = true;
            playerMotor.SetSpawnPoint(spawnPoint != null ? spawnPoint.position : transform.position);
            if (flagRenderer != null) flagRenderer.color = activeColor;
            if (levelController != null) levelController.NotifyCheckpoint();
        }
    }
}
