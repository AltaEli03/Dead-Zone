using DeadZone.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeadZone.UI
{
    public sealed class PauseMenuUI : MonoBehaviour
    {
        static readonly string[] PauseOptions = { "REANUDAR", "OPCIONES", "REINICIAR NIVEL", "MENÚ PRINCIPAL" };
        static readonly string[] DefeatOptions = { "REINTENTAR", "MENÚ PRINCIPAL" };

        PlayerInput playerInput;
        Canvas canvas;
        TextMeshProUGUI titleText;
        TextMeshProUGUI hintText;
        TextMeshProUGUI volumeText;
        TextMeshProUGUI[] optionTexts;
        GameObject optionsGroup;
        bool paused;
        bool defeat;
        bool optionsMode = true;
        int index;
        float nextNavTime;

        public void Begin(PlayerInput input)
        {
            playerInput = input;
            Build();
            canvas.gameObject.SetActive(false);
        }

        void Build()
        {
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                TMP_Text sample = FindAnyObjectByType<TMP_Text>();
                if (sample != null) font = sample.font;
            }

            GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
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
            image.color = new Color(0.02f, 0.03f, 0.035f, 0.92f);
            image.raycastTarget = false;

            titleText = CreateText(canvasGo.transform, "Title", 72f, new Vector2(0f, 280f), "PAUSA", new Color(0.55f, 1f, 0.65f));
            optionTexts = new TextMeshProUGUI[4];
            for (int i = 0; i < optionTexts.Length; i++)
            {
                optionTexts[i] = CreateText(canvasGo.transform, "Option_" + i, 40f,
                    new Vector2(0f, 80f - i * 80f), string.Empty, Color.white);
            }

            optionsGroup = new GameObject("OptionsGroup");
            optionsGroup.transform.SetParent(canvasGo.transform, false);
            volumeText = CreateText(optionsGroup.transform, "Volume", 42f, new Vector2(0f, 80f), string.Empty, Color.white);
            volumeText.transform.SetParent(optionsGroup.transform, false);
            hintText = CreateText(canvasGo.transform, "Hint", 26f, new Vector2(0f, -360f), string.Empty, new Color(0.7f, 0.75f, 0.75f));
            optionsGroup.SetActive(false);
        }

        TextMeshProUGUI CreateText(Transform parent, string name, float size, Vector2 position, string text, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1500f, 120f);
            rect.anchoredPosition = position;
            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            if (label.font == null)
            {
                TMP_Text sample = FindAnyObjectByType<TMP_Text>();
                if (sample != null) label.font = sample.font;
            }
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
            bool escape = keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
            if (gamepad != null && gamepad.startButton.wasPressedThisFrame) escape = true;

            if (!paused && !defeat)
            {
                if (escape) PauseGame();
                return;
            }

            if (escape && !defeat)
            {
                if (optionsMode) CloseOptions();
                else ResumeGame();
                return;
            }

            if (optionsMode)
            {
                if (keyboard != null)
                {
                    if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame) ChangeVolume(-0.1f);
                    if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame) ChangeVolume(0.1f);
                }
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
                    && (keyboard.jKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame);
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

        void PauseGame()
        {
            paused = true;
            index = 0;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            if (playerInput != null) playerInput.enabled = false;
            titleText.text = "PAUSA";
            optionsGroup.SetActive(false);
            optionsMode = false;
            canvas.gameObject.SetActive(true);
            Refresh();
        }

        void ResumeGame()
        {
            paused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (playerInput != null) playerInput.enabled = true;
            canvas.gameObject.SetActive(false);
        }

        public void ShowDefeat()
        {
            if (defeat) return;

            defeat = true;
            paused = true;
            index = 0;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            if (playerInput != null) playerInput.enabled = false;
            titleText.text = "HAS CAÍDO";
            titleText.color = new Color(0.95f, 0.3f, 0.25f);
            optionsGroup.SetActive(false);
            optionsMode = false;
            canvas.gameObject.SetActive(true);
            Refresh();
        }

        void Move(int direction)
        {
            int count = defeat ? DefeatOptions.Length : PauseOptions.Length;
            index = (index + direction + count) % count;
            Refresh();
        }

        void Refresh()
        {
            string[] labels = defeat ? DefeatOptions : PauseOptions;
            for (int i = 0; i < optionTexts.Length; i++)
            {
                bool visible = i < labels.Length;
                optionTexts[i].gameObject.SetActive(visible);
                if (!visible) continue;
                optionTexts[i].text = labels[i];
                optionTexts[i].color = i == index ? new Color(1f, 0.85f, 0.35f) : Color.white;
            }
            hintText.text = "W / S o flechas  elegir      J / Espacio  confirmar";
        }

        void OpenOptions()
        {
            optionsMode = true;
            optionsGroup.SetActive(true);
            foreach (TextMeshProUGUI text in optionTexts) text.gameObject.SetActive(false);
            titleText.text = "OPCIONES";
            hintText.text = "A / D  ajustar volumen      J / Espacio  volver";
            RefreshVolume();
        }

        void CloseOptions()
        {
            optionsMode = false;
            optionsGroup.SetActive(false);
            foreach (TextMeshProUGUI text in optionTexts) text.gameObject.SetActive(true);
            titleText.text = defeat ? "HAS CAÍDO" : "PAUSA";
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

        void Confirm()
        {
            if (defeat)
            {
                if (index == 0) RestartLevel();
                else LoadMainMenu();
                return;
            }

            if (index == 0) ResumeGame();
            else if (index == 1) OpenOptions();
            else if (index == 2) RestartLevel();
            else LoadMainMenu();
        }

        void RestartLevel()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (playerInput != null) playerInput.enabled = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void LoadMainMenu()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (playerInput != null) playerInput.enabled = true;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
