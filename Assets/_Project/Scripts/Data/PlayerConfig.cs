using UnityEngine;

namespace DeadZone.Data
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Dead Zone/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 6.5f;
        public float groundAcceleration = 90f;
        public float airAcceleration = 45f;

        [Header("Jump")]
        public float jumpVelocity = 11f;
        public float gravityScale = 3f;
        public float coyoteTime = 0.12f;
        public float jumpBufferTime = 0.12f;

        [Header("Dodge")]
        public float dodgeSpeed = 16f;
        public float dodgeDuration = 0.16f;
        public float dodgeCooldown = 0.7f;

        [Header("Ladders")]
        public float ladderClimbSpeed = 4f;

        [Header("Crouch")]
        public float crouchColliderHeight = 0.9f;
        public float crouchColliderOffsetY = 0.45f;

        [Header("Secondary Weapons")]
        public int maxActiveThrows = 2;

        [Header("Health")]
        public float maxHealth = 100f;
        public int lives = 3;
        public float invulnerabilityTime = 0.5f;
        public float respawnDelay = 1.5f;
        public float respawnInvulnerability = 1.5f;
        public float knockbackDuration = 0.2f;

        [Header("Safety")]
        public float fallResetY = -12f;
    }
}
