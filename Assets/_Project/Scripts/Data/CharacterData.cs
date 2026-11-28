using UnityEngine;

namespace DeadZone.Data
{
    public enum CharacterAbility
    {
        None,
        AmmoBonus,
        Heal,
        ShieldCharge,
        Knife
    }

    [CreateAssetMenu(fileName = "CharacterData", menuName = "Dead Zone/Character Data")]
    public sealed class CharacterData : ScriptableObject
    {
        public string displayName = "Alex";
        [TextArea] public string description;
        [TextArea] public string abilityDescription;
        public int sortOrder;

        [Header("Base")]
        public PlayerConfig config;
        public Sprite bodySprite;
        public Sprite crouchSprite;
        public WeaponData startingWeapon;

        [Header("Animation (optional)")]
        [Tooltip("Body frames for idle, run, jump, crouch, hurt and death. Empty = only Body/Crouch Sprite.")]
        public CharacterAnimationSet animations;

        [Header("Ability")]
        public CharacterAbility ability;
        public float ammoMultiplier = 1f;
        public int extraJumps;
        public float healAmount = 25f;
        public float healInterval = 45f;
        public float chargeSpeed = 12f;
        public float chargeDuration = 0.35f;
        public float chargeCooldown = 1.5f;
        public float knifeDamage = 20f;
        public float knifeRange = 1.1f;
        public float knifeCooldown = 0.6f;
    }
}
