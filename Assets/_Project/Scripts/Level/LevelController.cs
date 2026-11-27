using DeadZone.Enemies;
using DeadZone.Player;
using UnityEngine;

namespace DeadZone.Level
{
    public sealed class LevelController : MonoBehaviour
    {
        const string ControlsValue =
            "A/D mover · W/S apuntar · J disparar\n" +
            "S agacharse · S + Salto slide · Espacio saltar\n" +
            "K secundaria · Q cambiar · E habilidad · W/S escaleras";

        [SerializeField] PlayerHealth playerHealth;
        [SerializeField] ZombieSpawner finalWave;
        [SerializeField] Transform tutorialEnd;
        [SerializeField] float tutorialDuration = 15f;

        bool finalWaveStarted;
        bool finalWaveCleared;
        float tutorialTimer;
        float statusTimer;

        public bool ShowTutorial { get; private set; }
        public string ControlsText => ControlsValue;
        public string ObjectiveText { get; private set; }
        public string StatusMessage { get; private set; }
        public bool LevelComplete { get; private set; }
        public bool HasShotgun { get; private set; }

        void Start()
        {
            ShowTutorial = true;
            tutorialTimer = tutorialDuration;
            UpdateObjective();
        }

        void Update()
        {
            if (finalWaveStarted && !finalWaveCleared && finalWave != null && finalWave.WaveCleared)
            {
                finalWaveCleared = true;
                SetStatus("¡Oleada eliminada!");
            }

            if (ShowTutorial)
            {
                tutorialTimer -= Time.deltaTime;
                bool walkedPast = tutorialEnd != null
                    && playerHealth != null
                    && playerHealth.transform.position.x > tutorialEnd.position.x;
                if (tutorialTimer <= 0f || walkedPast) ShowTutorial = false;
            }

            if (statusTimer > 0f)
            {
                statusTimer -= Time.deltaTime;
                if (statusTimer <= 0f) StatusMessage = string.Empty;
            }

            UpdateObjective();
        }

        public void NotifyCheckpoint()
        {
            SetStatus("Punto de control alcanzado");
        }

        public void NotifyShotgunPicked()
        {
            HasShotgun = true;
            SetStatus("Escopeta conseguida");
        }

        public void StartFinalWave()
        {
            if (finalWaveStarted || LevelComplete) return;

            finalWaveStarted = true;
            if (finalWave != null) finalWave.SpawnWave();
            SetStatus("¡Oleada final!");
        }

        public void TryCompleteLevel()
        {
            if (LevelComplete) return;

            if (!HasShotgun)
            {
                SetStatus("Necesitas la escopeta");
                return;
            }

            if (!finalWaveCleared)
            {
                SetStatus("Elimina la oleada final");
                return;
            }

            LevelComplete = true;
            SetStatus("Nivel 1 completado");
        }

        void UpdateObjective()
        {
            if (LevelComplete) ObjectiveText = "Nivel 1 completado";
            else if (finalWaveStarted && !finalWaveCleared) ObjectiveText = "Elimina la oleada final";
            else if (finalWaveCleared && !HasShotgun) ObjectiveText = "Recoge la escopeta en la evacuación";
            else if (HasShotgun) ObjectiveText = "Llega a la salida";
            else ObjectiveText = "Llega al punto de evacuación";
        }

        void SetStatus(string message)
        {
            StatusMessage = message;
            statusTimer = 2.5f;
        }
    }
}
