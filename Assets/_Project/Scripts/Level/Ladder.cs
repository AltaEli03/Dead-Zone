using DeadZone.Player;
using UnityEngine;

namespace DeadZone.Level
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Ladder : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            PlayerMotor motor = other.GetComponentInParent<PlayerMotor>();
            if (motor != null) motor.SetLadder(this);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            PlayerMotor motor = other.GetComponentInParent<PlayerMotor>();
            if (motor != null) motor.ClearLadder(this);
        }
    }
}
