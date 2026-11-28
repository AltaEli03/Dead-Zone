using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Player
{
    // Frame-by-frame body animation from a CharacterAnimationSet: idle, walk/run, jump, crouch, hurt and death.
    // Runs in LateUpdate so its sprite wins over the motor's standing/crouch swap.
    // Without a set it disables itself and the character keeps its two static sprites.
    public sealed class PlayerSpriteAnimator : MonoBehaviour
    {
        const float RunThreshold = 0.6f;     // share of move speed above which the run cycle plays
        const float MoveThreshold = 0.3f;    // horizontal speed considered "standing still"
        const float JumpTopSpeed = 2f;       // |vertical speed| shown with the top-of-jump frame
        const float ArmLinger = 0.6f;        // seconds the weapon arm stays visible after shooting

        CharacterAnimationSet set;
        SpriteRenderer body;
        SpriteRenderer weaponArm;
        PlayerMotor motor;
        PlayerHealth health;
        PlayerInputReader input;
        PlayerAbilities abilities;
        Rigidbody2D rb;
        float moveSpeed = 6.5f;
        float abilityUntil;
        float doubleJumpUntil;

        Sprite[] rise, top, fall;
        Sprite[] clip;
        bool clipLoops;
        float clipFps;
        float clipTime;
        float lastHealth;
        float hurtUntil;
        float showArmUntil;

        public bool PlaysDeath => isActiveAndEnabled && set != null && set.death != null && set.death.Length > 0;

        public void Configure(CharacterAnimationSet animations, SpriteRenderer bodyRenderer, SpriteRenderer armRenderer, PlayerConfig config)
        {
            Unsubscribe();
            set = animations;
            body = bodyRenderer;
            weaponArm = armRenderer;
            motor = GetComponent<PlayerMotor>();
            health = GetComponent<PlayerHealth>();
            input = GetComponent<PlayerInputReader>();
            abilities = GetComponent<PlayerAbilities>();
            rb = GetComponent<Rigidbody2D>();
            abilityUntil = doubleJumpUntil = 0f;
            if (motor != null) motor.AirJumped += OnAirJumped;
            if (abilities != null) abilities.AbilityUsed += OnAbilityUsed;
            if (config != null) moveSpeed = config.moveSpeed;
            lastHealth = health != null ? health.CurrentHealth : 0f;
            clip = null;

            enabled = set != null;
            if (set == null)
            {
                if (weaponArm != null) weaponArm.enabled = true;
                return;
            }

            rise = Single(set.jumpRise);
            top = Single(set.jumpTop);
            fall = Single(set.jumpFall);
            PlayerAim aim = GetComponent<PlayerAim>();
            if (aim != null) aim.SetShoulders(set.standingShoulderY, set.crouchShoulderY);
        }

        void OnDestroy()
        {
            Unsubscribe();
        }

        void Unsubscribe()
        {
            if (motor != null) motor.AirJumped -= OnAirJumped;
            if (abilities != null) abilities.AbilityUsed -= OnAbilityUsed;
        }

        void OnAbilityUsed()
        {
            if (set == null || !Has(set.ability)) return;

            bool still = (motor == null || motor.IsGrounded) && (rb == null || Mathf.Abs(rb.linearVelocity.x) <= MoveThreshold);
            if (set.abilityOnlyWhenStill && !still) return;

            abilityUntil = Time.time + set.ability.Length / Mathf.Max(0.01f, set.abilityFps);
            clip = null; // restart even if it was already playing
        }

        void OnAirJumped()
        {
            if (set == null || !Has(set.doubleJump)) return;

            doubleJumpUntil = Time.time + set.doubleJump.Length / Mathf.Max(0.01f, set.onceFps);
            clip = null;
        }

        void LateUpdate()
        {
            if (body == null) return;

            bool dead = health != null && health.IsDead;
            if (health != null)
            {
                if (!dead && health.CurrentHealth < lastHealth - 0.01f && Has(set.hurt))
                    hurtUntil = Time.time + set.hurt.Length / set.onceFps;
                lastHealth = health.CurrentHealth;
            }

            PickClip(dead);
            if (clip == null) return;

            clipTime += Time.deltaTime;
            int frame = Mathf.FloorToInt(clipTime * clipFps);
            frame = clipLoops ? frame % clip.Length : Mathf.Min(frame, clip.Length - 1);
            if (clip[frame] != null) body.sprite = clip[frame];

            UpdateWeaponArm(dead);
        }

        void PickClip(bool dead)
        {
            bool grounded = motor == null || motor.IsGrounded || motor.IsOnLadder;
            Vector2 velocity = rb != null ? rb.linearVelocity : Vector2.zero;

            if (dead && Has(set.death)) Play(set.death, set.onceFps, false);
            else if (Time.time < hurtUntil) Play(set.hurt, set.onceFps, false);
            else if (Time.time < abilityUntil) Play(set.ability, set.abilityFps, false);
            else if (Time.time < doubleJumpUntil && !grounded) Play(set.doubleJump, set.onceFps, false);
            else if (motor != null && motor.IsCrouching && Has(set.crouch)) Play(set.crouch, set.onceFps, false);
            else if (!grounded && velocity.y > JumpTopSpeed && rise != null) Play(rise, 1f, true);
            else if (!grounded && velocity.y < -JumpTopSpeed && fall != null) Play(fall, 1f, true);
            else if (!grounded && top != null) Play(top, 1f, true);
            else if (Mathf.Abs(velocity.x) > moveSpeed * RunThreshold && Has(set.run)) Play(set.run, set.runFps, true);
            else if (Mathf.Abs(velocity.x) > MoveThreshold && Has(set.walk)) Play(set.walk, set.walkFps, true);
            else if (Has(set.idle)) Play(set.idle, set.idleFps, true);
        }

        void Play(Sprite[] next, float fps, bool loops)
        {
            if (next == clip) return;
            clip = next;
            clipFps = Mathf.Max(0.01f, fps);
            clipLoops = loops;
            clipTime = 0f;
        }

        void UpdateWeaponArm(bool dead)
        {
            if (weaponArm == null || dead) return;
            if (Time.time < abilityUntil)
            {
                weaponArm.enabled = false; // the ability frames draw the whole pose
                return;
            }
            if (!set.hideWeaponArmWhenNotShooting)
            {
                weaponArm.enabled = true;
                return;
            }

            bool crouching = motor != null && motor.IsCrouching;
            bool onLadder = motor != null && motor.IsOnLadder;
            bool aimingVertically = !crouching && !onLadder && input != null && Mathf.Abs(input.Move.y) > 0.5f;
            if ((input != null && input.FireHeld) || aimingVertically) showArmUntil = Time.time + ArmLinger;
            weaponArm.enabled = Time.time < showArmUntil;
        }

        static bool Has(Sprite[] frames)
        {
            return frames != null && frames.Length > 0;
        }

        static Sprite[] Single(Sprite sprite)
        {
            return sprite != null ? new[] { sprite } : null;
        }
    }
}
