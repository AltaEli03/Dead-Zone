using System.Collections.Generic;
using DeadZone.Combat;
using DeadZone.Data;
using UnityEngine;

namespace DeadZone.Player
{
    public sealed class PlayerThrower : MonoBehaviour
    {
        [SerializeField] PlayerInputReader input;
        [SerializeField] PlayerAim aim;
        [SerializeField] PlayerConfig config;
        [SerializeField] Transform throwOrigin;
        [SerializeField] Throwable grenadePrefab;
        [SerializeField] Throwable molotovPrefab;
        [SerializeField] int grenadeCount;
        [SerializeField] int molotovCount;

        bool useMolotov;
        Collider2D playerCollider;
        readonly List<Throwable> activeThrows = new List<Throwable>();

        public int GrenadeCount => grenadeCount;
        public int MolotovCount => molotovCount;
        public bool UseMolotov => useMolotov;
        public string SecondaryText => "G " + grenadeCount + " · M " + molotovCount + (useMolotov ? "  [M]" : "  [G]");

        void Awake()
        {
            playerCollider = GetComponent<Collider2D>();
        }

        void OnEnable()
        {
            input.SecondaryPressed += ThrowSelected;
            input.SwitchSecondaryPressed += ToggleSelected;
        }

        void OnDisable()
        {
            input.SecondaryPressed -= ThrowSelected;
            input.SwitchSecondaryPressed -= ToggleSelected;
        }

        void ToggleSelected()
        {
            useMolotov = !useMolotov;
        }

        void ThrowSelected()
        {
            Throwable prefab = useMolotov ? molotovPrefab : grenadePrefab;
            int count = useMolotov ? molotovCount : grenadeCount;

            if (prefab == null || count <= 0)
            {
                useMolotov = !useMolotov;
                return;
            }

            activeThrows.RemoveAll(t => t == null);
            int maxActive = config != null ? config.maxActiveThrows : 2;
            if (activeThrows.Count >= maxActive) return;

            float facing = aim != null ? aim.Facing : 1f;
            Vector3 position = throwOrigin != null ? throwOrigin.position : transform.position + Vector3.up;
            float originX = throwOrigin != null ? Mathf.Abs(throwOrigin.localPosition.x) : 0.35f;
            position.x = transform.position.x + facing * originX;

            Throwable thrown = Instantiate(prefab, position, Quaternion.identity);
            activeThrows.Add(thrown);
            if (playerCollider != null) thrown.IgnoreCollisionWith(playerCollider);

            ThrowableData data = thrown.Data;
            float vx = data != null ? data.throwSpeedX : 8f;
            float vy = data != null ? data.throwSpeedY : 6f;
            thrown.Initialize(new Vector2(facing * vx, vy));

            if (useMolotov) molotovCount--;
            else grenadeCount--;
        }

        public void AddSecondary(bool molotov, int amount)
        {
            if (molotov) molotovCount += amount;
            else grenadeCount += amount;
        }
    }
}
