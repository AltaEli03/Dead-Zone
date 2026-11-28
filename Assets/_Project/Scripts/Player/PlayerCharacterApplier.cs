using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Player
{
    public static class PlayerCharacterApplier
    {
        public static void Apply(PlayerHealth health, CharacterData data)
        {
            if (health == null || data == null) return;

            GameObject player = health.gameObject;
            PlayerMotor motor = player.GetComponent<PlayerMotor>();
            PlayerShooter shooter = player.GetComponent<PlayerShooter>();
            PlayerAbilities abilities = player.GetComponent<PlayerAbilities>();

            if (motor != null) motor.Configure(data.config, data.bodySprite, data.crouchSprite, data.extraJumps);
            if (shooter != null) shooter.Configure(data.startingWeapon, data.ammoMultiplier);
            health.Configure(data.config);

            if (abilities == null) abilities = player.AddComponent<PlayerAbilities>();
            abilities.Configure(data);

            PlayerSpriteAnimator animator = player.GetComponent<PlayerSpriteAnimator>();
            if (data.animations != null || animator != null)
            {
                if (animator == null) animator = player.AddComponent<PlayerSpriteAnimator>();
                animator.Configure(data.animations,
                    motor != null ? motor.VisualRenderer : null,
                    shooter != null ? shooter.HeldWeaponRenderer : null,
                    data.config);
            }

            PlayerSelection.Selected = data;
        }
    }
}
