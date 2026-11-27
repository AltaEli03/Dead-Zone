using System;
using DeadZone.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DeadZone.UI
{
    public sealed class CharacterSelectUI : MonoBehaviour
    {
        CharacterData[] characters;
        Action<CharacterData, PlayerInput> onConfirm;
        PlayerInput playerInput;
        TMP_FontAsset font;
        TextMeshProUGUI nameText;
        TextMeshProUGUI statsText;
        TextMeshProUGUI abilityText;
        Image preview;
        int index;
        float nextNavTime;

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

            Build();
            Refresh();
        }

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

            GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(canvasGo.transform, false);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            Image backgroundImage = background.GetComponent<Image>();
            backgroundImage.sprite = Resources.Load<Sprite>("Characters/Panel");
            backgroundImage.color = new Color(0.03f, 0.04f, 0.05f, 1f);
            backgroundImage.raycastTarget = false;

            CreateText(canvasGo.transform, "Title", 48f, new Vector2(0f, 440f), "ELIGE TU SUPERVIVIENTE", Color.white);
            nameText = CreateText(canvasGo.transform, "Name", 72f, new Vector2(0f, 350f), string.Empty, new Color(1f, 0.85f, 0.35f));
            statsText = CreateText(canvasGo.transform, "Stats", 34f, new Vector2(0f, -150f), string.Empty, Color.white);
            abilityText = CreateText(canvasGo.transform, "Ability", 30f, new Vector2(0f, -250f), string.Empty, new Color(0.6f, 0.9f, 1f));
            CreateText(canvasGo.transform, "Hint", 26f, new Vector2(0f, -430f),
                "A / D  o  ← / →  elegir      J / Espacio  confirmar", new Color(0.7f, 0.7f, 0.7f));

            GameObject previewGo = new GameObject("Preview", typeof(RectTransform), typeof(Image));
            previewGo.transform.SetParent(canvasGo.transform, false);
            RectTransform previewRect = previewGo.GetComponent<RectTransform>();
            previewRect.anchoredPosition = new Vector2(0f, 60f);
            previewRect.sizeDelta = new Vector2(180f, 270f);
            preview = previewGo.GetComponent<Image>();
            preview.preserveAspect = true;
            preview.raycastTarget = false;
        }

        TextMeshProUGUI CreateText(Transform parent, string name, float size, Vector2 position, string text, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1500f, 120f);
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
            if (characters == null || characters.Length == 0) return;

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
            index = (index + direction + characters.Length) % characters.Length;
            Refresh();
        }

        void Refresh()
        {
            CharacterData character = characters[index];
            nameText.text = character.displayName;
            statsText.text = "VIDA " + character.config.maxHealth + "    VELOCIDAD " + character.config.moveSpeed.ToString("0.0")
                + "\n" + character.description;
            abilityText.text = character.abilityDescription;
            preview.sprite = character.bodySprite;
        }

        void Confirm()
        {
            CharacterData character = characters[index];
            onConfirm?.Invoke(character, playerInput);
            Destroy(gameObject);
        }
    }
}
