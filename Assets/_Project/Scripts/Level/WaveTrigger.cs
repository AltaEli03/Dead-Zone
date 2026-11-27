using UnityEngine;

namespace DeadZone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class WaveTrigger : MonoBehaviour
    {
        [SerializeField] LevelController levelController;
        bool triggered;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered) return;
            if (other.GetComponentInParent<Player.PlayerMotor>() == null) return;

            triggered = true;
            if (levelController != null) levelController.StartFinalWave();
            gameObject.SetActive(false);
        }
    }
}
