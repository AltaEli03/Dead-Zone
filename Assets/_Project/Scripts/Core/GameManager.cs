using DeadZone.Data;
using DeadZone.Enemies;
using DeadZone.Level;
using DeadZone.Player;
using DeadZone.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DeadZone.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] PlayerHealth playerHealth;
        [SerializeField] ZombieSpawner spawner;
        [SerializeField] LevelController levelController;

        void Start()
        {
            if (levelController == null || playerHealth == null) return;

            if (PlayerSelection.Selected != null)
            {
                PlayerCharacterApplier.Apply(playerHealth, PlayerSelection.Selected);
                return;
            }

            CharacterData[] characters = Resources.LoadAll<CharacterData>("Characters");
            if (characters == null || characters.Length == 0) return;

            PlayerInput input = playerHealth.GetComponent<PlayerInput>();
            if (input != null) input.enabled = false;

            Time.timeScale = 0f;
            CharacterSelectUI ui = new GameObject("CharacterSelectUI").AddComponent<CharacterSelectUI>();
            ui.Begin(characters, OnCharacterChosen, input);
        }

        void OnCharacterChosen(CharacterData character, PlayerInput input)
        {
            PlayerCharacterApplier.Apply(playerHealth, character);
            Time.timeScale = 1f;
            if (input != null) input.enabled = true;
        }

        void Update()
        {
            if (playerHealth == null) return;

            bool won = levelController != null
                ? levelController.LevelComplete
                : spawner != null && spawner.WaveCleared;

            if (!playerHealth.IsGameOver && !won) return;

            bool restart = (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

            if (restart) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
