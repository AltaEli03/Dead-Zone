using DeadZone.Data;
using DeadZone.Level;
using UnityEngine;

namespace DeadZone.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(PlayerInputReader))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] PlayerConfig config;
        [SerializeField] Transform groundCheck;
        [SerializeField] LayerMask groundMask;
        [SerializeField] PlayerAim aim;
        [SerializeField] SpriteRenderer visualRenderer;
        [SerializeField] Sprite crouchSprite;

        const float GroundCheckRadius = 0.12f;
        const float CrouchInputThreshold = -0.5f;

        Rigidbody2D body;
        CapsuleCollider2D bodyCollider;
        PlayerInputReader input;
        PlayerPowerUps powerUps;
        Ladder currentLadder;
        Sprite standingSprite;
        Vector2 standingColliderSize;
        Vector2 standingColliderOffset;
        Vector2 spawnPoint;
        float coyoteTimer;
        float jumpBufferTimer;
        float dodgeEndTime;
        float dodgeReadyTime;
        float dodgeDirection;
        float knockbackEndTime;
        Vector2 knockbackVelocity;
        float abilityDashEndTime;
        float abilityDashSpeed;
        float abilityDashDirection;
        int extraJumps;
        int airJumpsLeft;

        public bool IsGrounded { get; private set; }
        public bool IsDodging => Time.time < dodgeEndTime;
        public bool IsKnockedBack => Time.time < knockbackEndTime;
        public bool IsAbilityDashing => Time.time < abilityDashEndTime;
        public bool IsCrouching { get; private set; }
        public bool IsSliding { get; private set; }
        public bool IsOnLadder => currentLadder != null;
        public float SlideDirection => dodgeDirection;
        public SpriteRenderer VisualRenderer => visualRenderer;

        public event System.Action AirJumped;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<CapsuleCollider2D>();
            input = GetComponent<PlayerInputReader>();
            powerUps = GetComponent<PlayerPowerUps>();
            spawnPoint = transform.position;
            body.gravityScale = config.gravityScale;

            standingSprite = visualRenderer.sprite;
            standingColliderSize = bodyCollider.size;
            standingColliderOffset = bodyCollider.offset;
        }

        void OnEnable()
        {
            input.JumpPressed += OnJumpPressed;
        }

        void OnDisable()
        {
            input.JumpPressed -= OnJumpPressed;
        }

        void Update()
        {
            if (IsSliding && Time.time >= dodgeEndTime) IsSliding = false;

            IsGrounded = CheckGround();
            if (IsGrounded) airJumpsLeft = extraJumps;

            bool wantCrouch = !IsOnLadder && (IsSliding || (IsGrounded && !IsDodging && input.Move.y < CrouchInputThreshold));
            if (wantCrouch != IsCrouching) SetCrouching(wantCrouch);

            coyoteTimer = IsGrounded ? config.coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
            jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);

            if (IsOnLadder && jumpBufferTimer > 0f)
            {
                jumpBufferTimer = 0f;
                ClearLadder(currentLadder);
                body.linearVelocity = new Vector2(0f, config.jumpVelocity);
            }

            if (jumpBufferTimer > 0f && coyoteTimer > 0f && !IsDodging)
            {
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;

                if (IsCrouching)
                {
                    StartDodge();
                }
                else
                {
                    body.linearVelocity = new Vector2(body.linearVelocity.x, config.jumpVelocity);
                }
            }

            if (jumpBufferTimer > 0f && coyoteTimer <= 0f && !IsGrounded && airJumpsLeft > 0 && !IsDodging && !IsOnLadder)
            {
                jumpBufferTimer = 0f;
                airJumpsLeft--;
                body.linearVelocity = new Vector2(body.linearVelocity.x, config.jumpVelocity);
                AirJumped?.Invoke();
            }

            if (transform.position.y < config.fallResetY) Respawn();
        }

        void FixedUpdate()
        {
            if (IsDodging)
            {
                body.linearVelocity = new Vector2(dodgeDirection * config.dodgeSpeed, body.linearVelocity.y);
                return;
            }

            if (IsKnockedBack)
            {
                body.linearVelocity = new Vector2(knockbackVelocity.x, body.linearVelocity.y);
                return;
            }

            if (IsAbilityDashing)
            {
                body.linearVelocity = new Vector2(abilityDashDirection * abilityDashSpeed, body.linearVelocity.y);
                return;
            }

            if (IsOnLadder)
            {
                body.linearVelocity = new Vector2(0f, input.Move.y * config.ladderClimbSpeed);
                return;
            }

            float moveSpeed = config.moveSpeed * (powerUps != null ? powerUps.SpeedMultiplier : 1f);
            float targetSpeed = IsCrouching ? 0f : input.Move.x * moveSpeed;
            float acceleration = CheckGround() ? config.groundAcceleration : config.airAcceleration;
            float nextX = Mathf.MoveTowards(body.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
            body.linearVelocity = new Vector2(nextX, body.linearVelocity.y);
        }

        bool CheckGround()
        {
            return Physics2D.OverlapCircle(groundCheck.position, GroundCheckRadius, groundMask);
        }

        void OnJumpPressed()
        {
            jumpBufferTimer = config.jumpBufferTime;
        }

        public void Configure(PlayerConfig newConfig, Sprite standing, Sprite crouch, int newExtraJumps)
        {
            if (newConfig != null)
            {
                config = newConfig;
                body.gravityScale = config.gravityScale;
            }

            if (standing != null) standingSprite = standing;
            if (crouch != null) crouchSprite = crouch;

            extraJumps = Mathf.Max(0, newExtraJumps);
            airJumpsLeft = extraJumps;
            abilityDashEndTime = 0f;
            SetCrouching(false);
        }

        public void StartAbilityDash(float speed, float duration, float direction)
        {
            abilityDashSpeed = speed;
            abilityDashDirection = direction < 0f ? -1f : 1f;
            abilityDashEndTime = Time.time + duration;
        }

        public void ApplyKnockback(Vector2 force)
        {
            knockbackVelocity = force;
            knockbackEndTime = Time.time + config.knockbackDuration;
            body.linearVelocity = new Vector2(force.x, force.y);
        }

        public void SetSpawnPoint(Vector3 position)
        {
            spawnPoint = position;
        }

        public void SetLadder(Ladder ladder)
        {
            if (ladder == null || currentLadder == ladder) return;

            currentLadder = ladder;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
        }

        public void ClearLadder(Ladder ladder)
        {
            if (currentLadder != ladder) return;

            currentLadder = null;
            body.gravityScale = config.gravityScale;
        }

        void StartDodge()
        {
            if (Time.time < dodgeReadyTime || IsDodging) return;

            IsSliding = true;
            dodgeDirection = Mathf.Abs(input.Move.x) > 0.01f
                ? Mathf.Sign(input.Move.x)
                : aim != null ? aim.Facing : 1f;

            dodgeEndTime = Time.time + config.dodgeDuration;
            dodgeReadyTime = dodgeEndTime + config.dodgeCooldown;
            SetCrouching(true);
        }

        void SetCrouching(bool crouching)
        {
            IsCrouching = crouching;
            bodyCollider.size = crouching
                ? new Vector2(standingColliderSize.x, config.crouchColliderHeight)
                : standingColliderSize;
            bodyCollider.offset = crouching
                ? new Vector2(standingColliderOffset.x, config.crouchColliderOffsetY)
                : standingColliderOffset;
            visualRenderer.sprite = crouching ? crouchSprite : standingSprite;
            // shortcut: no headroom check when standing; add when levels have low ceilings.
        }

        public void Respawn()
        {
            IsSliding = false;
            knockbackEndTime = 0f;
            knockbackVelocity = Vector2.zero;
            abilityDashEndTime = 0f;
            airJumpsLeft = extraJumps;
            currentLadder = null;
            body.gravityScale = config.gravityScale;
            if (IsCrouching) SetCrouching(false);
            transform.position = spawnPoint;
            body.linearVelocity = Vector2.zero;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
        }

        void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, GroundCheckRadius);
        }
    }
}
