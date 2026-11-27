using UnityEngine;

namespace DeadZone.Combat
{
    public sealed class ExplosionEffect : MonoBehaviour
    {
        [SerializeField] float lifetime = 0.25f;

        float elapsed;

        void Start()
        {
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            float t = lifetime > 0f ? Mathf.Clamp01(elapsed / lifetime) : 1f;

            transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 2f, t);

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = 1f - t;
                renderer.color = color;
            }
        }
    }
}
