using DeadZone.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeadZone.UI
{
    public sealed class MainMenuUI : MonoBehaviour
    {
        readonly string[] options = { "JUGAR", "OPCIONES", "SALIR" };

        static readonly Color NeonGreen = new Color(0.45f, 1f, 0.15f);
        static readonly Color SelectedGreen = new Color(0.2f, 0.9f, 0.6f);
        static readonly Color HintWhite = new Color(0.95f, 0.97f, 0.95f);

        TMP_FontAsset font;
        TextMeshProUGUI[] optionTexts;
        TextMeshProUGUI volumeText;
        TextMeshProUGUI hintText;
        GameObject optionsGroup;
        bool optionsMode;
        int index;
        float nextNavTime;

        void Start()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameSettings.Apply();
            Build();
            Refresh();
        }

        void Build()
        {
            font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                TMP_Text sample = FindAnyObjectByType<TMP_Text>();
                if (sample != null) font = sample.font;
            }

            GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateBackground(canvasGo.transform);

            Texture2D titleArt = Resources.Load<Texture2D>("Menu/TitleDeadZone");
            if (titleArt != null)
            {
                SlimeTitle.Create(canvasGo.transform, titleArt, Resources.Load<Texture2D>("Menu/TitleGlow"),
                    new Vector2(0f, 372f), 1300f);
            }
            else
            {
                TextMeshProUGUI title = CreateText(canvasGo.transform, "Title", 130f, new Vector2(0f, 330f), "DEAD ZONE", NeonGreen);
                title.rectTransform.sizeDelta = new Vector2(1500f, 200f);
                title.fontStyle = FontStyles.Bold;
                MakeNeon(title, NeonGreen, 0.6f);
                title.gameObject.AddComponent<AcidDripText>();
            }

            TextMeshProUGUI subtitle = CreateText(canvasGo.transform, "Subtitle", 40f, new Vector2(0f, 195f), "EL ÚLTIMO AMANECER", NeonGreen);
            MakeNeon(subtitle, NeonGreen, 0.3f);

            optionTexts = new TextMeshProUGUI[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                optionTexts[i] = CreateText(canvasGo.transform, "Option_" + options[i], 48f,
                    new Vector2(0f, 20f - i * 90f), options[i], Color.white);
            }

            optionsGroup = new GameObject("OptionsGroup");
            optionsGroup.transform.SetParent(canvasGo.transform, false);
            volumeText = CreateText(optionsGroup.transform, "Volume", 42f, new Vector2(0f, 20f), string.Empty, Color.white);

            hintText = CreateText(canvasGo.transform, "Hint", 30f, new Vector2(0f, -380f), string.Empty, HintWhite);
            optionsGroup.SetActive(false);
        }

        // Menu art from Resources/Menu/MainMenuBG, covering the screen without stretching.
        // Falls back to the old dark panel if the image is missing.
        static void CreateBackground(Transform parent)
        {
            GameObject background = new GameObject("Background", typeof(RectTransform));
            background.transform.SetParent(parent, false);
            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Texture2D art = Resources.Load<Texture2D>("Menu/MainMenuBG");
            if (art != null)
            {
                RawImage raw = background.AddComponent<RawImage>();
                raw.texture = art;
                raw.raycastTarget = false;
                AspectRatioFitter fitter = background.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = (float)art.width / art.height;
                return;
            }

            Image image = background.AddComponent<Image>();
            image.sprite = Resources.Load<Sprite>("Characters/Panel");
            image.color = new Color(0.02f, 0.03f, 0.035f, 1f);
            image.raycastTarget = false;
        }

        // Fluorescent look: a soft underlay of the same color acts as a glow (works with the mobile SDF shader).
        internal static void MakeNeon(TextMeshProUGUI label, Color glow, float strength)
        {
            label.extraPadding = true;
            Material material = label.fontMaterial;
            material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(glow.r, glow.g, glow.b, 0.55f));
            material.SetFloat(ShaderUtilities.ID_UnderlayDilate, strength);
            material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.85f);
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0f);
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, 0f);
            label.outlineColor = new Color32(12, 70, 12, 255);
            label.outlineWidth = 0.12f;
            label.UpdateMeshPadding();
        }

        TextMeshProUGUI CreateText(Transform parent, string name, float size, Vector2 position, string text, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1500f, 140f);
            rect.anchoredPosition = position;
            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = size;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.text = text;
            label.raycastTarget = false;
            return label;
        }

        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;

            if (optionsMode)
            {
                if (keyboard != null && (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)) ChangeVolume(-0.1f);
                if (keyboard != null && (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)) ChangeVolume(0.1f);
                if (gamepad != null)
                {
                    if (gamepad.dpad.left.wasPressedThisFrame) ChangeVolume(-0.1f);
                    if (gamepad.dpad.right.wasPressedThisFrame) ChangeVolume(0.1f);
                    float stick = gamepad.leftStick.ReadValue().x;
                    if (Time.unscaledTime >= nextNavTime && Mathf.Abs(stick) > 0.5f)
                    {
                        ChangeVolume(stick > 0f ? 0.1f : -0.1f);
                        nextNavTime = Time.unscaledTime + 0.2f;
                    }
                }

                bool back = keyboard != null
                    && (keyboard.jKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame);
                if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame) back = true;
                if (back) CloseOptions();
                return;
            }

            if (keyboard != null)
            {
                if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) Move(-1);
                if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame) Move(1);
            }
            if (gamepad != null)
            {
                if (gamepad.dpad.up.wasPressedThisFrame) Move(-1);
                if (gamepad.dpad.down.wasPressedThisFrame) Move(1);
                float stick = gamepad.leftStick.ReadValue().y;
                if (Time.unscaledTime >= nextNavTime && Mathf.Abs(stick) > 0.5f)
                {
                    Move(stick > 0f ? -1 : 1);
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
            index = (index + direction + options.Length) % options.Length;
            Refresh();
        }

        void Refresh()
        {
            for (int i = 0; i < optionTexts.Length; i++)
            {
                bool selected = i == index;
                optionTexts[i].color = selected ? SelectedGreen : Color.white;
                optionTexts[i].rectTransform.localScale = Vector3.one * (selected ? 1.1f : 1f);
            }

            hintText.text = "W / S o flechas  elegir      J / Espacio  confirmar";
        }

        void Confirm()
        {
            if (index == 0) SceneManager.LoadScene("CharacterSelect");
            else if (index == 1) OpenOptions();
            else QuitGame();
        }

        void OpenOptions()
        {
            optionsMode = true;
            optionsGroup.SetActive(true);
            foreach (TextMeshProUGUI text in optionTexts) text.gameObject.SetActive(false);
            hintText.text = "A / D  ajustar volumen      J / Espacio  volver";
            RefreshVolume();
        }

        void CloseOptions()
        {
            optionsMode = false;
            optionsGroup.SetActive(false);
            foreach (TextMeshProUGUI text in optionTexts) text.gameObject.SetActive(true);
            Refresh();
        }

        void ChangeVolume(float delta)
        {
            GameSettings.SetMasterVolume(GameSettings.MasterVolume + delta);
            RefreshVolume();
        }

        void RefreshVolume()
        {
            volumeText.text = "VOLUMEN MAESTRO: " + Mathf.RoundToInt(GameSettings.MasterVolume * 100f) + " %";
        }

        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
