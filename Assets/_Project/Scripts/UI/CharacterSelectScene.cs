using DeadZone.Core;
using DeadZone.Data;
using DeadZone.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DeadZone.UI
{
    public sealed class CharacterSelectScene : MonoBehaviour
    {
        void Start()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameSettings.Apply();

            CharacterData[] characters = Resources.LoadAll<CharacterData>("Characters");
            if (characters == null || characters.Length == 0)
            {
                SceneManager.LoadScene("Level01");
                return;
            }

            CharacterSelectUI ui = new GameObject("CharacterSelectUI").AddComponent<CharacterSelectUI>();
            ui.Begin(characters, OnCharacterChosen, null);
        }

        void OnCharacterChosen(CharacterData character, PlayerInput input)
        {
            PlayerSelection.Selected = character;
            SceneManager.LoadScene("Level01");
        }
    }
}
