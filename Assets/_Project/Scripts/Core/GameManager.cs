using DeadZone.Enemies;
using DeadZone.Level;
using DeadZone.Player;
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
