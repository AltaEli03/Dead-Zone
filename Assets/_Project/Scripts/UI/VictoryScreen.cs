using DeadZone.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DeadZone.UI
{
    public sealed class VictoryScreen : MonoBehaviour
    {
        [SerializeField] TMP_Text message;

        void Start()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            PlayerSelection.Selected = null;
            if (message != null)
                message.text = "Has destruido el núcleo de investigación.\n\nPulsa R o Start para volver al inicio";
        }

        void Update()
        {
            bool restart = (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

            if (restart) SceneManager.LoadScene("MainMenu");
        }
    }
}
