using UnityEngine;

namespace DeadZone.Player
{
    [DefaultExecutionOrder(10)]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] PlayerInputReader input;
        [SerializeField] Transform weaponPivot;
        [SerializeField] Transform visualRoot;
        [SerializeField] SpriteRenderer weaponRenderer;
        [SerializeField, Range(0f, 1f)] float inputDeadzone = 0.2f;
        [SerializeField] float standingShoulderY = 0.81f;
        [SerializeField] float crouchShoulderY = 0.52f;

        public float Facing { get; private set; } = 1f;

        PlayerMotor motor;

        void Awake()
        {
            motor = GetComponent<PlayerMotor>();
        }

        public void SetShoulders(float standing, float crouch)
        {
            if (standing > 0f) standingShoulderY = standing;
            if (crouch > 0f) crouchShoulderY = crouch;
        }

        void Update()
        {
            bool crouching = motor != null && motor.IsCrouching;
            bool onLadder = motor != null && motor.IsOnLadder;
            Vector2 raw = input.Move;
            if (crouching)
            {
                float horizontal = motor != null && motor.IsSliding
                    ? motor.SlideDirection
                    : Mathf.Abs(raw.x) > inputDeadzone ? raw.x : Facing;
                raw = new Vector2(horizontal, 0f);
            }
            else if (onLadder)
            {
                float horizontal = Mathf.Abs(raw.x) > inputDeadzone ? raw.x : Facing;
                raw = new Vector2(horizontal, 0f);
            }
            else if (raw.magnitude < inputDeadzone)
            {
                raw = new Vector2(Facing, 0f);
            }

            Vector2 snapped = AimDirections.Snap(raw);
            float angle = Mathf.Atan2(snapped.y, snapped.x) * Mathf.Rad2Deg;
            weaponPivot.localRotation = Quaternion.Euler(0f, 0f, angle);

            if (Mathf.Abs(snapped.x) > 0.01f)
            {
                Facing = Mathf.Sign(snapped.x);
                Vector3 scale = visualRoot.localScale;
                visualRoot.localScale = new Vector3(Mathf.Abs(scale.x) * Facing, scale.y, scale.z);
            }

            float shoulderY = crouching ? crouchShoulderY : standingShoulderY;
            Vector3 pivotPosition = weaponPivot.localPosition;
            weaponPivot.localPosition = new Vector3(Mathf.Abs(pivotPosition.x) * Facing, shoulderY, pivotPosition.z);

            weaponRenderer.flipY = Facing < 0f;
        }
    }
}
