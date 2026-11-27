using System.Collections.Generic;
using DeadZone.Core;
using UnityEngine;

namespace DeadZone.Combat
{
    public sealed class FireZone : MonoBehaviour
    {
        [SerializeField] float length = 2.5f;
        [SerializeField] float height = 0.8f;
        [SerializeField] float duration = 2f;
        [SerializeField] float spreadDuration = 0.3f;
        [SerializeField] float damagePerSecond = 10f;
        [SerializeField] LayerMask damageMask;

        SpriteRenderer visual;
        Vector3 origin;
        float startTime;
        float endTime;
        float nextTick;

        public void Configure(float newDuration, float damagePerSecondValue, float newLength, float newHeight, float newSpreadDuration)
        {
            duration = newDuration;
            damagePerSecond = damagePerSecondValue;
            length = newLength;
            height = newHeight;
            spreadDuration = newSpreadDuration;
        }

        void Awake()
        {
            visual = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            origin = transform.position;
            startTime = Time.time;
            endTime = startTime + duration;
            nextTick = startTime;
            ApplyShape(Mathf.Min(0.2f, length));
        }

        void Update()
        {
            if (Time.time >= endTime)
            {
                Destroy(gameObject);
                return;
            }

            float progress = spreadDuration > 0f ? Mathf.Clamp01((Time.time - startTime) / spreadDuration) : 1f;
            float currentLength = Mathf.Lerp(0.2f, length, progress);
            ApplyShape(currentLength);

            if (Time.time < nextTick) return;

            nextTick = Time.time + 0.5f;
            Vector2 center = new Vector2(origin.x + currentLength * 0.5f, origin.y);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(currentLength, height), 0f, damageMask);
            HashSet<IDamageable> damaged = new HashSet<IDamageable>();

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || damaged.Contains(damageable)) continue;

                damaged.Add(damageable);
                damageable.TakeDamage(new DamageInfo(damagePerSecond * 0.5f, transform.position, true));
            }
        }

        void ApplyShape(float currentLength)
        {
            transform.position = new Vector3(origin.x + currentLength * 0.5f, origin.y, origin.z);

            Vector2 spriteSize = visual != null && visual.sprite != null
                ? (Vector2)visual.sprite.bounds.size
                : Vector2.one;
            transform.localScale = new Vector3(
                currentLength / Mathf.Max(spriteSize.x, 0.01f),
                height / Mathf.Max(spriteSize.y, 0.01f),
                1f);
        }
    }
}
