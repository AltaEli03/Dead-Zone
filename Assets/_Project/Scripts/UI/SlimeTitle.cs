using UnityEngine;
using UnityEngine.UI;

namespace DeadZone.UI
{
    // Slime "DEAD ZONE" title: the art from Resources/Menu with a pulsing glow,
    // plus acid drops that form at the tips of the drawn drips and fall off.
    public sealed class SlimeTitle : MonoBehaviour
    {
        // Tips of the drips that hang below the letters in TitleDeadZone.png, normalized from its top-left corner.
        static readonly Vector2[] DripTips =
        {
            new Vector2(0.0593f, 0.6943f),
            new Vector2(0.1479f, 0.6367f),
            new Vector2(0.1761f, 0.8613f),
            new Vector2(0.2892f, 0.5825f),
            new Vector2(0.3381f, 0.6465f),
            new Vector2(0.3741f, 0.8489f),
            new Vector2(0.4023f, 0.7947f),
            new Vector2(0.4403f, 0.6334f),
            new Vector2(0.6095f, 0.6415f),
            new Vector2(0.7099f, 0.8715f),
            new Vector2(0.8184f, 0.5963f),
            new Vector2(0.8866f, 0.9387f),
            new Vector2(0.9134f, 0.7045f),
        };

        static readonly Color DropColor = new Color(0.53f, 0.87f, 0.25f);
        static readonly Color DropEdge = new Color(0.055f, 0.15f, 0.04f);
        const float DropSize = 13f;
        const float Gravity = 900f;
        const float FallTime = 0.7f;

        sealed class Drop
        {
            public RectTransform root;
            public Image fill;
            public Image edge;
            public Vector2 tip;
            public float timer, form, y, speed;
            public bool forming, falling;
        }

        RawImage glow;
        RectTransform art;
        Drop[] drops;
        float[] nextDrop;

        public static SlimeTitle Create(Transform parent, Texture2D titleArt, Texture2D glowArt, Vector2 center, float width)
        {
            var go = new GameObject("SlimeTitle", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            float height = width * titleArt.height / titleArt.width;
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = center;

            var title = go.AddComponent<SlimeTitle>();
            title.Build(titleArt, glowArt, width, height);
            return title;
        }

        void Build(Texture2D titleArt, Texture2D glowArt, float width, float height)
        {
            if (glowArt != null) glow = CreateRaw("Glow", glowArt, width, height);
            art = CreateRaw("Art", titleArt, width, height).rectTransform;

            drops = new Drop[DripTips.Length];
            nextDrop = new float[DripTips.Length];
            for (int i = 0; i < drops.Length; i++)
            {
                Vector2 tip = new Vector2((DripTips[i].x - 0.5f) * width, (0.5f - DripTips[i].y) * height);
                drops[i] = CreateDrop(tip);
                nextDrop[i] = Time.unscaledTime + Random.Range(0.3f, 4f);
            }
        }

        RawImage CreateRaw(string name, Texture2D texture, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(transform, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(width, height);
            var raw = go.GetComponent<RawImage>();
            raw.texture = texture;
            raw.raycastTarget = false;
            return raw;
        }

        Drop CreateDrop(Vector2 tip)
        {
            var root = new GameObject("Drop", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(transform, false);
            var drop = new Drop
            {
                root = root,
                edge = CreateCircle(root, "Edge", DropEdge),
                fill = CreateCircle(root, "Fill", DropColor),
                tip = tip,
            };
            root.gameObject.SetActive(false);
            return drop;
        }

        static Image CreateCircle(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = AcidDripText.Circle();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        void Update()
        {
            float time = Time.unscaledTime;
            float dt = Time.unscaledDeltaTime;

            if (glow != null) glow.color = new Color(1f, 1f, 1f, 0.55f + 0.3f * Mathf.Sin(time * 2.2f));
            art.localScale = new Vector3(1f, 1f + 0.012f * Mathf.Sin(time * 1.6f), 1f);

            for (int i = 0; i < drops.Length; i++)
            {
                Drop drop = drops[i];
                if (!drop.forming && !drop.falling && time >= nextDrop[i]) Begin(drop);
                if (Animate(drop, dt)) nextDrop[i] = time + Random.Range(1.5f, 5f);
            }
        }

        static void Begin(Drop drop)
        {
            drop.forming = true;
            drop.falling = false;
            drop.timer = 0f;
            drop.form = Random.Range(0.8f, 1.6f);
            drop.y = drop.tip.y;
            drop.speed = 0f;
            drop.root.gameObject.SetActive(true);
            SetAlpha(drop, 1f);
        }

        // Returns true on the frame the drop finishes.
        static bool Animate(Drop drop, float dt)
        {
            if (!drop.forming && !drop.falling) return false;
            drop.timer += dt;

            if (drop.forming)
            {
                float k = Mathf.Clamp01(drop.timer / drop.form);
                SetSize(drop, DropSize * k, DropSize * k);
                drop.root.localPosition = new Vector2(drop.tip.x, drop.tip.y - DropSize * 0.35f * k);
                if (k >= 1f)
                {
                    drop.forming = false;
                    drop.falling = true;
                    drop.timer = 0f;
                    drop.y = drop.tip.y - DropSize * 0.35f;
                }
                return false;
            }

            float t = Mathf.Clamp01(drop.timer / FallTime);
            drop.speed += Gravity * dt;
            drop.y -= drop.speed * dt;
            drop.root.localPosition = new Vector2(drop.tip.x, drop.y);
            SetSize(drop, DropSize * (1f - 0.15f * t), DropSize * (1f + 0.35f * t));
            SetAlpha(drop, 1f - t);

            if (t < 1f) return false;
            drop.falling = false;
            drop.root.gameObject.SetActive(false);
            return true;
        }

        static void SetSize(Drop drop, float width, float height)
        {
            drop.fill.rectTransform.sizeDelta = new Vector2(width, height);
            drop.edge.rectTransform.sizeDelta = new Vector2(width + 5f, height + 5f);
        }

        static void SetAlpha(Drop drop, float alpha)
        {
            Color fill = drop.fill.color;
            fill.a = alpha;
            drop.fill.color = fill;
            Color edge = drop.edge.color;
            edge.a = alpha;
            drop.edge.color = edge;
        }
    }
}
