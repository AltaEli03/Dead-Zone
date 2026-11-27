using DeadZone.Core;
using UnityEngine;

namespace DeadZone.Enemies
{
    public sealed class Shield : MonoBehaviour
    {
        [SerializeField] SpriteRenderer shieldVisual;
        [SerializeField] float blockAngle = 90f;
        [SerializeField] float localX = 0.55f;
        [SerializeField] float localY = 0.55f;

        float facing = 1f;
        float flashUntil;
        Color baseColor;

        void Awake()
        {
            if (shieldVisual == null) shieldVisual = GetComponentInChildren<SpriteRenderer>();
            if (shieldVisual != null) baseColor = shieldVisual.color;
        }

        void Update()
        {
            if (shieldVisual == null) return;
            shieldVisual.color = Time.time < flashUntil ? Color.white : baseColor;
        }

        public void SetBlockAngle(float angle)
        {
            blockAngle = angle;
        }

        public void SetFacing(float direction)
        {
            facing = direction < 0f ? -1f : 1f;
            if (shieldVisual == null) return;

            shieldVisual.flipX = facing < 0f;
            Vector3 position = shieldVisual.transform.localPosition;
            shieldVisual.transform.localPosition = new Vector3(Mathf.Abs(localX) * facing, localY, position.z);
        }

        public bool Blocks(DamageInfo info)
        {
            if (info.bypassShield) return false;

            Vector2 toSource = info.sourcePosition - (Vector2)transform.position;
            if (toSource.sqrMagnitude < 0.0001f) return false;

            float dot = Vector2.Dot(toSource.normalized, new Vector2(facing, 0f));
            return dot >= Mathf.Cos(blockAngle * 0.5f * Mathf.Deg2Rad);
        }

        public void Flash()
        {
            flashUntil = Time.time + 0.08f;
        }
    }
}
