using System;
using DeadZone.Core;
using DeadZone.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DeadZone.UI
{
    // Locker-room character select: every character owns a locker of the background art. The selected
    // one opens under a flickering green spotlight, the character steps out and its file shows on the left.
    // Everything comes from CharacterData, so new art or new characters show up without touching this.
    // It also opens at the start of a level with timeScale 0, so it only uses unscaled time.
    public sealed class CharacterSelectUI : MonoBehaviour
    {
        // Locker doors in Resources/Menu/CharacterSelectBG.png, normalized from the image's left edge (x0, x1).
        static readonly Vector2[] LockerX =
        {
            new Vector2(0.2889f, 0.3660f),
            new Vector2(0.3690f, 0.4498f),
            new Vector2(0.4528f, 0.5353f),
            new Vector2(0.5383f, 0.6178f),
            new Vector2(0.6202f, 0.6938f),
        };
        const float LockerTop = 0.2784f;    // normalized from the image's top edge
        const float LockerBottom = 0.7088f;
        const float FloorY = 0.745f;
        const float HeroHeight = 400f;

        static readonly Vector2 DarknessSize = new Vector2(4224f, 2376f);
        static readonly Vector2 HoleRadius = new Vector2(230f, 352f);

        static readonly Color Neon = new Color(0.45f, 1f, 0.15f);
        static readonly Color Paper = new Color(0.84f, 0.79f, 0.66f);
        static readonly Color Ink = new Color(0.2f, 0.16f, 0.12f);
        static readonly Color InkGreen = new Color(0.12f, 0.36f, 0.08f);
        static readonly Color TagPaper = new Color(0.89f, 0.84f, 0.69f);
        static readonly Color LockedTag = new Color(0.59f, 0.55f, 0.47f);
        static readonly Color DoorColor = new Color(0.2f, 0.25f, 0.23f);
        static readonly Color BarOn = new Color(0.28f, 0.67f, 0.16f);
        static readonly Color BarOff = new Color(0.59f, 0.55f, 0.45f);
        static readonly Color StampRed = new Color(0.67f, 0.12f, 0.12f, 0.85f);

        sealed class Locker
        {
            public RectTransform door;
            public GameObject inside;
            public RectTransform tag;
            public Outline tagOutline;
            public float open;
        }

        static Texture2D darknessTexture;
        static Texture2D coneTexture;
        static Sprite triangleSprite;
        static Sprite ringSprite;

        CharacterData[] characters;
        Action<CharacterData, PlayerInput> onConfirm;
        PlayerInput playerInput;
        TMP_FontAsset font;
        int index;
        float nextNavTime;

        RectTransform root;
        RectTransform scene;
        RectTransform darkness;
        RawImage darknessImage;
        RectTransform cone;
        RawImage coneImage;
        Locker[] lockers;
        RectTransform hero;
        Image heroImage;
        RectTransform heroShadow;
        RectTransform leftArrow;
        RectTransform rightArrow;
        RectTransform file;
        Image photo;
        TextMeshProUGUI fileNumber;
        TextMeshProUGUI nameText;
        TextMeshProUGUI roleText;
        TextMeshProUGUI weaponText;
        TextMeshProUGUI abilityText;
        Image[] healthBar;
        Image[] speedBar;
        RectTransform stamp;
        Image fade;
        TextMeshProUGUI fadeText;

        float spotX;
        float heroTimer = 1f;
        float fileTimer = 1f;
        float nextFlicker;
        float flickerUntil;
        bool confirmed;
        bool finished;
        float confirmTimer;
        float minHealth, maxHealth, minSpeed, maxSpeed;

        public void Begin(CharacterData[] data, Action<CharacterData, PlayerInput> confirm, PlayerInput input)
        {
            characters = data;
            onConfirm = confirm;
            playerInput = input;
            Array.Sort(characters, (a, b) => a.sortOrder.CompareTo(b.sortOrder));

            font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                TMP_Text sample = FindAnyObjectByType<TMP_Text>();
                if (sample != null) font = sample.font;
            }

            minHealth = minSpeed = float.MaxValue;
            maxHealth = maxSpeed = float.MinValue;
            foreach (CharacterData character in characters)
            {
                if (character.config == null) continue;
                minHealth = Mathf.Min(minHealth, character.config.maxHealth);
                maxHealth = Mathf.Max(maxHealth, character.config.maxHealth);
                minSpeed = Mathf.Min(minSpeed, character.config.moveSpeed);
                maxSpeed = Mathf.Max(maxSpeed, character.config.moveSpeed);
            }

            Build();
            spotX = LockerCenter(0);
            nextFlicker = Time.unscaledTime + 2f;
            Select(0);
        }

        // ---------------- building ----------------

        void Build()
        {
            GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            root = NewRect("Root", canvasGo.transform);
            Stretch(root);
            scene = NewRect("Scene", root);
            Stretch(scene);
            Texture2D art = Resources.Load<Texture2D>("Menu/CharacterSelectBG");
            if (art != null)
            {
                RawImage raw = scene.gameObject.AddComponent<RawImage>();
                raw.texture = art;
                raw.raycastTarget = false;
                AspectRatioFitter fitter = scene.gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = (float)art.width / art.height;
            }
            else
            {
                scene.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.04f, 0.05f);
            }

            lockers = new Locker[LockerX.Length];
            for (int i = 0; i < lockers.Length; i++) lockers[i] = BuildLockerDoor(i);

            darknessImage = NewRaw("Darkness", scene, DarknessTexture());
            darkness = darknessImage.rectTransform;
            darkness.sizeDelta = DarknessSize;
            coneImage = NewRaw("Light", scene, ConeTexture());
            coneImage.color = new Color(0.47f, 1f, 0.47f, 0.22f);
            cone = coneImage.rectTransform;
            cone.pivot = new Vector2(0.5f, 1f);
            cone.sizeDelta = new Vector2(520f, FloorY * 1080f + 40f);

            for (int i = 0; i < lockers.Length; i++) BuildLockerTag(i);

            Image shadow = NewImage("HeroShadow", scene, new Color(0f, 0f, 0f, 0.55f), AcidDripText.Circle());
            heroShadow = shadow.rectTransform;
            heroShadow.sizeDelta = new Vector2(220f, 36f);
            heroImage = NewImage("Hero", scene, Color.white, null);
            heroImage.preserveAspect = true;
            hero = heroImage.rectTransform;
            hero.pivot = new Vector2(0.5f, 0f);
            leftArrow = NewImage("ArrowLeft", scene, Neon, TriangleSprite()).rectTransform;
            rightArrow = NewImage("ArrowRight", scene, Neon, TriangleSprite()).rectTransform;
            leftArrow.sizeDelta = rightArrow.sizeDelta = new Vector2(44f, 70f);
            leftArrow.localScale = new Vector3(-1f, 1f, 1f);

            BuildFile();

            TextMeshProUGUI title = NewText("Title", root, "ELIGE TU SUPERVIVIENTE", 64f, Neon, TextAlignmentOptions.Center);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(1600f, 110f));
            title.fontStyle = FontStyles.Bold;
            MainMenuUI.MakeNeon(title, Neon, 0.5f);

            TextMeshProUGUI hint = NewText("Hint", root, "A / D  o  ← / →  elegir      J / Espacio  confirmar", 30f,
                new Color(0.95f, 0.97f, 0.95f), TextAlignmentOptions.Center);
            Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(1600f, 60f));
            hint.outlineColor = new Color32(0, 0, 0, 255);
            hint.outlineWidth = 0.25f;

            fade = NewImage("Fade", root, new Color(0f, 0f, 0f, 0f), null);
            Stretch(fade.rectTransform);
            fadeText = NewText("Preparing", root, "PREPARÁNDOSE…", 56f, new Color(Neon.r, Neon.g, Neon.b, 0f), TextAlignmentOptions.Center);
            Place(fadeText.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200f, 100f));
        }

        Locker BuildLockerDoor(int i)
        {
            var locker = new Locker();
            Vector2 x = LockerX[i];

            Image inside = NewImage("Inside_" + i, scene, new Color(0.04f, 0.055f, 0.05f), null);
            AnchorBox(inside.rectTransform, x.x + 0.004f, LockerTop + 0.007f, x.y - 0.004f, LockerBottom - 0.004f);
            AnchorBox(NewImage("Shelf", inside.transform, new Color(0.16f, 0.19f, 0.17f), null).rectTransform, 0.06f, 0.15f, 0.94f, 0.165f);
            AnchorBox(NewImage("Hook", inside.transform, new Color(0.24f, 0.27f, 0.25f), null).rectTransform, 0.48f, 0.165f, 0.52f, 0.21f);
            locker.inside = inside.gameObject;
            locker.inside.SetActive(false);

            Image door = NewImage("Door_" + i, scene, DoorColor, null);
            locker.door = door.rectTransform;
            AnchorBox(locker.door, x.x, LockerTop, x.y, LockerBottom);
            locker.door.pivot = new Vector2(0f, 0.5f);
            for (int k = 0; k < 4; k++)
            {
                float y = 0.1f + k * 0.03f;
                AnchorBox(NewImage("Vent", locker.door, new Color(0.12f, 0.15f, 0.14f), null).rectTransform, 0.25f, y, 0.75f, y + 0.012f);
            }
            AnchorBox(NewImage("Handle", locker.door, new Color(0.1f, 0.12f, 0.11f), null).rectTransform, 0.78f, 0.46f, 0.86f, 0.56f);
            locker.door.gameObject.SetActive(false);
            return locker;
        }

        void BuildLockerTag(int i)
        {
            Locker locker = lockers[i];
            bool used = i < characters.Length;
            float center = LockerCenter(i);

            Image tag = NewImage("Tag_" + i, scene, used ? TagPaper : LockedTag, null);
            locker.tag = tag.rectTransform;
            Anchor(locker.tag, center, LockerTop - 0.03f);
            locker.tag.sizeDelta = new Vector2(150f, 42f);
            locker.tag.localEulerAngles = new Vector3(0f, 0f, ((i * 37) % 7 - 3) * 0.9f);
            locker.tagOutline = tag.gameObject.AddComponent<Outline>();
            locker.tagOutline.effectColor = Neon;
            locker.tagOutline.effectDistance = new Vector2(4f, -4f);
            locker.tagOutline.enabled = false;

            string label = used ? characters[i].displayName.ToUpperInvariant() : "???";
            TextMeshProUGUI text = NewText("Name", locker.tag, label, 24f, Ink, TextAlignmentOptions.Center);
            Stretch(text.rectTransform);
            text.fontStyle = FontStyles.Bold;

            if (used) return;

            // padlock on lockers nobody owns yet
            float lockY = (LockerTop + LockerBottom) / 2f;
            Image shackle = NewImage("Shackle", scene, new Color(0.6f, 0.6f, 0.56f), RingSprite());
            Anchor(shackle.rectTransform, center, lockY - 0.025f);
            shackle.rectTransform.sizeDelta = new Vector2(48f, 48f);
            Image body = NewImage("Padlock", scene, new Color(0.69f, 0.55f, 0.24f), null);
            Anchor(body.rectTransform, center, lockY);
            body.rectTransform.sizeDelta = new Vector2(60f, 48f);
            Image keyhole = NewImage("Keyhole", body.transform, new Color(0.16f, 0.12f, 0.06f), AcidDripText.Circle());
            keyhole.rectTransform.sizeDelta = new Vector2(12f, 12f);
        }

        void BuildFile()
        {
            Image fileShadow = NewImage("FileShadow", scene, new Color(0f, 0f, 0f, 0.6f), null);
            Anchor(fileShadow.rectTransform, 0.152f, 0.512f);
            fileShadow.rectTransform.sizeDelta = new Vector2(430f, 600f);
            fileShadow.rectTransform.localEulerAngles = new Vector3(0f, 0f, 2f);

            Image paper = NewImage("File", scene, Paper, null);
            file = paper.rectTransform;
            Anchor(file, 0.145f, 0.5f);
            file.sizeDelta = new Vector2(430f, 600f);
            file.localEulerAngles = new Vector3(0f, 0f, 2f);
            Outline border = paper.gameObject.AddComponent<Outline>();
            border.effectColor = new Color(0.47f, 0.39f, 0.27f);
            border.effectDistance = new Vector2(3f, -3f);

            Place(NewImage("Header", file, new Color(0.27f, 0.23f, 0.18f), null).rectTransform, 0f, 0f, 430f, 56f);
            fileNumber = NewText("Number", file, string.Empty, 28f, new Color(0.9f, 0.86f, 0.75f), TextAlignmentOptions.MidlineLeft);
            Place(fileNumber.rectTransform, 20f, 0f, 400f, 56f);
            fileNumber.fontStyle = FontStyles.Bold;
            Place(NewImage("Clip", file, new Color(0.67f, 0.67f, 0.63f), null).rectTransform, 360f, -14f, 32f, 38f);

            Image frame = NewImage("PhotoFrame", file, new Color(0.24f, 0.26f, 0.24f), null);
            Place(frame.rectTransform, 24f, 80f, 136f, 180f);
            photo = NewImage("Photo", frame.transform, Color.white, null);
            photo.preserveAspect = true;
            Stretch(photo.rectTransform, 6f);

            nameText = NewText("Name", file, string.Empty, 46f, InkGreen, TextAlignmentOptions.TopLeft);
            Place(nameText.rectTransform, 180f, 88f, 236f, 64f);
            nameText.fontStyle = FontStyles.Bold;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 24f;
            nameText.fontSizeMax = 46f;
            roleText = NewText("Role", file, string.Empty, 22f, Ink, TextAlignmentOptions.TopLeft);
            Place(roleText.rectTransform, 180f, 156f, 236f, 100f);

            healthBar = BuildBar("VIDA", 290f);
            speedBar = BuildBar("VELOCIDAD", 335f);

            Label("ARMA:", 24f, 390f, 100f);
            weaponText = NewText("Weapon", file, string.Empty, 24f, Ink, TextAlignmentOptions.TopLeft);
            Place(weaponText.rectTransform, 120f, 390f, 290f, 34f);
            Label("HABILIDAD:", 24f, 435f, 200f);
            abilityText = NewText("Ability", file, string.Empty, 24f, Ink, TextAlignmentOptions.TopLeft);
            Place(abilityText.rectTransform, 24f, 470f, 380f, 110f);
            abilityText.enableAutoSizing = true;
            abilityText.fontSizeMin = 16f;
            abilityText.fontSizeMax = 24f;

            stamp = NewRect("Stamp", file);
            Place(stamp, 175f, 505f, 240f, 70f);
            stamp.localEulerAngles = new Vector3(0f, 0f, 12f);
            Place(NewImage("Top", stamp, StampRed, null).rectTransform, 0f, 0f, 240f, 5f);
            Place(NewImage("Bottom", stamp, StampRed, null).rectTransform, 0f, 65f, 240f, 5f);
            Place(NewImage("Left", stamp, StampRed, null).rectTransform, 0f, 0f, 5f, 70f);
            Place(NewImage("Right", stamp, StampRed, null).rectTransform, 235f, 0f, 5f, 70f);
            TextMeshProUGUI stampText = NewText("Text", stamp, "SUPERVIVIENTE", 26f, StampRed, TextAlignmentOptions.Center);
            Stretch(stampText.rectTransform);
            stampText.fontStyle = FontStyles.Bold;
            stamp.gameObject.SetActive(false);
        }

        Image[] BuildBar(string label, float y)
        {
            Label(label, 24f, y, 170f);
            var segments = new Image[10];
            for (int k = 0; k < segments.Length; k++)
            {
                segments[k] = NewImage("Segment", file, BarOff, null);
                Place(segments[k].rectTransform, 190f + k * 22f, y + 2f, 17f, 24f);
            }
            return segments;
        }

        void Label(string text, float x, float y, float width)
        {
            TextMeshProUGUI label = NewText(text, file, text, 24f, Ink, TextAlignmentOptions.TopLeft);
            Place(label.rectTransform, x, y, width, 34f);
            label.fontStyle = FontStyles.Bold;
        }

        // ---------------- update ----------------

        void Update()
        {
            if (characters == null || characters.Length == 0) return;

            float dt = Time.unscaledDeltaTime;
            float time = Time.unscaledTime;

            if (!confirmed) HandleInput();
            AnimateSpotlight(dt, time);
            AnimateLockers(dt);
            AnimateHero(dt, time);
            AnimateFile(dt);
            if (confirmed) AnimateConfirm(dt);
        }

        void HandleInput()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;

            if (keyboard != null)
            {
                if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame) Move(-1);
                if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame) Move(1);
            }

            if (gamepad != null)
            {
                if (gamepad.dpad.left.wasPressedThisFrame) Move(-1);
                if (gamepad.dpad.right.wasPressedThisFrame) Move(1);
                float stick = gamepad.leftStick.ReadValue().x;
                if (Time.unscaledTime >= nextNavTime && Mathf.Abs(stick) > 0.5f)
                {
                    Move(stick > 0f ? 1 : -1);
                    nextNavTime = Time.unscaledTime + 0.2f;
                }
            }

            bool confirm = keyboard != null
                && (keyboard.jKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame);
            if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame) confirm = true;
            if (confirm) Confirm();
        }

        void Move(int direction)
        {
            Select((index + direction + characters.Length) % characters.Length);
            AudioManager.PlaySfx(AudioManager.Sfx.Pickup);
        }

        void Select(int newIndex)
        {
            index = newIndex;
            heroTimer = 0f;
            fileTimer = 0f;
            CharacterData character = characters[index];

            heroImage.sprite = character.bodySprite;
            float aspect = character.bodySprite != null ? character.bodySprite.rect.width / character.bodySprite.rect.height : 0.66f;
            hero.sizeDelta = new Vector2(HeroHeight * aspect, HeroHeight);
            float center = LockerCenter(index);
            Anchor(hero, center, FloorY);
            Anchor(heroShadow, center, FloorY);

            fileNumber.text = "EXPEDIENTE  #" + (index + 1).ToString("00");
            photo.sprite = character.bodySprite;
            nameText.text = character.displayName.ToUpperInvariant();
            int dot = character.description != null ? character.description.IndexOf('.') : -1;
            roleText.text = dot > 0 ? character.description.Substring(0, dot) : character.description;
            weaponText.text = character.startingWeapon != null ? character.startingWeapon.weaponName : "—";
            abilityText.text = character.abilityDescription;
            if (character.config != null)
            {
                FillBar(healthBar, character.config.maxHealth, minHealth, maxHealth);
                FillBar(speedBar, character.config.moveSpeed, minSpeed, maxSpeed);
            }

            for (int i = 0; i < lockers.Length; i++)
            {
                bool selected = i == index;
                lockers[i].tagOutline.enabled = selected;
                lockers[i].tag.localScale = Vector3.one * (selected ? 1.08f : 1f);
            }
        }

        static void FillBar(Image[] bar, float value, float min, float max)
        {
            float t = max > min ? Mathf.InverseLerp(min, max, value) : 0.5f;
            int filled = Mathf.RoundToInt(Mathf.Lerp(4f, 10f, t));
            for (int k = 0; k < bar.Length; k++) bar[k].color = k < filled ? BarOn : BarOff;
        }

        void AnimateSpotlight(float dt, float time)
        {
            spotX = Mathf.Lerp(spotX, LockerCenter(index), 1f - Mathf.Exp(-dt * 10f));
            Anchor(darkness, spotX, (LockerTop + FloorY) / 2f);
            Anchor(cone, spotX, 0f);

            if (time >= nextFlicker)
            {
                flickerUntil = time + UnityEngine.Random.Range(0.15f, 0.45f);
                nextFlicker = time + UnityEngine.Random.Range(2.5f, 6f);
            }
            bool lampOn = time >= flickerUntil || Mathf.PerlinNoise(time * 25f, 0f) > 0.45f;
            if (confirmed) lampOn = true;
            coneImage.color = new Color(0.47f, 1f, 0.47f, confirmed ? 0.45f : lampOn ? 0.22f : 0.04f);
            darknessImage.color = new Color(1f, 1f, 1f, lampOn ? 0.85f : 1f);
        }

        void AnimateLockers(float dt)
        {
            for (int i = 0; i < lockers.Length; i++)
            {
                Locker locker = lockers[i];
                float target = i == index && i < characters.Length ? 1f : 0f;
                locker.open = Mathf.MoveTowards(locker.open, target, dt * 5f);
                bool visible = locker.open > 0.001f;
                locker.inside.SetActive(visible);
                locker.door.gameObject.SetActive(visible);
                float swing = 1f - (1f - locker.open) * (1f - locker.open);
                locker.door.localScale = new Vector3(Mathf.Lerp(1f, -0.42f, swing), 1f, 1f);
            }
        }

        void AnimateHero(float dt, float time)
        {
            heroTimer += dt;
            float t = Mathf.Clamp01(heroTimer / 0.3f);
            float back = 1f + 2.2f * Mathf.Pow(t - 1f, 3f) + 1.2f * Mathf.Pow(t - 1f, 2f);
            float breathe = 1f + 0.012f * Mathf.Sin(time * 2f);
            hero.localScale = new Vector3(Mathf.Lerp(0.85f, 1f, back), Mathf.Lerp(0.85f, 1f, back) * breathe, 1f);
            hero.anchoredPosition = new Vector2(0f, -10f + (1f - t) * 30f);
            heroImage.color = new Color(1f, 1f, 1f, t);
            heroShadow.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, t);

            float bob = 215f + 6f * Mathf.Sin(time * 4f);
            float middle = (LockerTop + FloorY) / 2f;
            float center = LockerCenter(index);
            Anchor(leftArrow, center, middle);
            Anchor(rightArrow, center, middle);
            leftArrow.anchoredPosition = new Vector2(-bob, 0f);
            rightArrow.anchoredPosition = new Vector2(bob, 0f);
            bool arrows = characters.Length > 1 && !confirmed;
            leftArrow.gameObject.SetActive(arrows);
            rightArrow.gameObject.SetActive(arrows);
        }

        void AnimateFile(float dt)
        {
            fileTimer += dt;
            float t = Mathf.Clamp01(fileTimer / 0.15f);
            file.localScale = Vector3.one * Mathf.Lerp(0.96f, 1f, t);
        }

        void Confirm()
        {
            if (confirmed) return;
            confirmed = true;
            confirmTimer = 0f;
            stamp.gameObject.SetActive(true);
            AudioManager.PlaySfx(AudioManager.Sfx.Hit);
        }

        void AnimateConfirm(float dt)
        {
            confirmTimer += dt;
            stamp.localScale = Vector3.one * Mathf.Lerp(2.2f, 1f, Mathf.Clamp01(confirmTimer / 0.18f));

            float shake = Mathf.Clamp01(1f - confirmTimer / 0.35f);
            root.anchoredPosition = shake > 0f ? UnityEngine.Random.insideUnitCircle * 12f * shake : Vector2.zero;

            float f = Mathf.Clamp01((confirmTimer - 0.6f) / 0.7f);
            fade.color = new Color(0f, 0f, 0f, f);
            fadeText.color = new Color(Neon.r, Neon.g, Neon.b, f);

            if (confirmTimer < 1.5f || finished) return;
            finished = true;
            onConfirm?.Invoke(characters[index], playerInput);
            Destroy(gameObject);
        }

        static float LockerCenter(int i)
        {
            Vector2 x = LockerX[Mathf.Clamp(i, 0, LockerX.Length - 1)];
            return (x.x + x.y) / 2f;
        }

        // ---------------- UI helpers ----------------

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static Image NewImage(string name, Transform parent, Color color, Sprite sprite)
        {
            Image image = NewRect(name, parent).gameObject.AddComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        static RawImage NewRaw(string name, Transform parent, Texture texture)
        {
            RawImage raw = NewRect(name, parent).gameObject.AddComponent<RawImage>();
            raw.texture = texture;
            raw.raycastTarget = false;
            return raw;
        }

        TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions alignment)
        {
            TextMeshProUGUI label = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;
            label.text = text;
            label.raycastTarget = false;
            return label;
        }

        static void Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        // Box in normalized coordinates of the parent, measured from its top-left corner.
        static void AnchorBox(RectTransform rect, float x0, float y0, float x1, float y1)
        {
            rect.anchorMin = new Vector2(x0, 1f - y1);
            rect.anchorMax = new Vector2(x1, 1f - y0);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // Point in normalized coordinates of the parent, measured from its top-left corner.
        static void Anchor(RectTransform rect, float x, float y)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(x, 1f - y);
        }

        // Pixel box inside a parent, measured from its top-left corner.
        static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        // ---------------- generated textures ----------------

        static Texture2D NewTexture(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
        }

        // Black overlay with a soft elliptical hole in the middle (the spotlight).
        static Texture2D DarknessTexture()
        {
            if (darknessTexture != null) return darknessTexture;
            const int size = 256;
            darknessTexture = NewTexture(size, size);
            Vector2 radius = new Vector2(HoleRadius.x / DarknessSize.x, HoleRadius.y / DarknessSize.y);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = ((x + 0.5f) / size - 0.5f) / radius.x;
                    float dy = ((y + 0.5f) / size - 0.5f) / radius.y;
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.7f, 1.45f, Mathf.Sqrt(dx * dx + dy * dy)));
                    darknessTexture.SetPixel(x, y, new Color(0f, 0f, 0f, a * 0.72f));
                }
            }
            darknessTexture.Apply();
            return darknessTexture;
        }

        // Light cone from the ceiling lamp: narrow at the top, wide and faint at the floor.
        static Texture2D ConeTexture()
        {
            if (coneTexture != null) return coneTexture;
            const int width = 64, height = 256;
            coneTexture = NewTexture(width, height);
            for (int y = 0; y < height; y++)
            {
                float v = 1f - (y + 0.5f) / height;
                float half = Mathf.Lerp(0.12f, 0.5f, v);
                for (int x = 0; x < width; x++)
                {
                    float u = Mathf.Abs((x + 0.5f) / width - 0.5f);
                    float edge = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(half - 0.1f, half, u));
                    coneTexture.SetPixel(x, y, new Color(1f, 1f, 1f, edge * Mathf.Lerp(1f, 0.35f, v)));
                }
            }
            coneTexture.Apply();
            return coneTexture;
        }

        static Sprite TriangleSprite()
        {
            if (triangleSprite != null) return triangleSprite;
            const int size = 64;
            Texture2D texture = NewTexture(size, size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size;
                    float v = Mathf.Abs((y + 0.5f) / size - 0.5f) * 2f;
                    float inside = (1f - u) - v;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(inside * size * 0.5f)));
                }
            }
            texture.Apply();
            triangleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return triangleSprite;
        }

        static Sprite RingSprite()
        {
            if (ringSprite != null) return ringSprite;
            const int size = 64;
            Texture2D texture = NewTexture(size, size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float r = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size / 2f, size / 2f)) / (size / 2f);
                    float a = Mathf.Clamp01((1f - r) * size * 0.5f) * Mathf.Clamp01((r - 0.62f) * size * 0.5f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            texture.Apply();
            ringSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return ringSprite;
        }
    }
}
