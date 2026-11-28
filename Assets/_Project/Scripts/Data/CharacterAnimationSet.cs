using UnityEngine;

namespace DeadZone.Data
{
    // Body frames for one character. All frames share the same canvas, with the feet at the bottom-center pivot.
    [CreateAssetMenu(fileName = "Animations", menuName = "Dead Zone/Character Animations")]
    public sealed class CharacterAnimationSet : ScriptableObject
    {
        [Header("Loops")]
        public Sprite[] idle;
        public float idleFps = 6f;
        public Sprite[] walk;
        public float walkFps = 10f;
        public Sprite[] run;
        public float runFps = 12f;

        [Header("Jump (one frame each, picked by vertical speed)")]
        public Sprite jumpRise;
        public Sprite jumpTop;
        public Sprite jumpFall;

        [Header("Play once")]
        [Tooltip("Played when crouching starts; the last frame is held while crouched.")]
        public Sprite[] crouch;
        public Sprite[] hurt;
        [Tooltip("The last frame is held until the player respawns.")]
        public Sprite[] death;
        public float onceFps = 12f;

        [Header("Abilities (optional)")]
        [Tooltip("Played once when the character's ability fires: shield charge, knife or heal.")]
        public Sprite[] ability;
        public float abilityFps = 12f;
        [Tooltip("Only play the ability frames while standing still (for passive abilities like healing).")]
        public bool abilityOnlyWhenStill;
        [Tooltip("Played once on a jump in the air.")]
        public Sprite[] doubleJump;

        [Header("Weapon arm")]
        [Tooltip("The body frames already draw the arms: hide the separate arm+weapon unless the player shoots or aims up/down.")]
        public bool hideWeaponArmWhenNotShooting = true;
        public float standingShoulderY = 0.81f;
        public float crouchShoulderY = 0.52f;
    }
}
