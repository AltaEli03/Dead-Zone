using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadZone.UI
{
    // Melting acid look for a TMP title: letter bottoms sag, drips grow from them and drops fall off.
    [RequireComponent(typeof(TMP_Text))]
    public sealed class AcidDripText : MonoBehaviour
    {
        [SerializeField] Color acidColor = new Color(0.45f, 1f, 0.15f);
        [SerializeField] float sag = 8f;
        [SerializeField] int maxDrips = 12;
        [SerializeField] Vector2 dripLength = new Vector2(18f, 55f);
        [SerializeField] Vector2 dripWidth = new Vector2(5f, 10f);
        [SerializeField] Vector2 growTime = new Vector2(1.2f, 2.8f);
        [SerializeField] Vector2 spawnDelay = new Vector2(0.25f, 0.9f);
        [SerializeField] float glowPulse = 0.15f;

        sealed class Drip
        {
            public RectTransform stem;
            public RectTransform drop;
            public Image dropImage;
            public Vector2 origin;
            public float length, width, grow, timer, dropY, dropSpeed;
            public bool growing, falling;
        }

        const float Gravity = 900f;
        const float FallTime = 0.7f;

        static Sprite circle;

        TMP_Text text;
        Material material;
        float baseDilate;
        Drip[] drips;
        float[] sagLeft;
        float[] sagRight;
        float nextSpawn;

        void Start()
        {
            text = GetComponent<TMP_Text>();
            material = text.fontSharedMaterial;
            if (material != null && material.HasProperty(ShaderUtilities.ID_UnderlayDilate))
                baseDilate = material.GetFloat(ShaderUtilities.ID_UnderlayDilate);

            sagLeft = new float[64];
            sagRight = new float[64];
            for (int i = 0; i < sagLeft.Length; i++)
            {
                sagLeft[i] = Random.Range(0.2f, 1f);
                sagRight[i] = Random.Range(0.2f, 1f);
            }

            var container = new GameObject("Drips", typeof(RectTransform)).GetComponent<RectTransform>();
            container.SetParent(transform, false);
            container.localPosition = Vector3.zero;
            container.sizeDelta = Vector2.zero;

            drips = new Drip[maxDrips];
            for (int i = 0; i < drips.Length; i++)
                drips[i] = CreateDrip(container);
        }

        Drip CreateDrip(Transform parent)
        {
            var drip = new Drip
            {
                stem = CreateImage(parent, "Stem", null, new Vector2(0.5f, 1f), acidColor),
                drop = CreateImage(parent, "Drop", Circle(), new Vector2(0.5f, 0.5f), Color.Lerp(acidColor, Color.white, 0.25f)),
            };
            drip.dropImage = drip.drop.GetComponent<Image>();
            return drip;
        }

        static RectTransform CreateImage(Transform parent, string name, Sprite sprite, Vector2 pivot, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.pivot = pivot;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            go.SetActive(false);
            return rect;
        }

        void Update()
        {
            float time = Time.unscaledTime;
            float dt = Time.unscaledDeltaTime;

            text.ForceMeshUpdate();
            TMP_TextInfo info = text.textInfo;
            Melt(info, time);

            if (material != null && material.HasProperty(ShaderUtilities.ID_UnderlayDilate))
                material.SetFloat(ShaderUtilities.ID_UnderlayDilate, baseDilate + glowPulse * Mathf.Sin(time * 2.2f));

            if (time >= nextSpawn)
            {
                nextSpawn = time + Random.Range(spawnDelay.x, spawnDelay.y);
                Drip idle = System.Array.Find(drips, d => !d.growing && !d.falling);
                if (idle != null && TryPickDripPoint(info, out Vector2 point)) Begin(idle, point);
            }

            foreach (Drip drip in drips)
                Animate(drip, dt);
        }

        void Melt(TMP_TextInfo info, float time)
        {
            for (int i = 0; i < info.characterCount; i++)
            {
                TMP_CharacterInfo letter = info.characterInfo[i];
                if (!letter.isVisible) continue;

                Vector3[] vertices = info.meshInfo[letter.materialReferenceIndex].vertices;
                float wave = 0.6f + 0.4f * Mathf.Sin(time * 1.3f + i * 1.7f);
                int seed = i % sagLeft.Length;
                vertices[letter.vertexIndex + 0].y -= sag * sagLeft[seed] * wave;  // bottom-left
                vertices[letter.vertexIndex + 3].y -= sag * sagRight[seed] * wave; // bottom-right
            }
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }

        bool TryPickDripPoint(TMP_TextInfo info, out Vector2 point)
        {
            point = Vector2.zero;
            for (int attempt = 0; attempt < 8 && info.characterCount > 0; attempt++)
            {
                TMP_CharacterInfo letter = info.characterInfo[Random.Range(0, info.characterCount)];
                if (!letter.isVisible) continue;

                float width = letter.bottomRight.x - letter.bottomLeft.x;
                float x = Random.Range(letter.bottomLeft.x + width * 0.15f, letter.bottomRight.x - width * 0.15f);
                point = new Vector2(x, letter.baseLine - sag * 0.4f);
                return true;
            }
            return false;
        }

        void Begin(Drip drip, Vector2 origin)
        {
            drip.origin = origin;
            drip.length = Random.Range(dripLength.x, dripLength.y);
            drip.width = Random.Range(dripWidth.x, dripWidth.y);
            drip.grow = Random.Range(growTime.x, growTime.y);
            drip.timer = 0f;
            drip.growing = true;
            drip.falling = false;
            drip.dropImage.color = Color.Lerp(acidColor, Color.white, 0.25f);
            drip.stem.gameObject.SetActive(true);
            drip.drop.gameObject.SetActive(true);
            Animate(drip, 0f);
        }

        void Animate(Drip drip, float dt)
        {
            if (!drip.growing && !drip.falling) return;
            drip.timer += dt;

            if (drip.growing)
            {
                float k = Mathf.Clamp01(drip.timer / drip.grow);
                float height = drip.length * (1f - (1f - k) * (1f - k));
                drip.stem.localPosition = drip.origin;
                drip.stem.sizeDelta = new Vector2(drip.width * (1f - 0.35f * k), height);
                drip.drop.localPosition = drip.origin - new Vector2(0f, height);
                float size = drip.width * (1.1f + 0.6f * k);
                drip.drop.sizeDelta = new Vector2(size, size);

                if (k >= 1f)
                {
                    drip.growing = false;
                    drip.falling = true;
                    drip.timer = 0f;
                    drip.dropY = drip.origin.y - height;
                    drip.dropSpeed = 0f;
                }
                return;
            }

            float t = Mathf.Clamp01(drip.timer / FallTime);
            drip.dropSpeed += Gravity * dt;
            drip.dropY -= drip.dropSpeed * dt;
            drip.drop.localPosition = new Vector2(drip.origin.x, drip.dropY);
            float dropSize = drip.width * 1.7f;
            drip.drop.sizeDelta = new Vector2(dropSize * (1f - 0.2f * t), dropSize * (1f + 0.35f * t));
            Color color = drip.dropImage.color;
            color.a = 1f - t;
            drip.dropImage.color = color;
            drip.stem.sizeDelta = new Vector2(drip.stem.sizeDelta.x, drip.length * (1f - t));

            if (t >= 1f)
            {
                drip.falling = false;
                drip.stem.gameObject.SetActive(false);
                drip.drop.gameObject.SetActive(false);
            }
        }

        internal static Sprite Circle()
        {
            if (circle != null) return circle;

            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            float radius = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius));
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radius - distance)));
                }
            }
            texture.Apply();
            circle = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return circle;
        }
    }
}
