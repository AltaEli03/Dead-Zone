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

            GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(canvasGo.transform, false);
            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = background.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("Characters/Panel");
            image.color = new Color(0.02f, 0.03f, 0.035f, 1f);
            image.raycastTarget = false;

            CreateText(canvasGo.transform, "Title", 96f, new Vector2(0f, 330f), "DEAD ZONE", new Color(0.55f, 1f, 0.65f));
            CreateText(canvasGo.transform, "Subtitle", 36f, new Vector2(0f, 235f), "EL ÚLTIMO AMANECER", new Color(0.9f, 0.9f, 0.9f));

            optionTexts = new TextMeshProUGUI[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                optionTexts[i] = CreateText(canvasGo.transform, "Option_" + options[i], 48f,
                    new Vector2(0f, 20f - i * 90f), options[i], Color.white);
            }

            optionsGroup = new GameObject("OptionsGroup");
            optionsGroup.transform.SetParent(canvasGo.transform, false);
            volumeText = CreateText(optionsGroup.transform, "Volume", 42f, new Vector2(0f, 20f), string.Empty, Color.white);

            hintText = CreateText(canvasGo.transform, "Hint", 26f, new Vector2(0f, -380f), string.Empty, new Color(0.7f, 0.75f, 0.75f));
            optionsGroup.SetActive(false);
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
                optionTexts[i].color = i == index ? new Color(1f, 0.85f, 0.35f) : Color.white;

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
