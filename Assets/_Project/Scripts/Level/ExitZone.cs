using UnityEngine;

namespace DeadZone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ExitZone : MonoBehaviour
    {
        [SerializeField] LevelController levelController;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<Player.PlayerMotor>() == null) return;
            if (levelController != null) levelController.TryCompleteLevel();
        }
    }
}
