using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadZone.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        PlayerInput playerInput;
        InputAction fireAction;

        public Vector2 Move { get; private set; }
        public bool FireHeld => fireAction != null && fireAction.IsPressed();

        public event Action JumpPressed;
        public event Action FirePressed;
        public event Action SecondaryPressed;
        public event Action SwitchSecondaryPressed;

        void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput != null && playerInput.actions != null)
                fireAction = playerInput.actions.FindAction("Fire");
        }

        public void OnMove(InputValue value)
        {
            Move = value.Get<Vector2>();
        }

        public void OnFire(InputValue value)
        {
            if (value.isPressed) FirePressed?.Invoke();
        }

        public void OnJump(InputValue value)
        {
            if (value.isPressed) JumpPressed?.Invoke();
        }

        public void OnSecondary(InputValue value)
        {
            if (value.isPressed) SecondaryPressed?.Invoke();
        }

        public void OnSwitchSecondary(InputValue value)
        {
            if (value.isPressed) SwitchSecondaryPressed?.Invoke();
        }
    }
}
