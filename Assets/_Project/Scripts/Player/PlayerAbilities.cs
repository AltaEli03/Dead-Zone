using System.Collections;
using System.Collections.Generic;
using DeadZone.Core;
using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Player
{
    public sealed class PlayerAbilities : MonoBehaviour
    {
        PlayerInputReader input;
        PlayerHealth health;
        PlayerMotor motor;
        PlayerAim aim;
        CharacterData data;
        LayerMask enemyMask;
        float nextAbilityTime;
        SpriteRenderer shieldVisual;
        Coroutine healRoutine;

        void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            health = GetComponent<PlayerHealth>();
            motor = GetComponent<PlayerMotor>();
            aim = GetComponent<PlayerAim>();

            int enemyLayer = LayerMask.NameToLayer("Enemy");
            enemyMask = enemyLayer >= 0 ? 1 << enemyLayer : 0;
        }

        void OnEnable()
        {
            if (input != null) input.AbilityPressed += OnAbilityPressed;
        }

        void OnDisable()
        {
            if (input != null) input.AbilityPressed -= OnAbilityPressed;
            StopHealing();
            if (shieldVisual != null) shieldVisual.enabled = false;
        }

        public void Configure(CharacterData character)
        {
            data = character;
            if (data == null) return;

            StopHealing();
            nextAbilityTime = 0f;
            if (shieldVisual != null) shieldVisual.enabled = false;

            if (data.ability == CharacterAbility.Heal && health != null)
                healRoutine = StartCoroutine(HealRoutine());

            if (data.ability == CharacterAbility.ShieldCharge && shieldVisual == null)
                CreateShield();
        }

        void OnAbilityPressed()
        {
            if (data == null || health == null || health.IsDead || Time.time < nextAbilityTime) return;

            if (data.ability == CharacterAbility.ShieldCharge) StartShieldCharge();
            else if (data.ability == CharacterAbility.Knife) StartKnife();
        }

        IEnumerator HealRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(data.healInterval);
                if (health != null) health.Heal(data.healAmount);
            }
        }

        void StopHealing()
        {
            if (healRoutine == null) return;
            StopCoroutine(healRoutine);
            healRoutine = null;
        }

        void StartShieldCharge()
        {
            if (motor == null || data == null) return;

            nextAbilityTime = Time.time + data.chargeCooldown;
            motor.StartAbilityDash(data.chargeSpeed, data.chargeDuration, aim != null ? aim.Facing : 1f);
            if (health != null) health.GrantInvulnerability(data.chargeDuration + 0.05f);
            if (shieldVisual != null) StartCoroutine(ShowShield(data.chargeDuration));
        }

        IEnumerator ShowShield(float duration)
        {
            if (shieldVisual == null) yield break;

            float facing = aim != null ? aim.Facing : 1f;
            Vector3 position = shieldVisual.transform.localPosition;
            shieldVisual.transform.localPosition = new Vector3(Mathf.Abs(position.x) * facing, position.y, position.z);
            shieldVisual.flipX = facing < 0f;
            shieldVisual.enabled = true;

            yield return new WaitForSeconds(duration);
            if (shieldVisual != null) shieldVisual.enabled = false;
        }

        void StartKnife()
        {
            if (data == null) return;

            nextAbilityTime = Time.time + data.knifeCooldown;
            float facing = aim != null ? aim.Facing : 1f;
            Vector2 center = (Vector2)transform.position + Vector2.right * facing * 0.7f + Vector2.up * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, data.knifeRange, enemyMask);
            HashSet<IDamageable> damaged = new HashSet<IDamageable>();

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || damaged.Contains(damageable)) continue;

                damaged.Add(damageable);
                damageable.TakeDamage(new DamageInfo(data.knifeDamage, transform.position));
            }
        }

        void CreateShield()
        {
            GameObject shield = new GameObject("AbilityShield");
            shield.transform.SetParent(transform, false);
            shield.transform.localPosition = new Vector3(0.55f, 0.55f, 0f);

            shieldVisual = shield.AddComponent<SpriteRenderer>();
            shieldVisual.sprite = Resources.Load<Sprite>("Characters/Shield");
            shieldVisual.sortingOrder = 22;
            shieldVisual.enabled = false;
        }
    }
}
