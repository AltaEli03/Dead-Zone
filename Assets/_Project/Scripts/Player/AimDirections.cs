using UnityEngine;

namespace DeadZone.Player
{
    public static class AimDirections
    {
        public const int DirectionCount = 8;
        public const float StepDegrees = 360f / DirectionCount;

        public static Vector2 Snap(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return Vector2.right;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float snapped = Mathf.Round(angle / StepDegrees) * StepDegrees;
            float radians = snapped * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
    }
}
