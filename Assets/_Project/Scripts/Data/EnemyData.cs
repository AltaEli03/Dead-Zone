using UnityEngine;

namespace DeadZone.Data
{
    public enum EnemyKind
    {
        Common,
        Runner,
        Climber,
        Spitter,
        Screamer,
        Armored,
        Nester,
        Mutant,
        Offspring
    }

    [CreateAssetMenu(fileName = "EnemyData", menuName = "Dead Zone/Enemy Data")]
    public sealed class EnemyData : ScriptableObject
    {
        public string label = "Zombi común";
        public EnemyKind kind = EnemyKind.Common;

        [Header("Combat")]
        public float maxHealth = 30f;
        public float moveSpeed = 2.2f;
        public float attackDamage = 10f;
        public float attackRange = 1.1f;
        public float attackCooldown = 0.8f;
        public float headshotMultiplier = 2f;
        public float knockbackForce = 6f;
        public float knockbackUp = 2f;
        public float activationRange = 20f;

        [Header("Movement")]
        public float ambushRange = 6f;
        public float jumpVelocity = 8f;
        public float climbSpeed = 3f;
        public float turnDelay = 0.8f;
        public float erraticInterval = 0.6f;

        [Header("Spitter")]
        public float preferredRange = 6f;
        public float projectileSpeed = 9f;
        public float projectileGravity = 3f;
        public float acidDamage = 8f;
        public float acidDuration = 3f;
        public float acidRadius = 0.9f;

        [Header("Screamer")]
        public float screamRadius = 9f;
        public float screamCooldown = 6f;
        public int summonCount = 2;

        [Header("Nester")]
        public float regenPerSecond = 6f;
        public float regenRadius = 3.5f;
        public int offspringCount = 3;

        [Header("Armored / Mutant")]
        public float shieldBlockAngle = 90f;
        public float alertRadius = 6f;
    }
}
