using DeadZone.Player;
using UnityEngine;

namespace DeadZone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ObjectivePoint : MonoBehaviour
    {
        [SerializeField] LevelController levelController;
        [SerializeField] SpriteRenderer visual;
        [SerializeField] Color activeColor = new Color(0.5f, 0.95f, 0.4f);

        bool activated;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (activated) return;
            if (other.GetComponentInParent<PlayerMotor>() == null) return;

            activated = true;
            if (levelController != null) levelController.NotifyObjectiveComplete();
            if (visual != null) visual.color = activeColor;
        }
    }
}
