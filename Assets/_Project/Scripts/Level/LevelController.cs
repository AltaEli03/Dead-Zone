using System.Collections;
using DeadZone.Enemies;
using DeadZone.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        [Header("Campaign")]
        [SerializeField] string levelName = "Nivel 1";
        [SerializeField] string objectiveText = "Llega al punto de evacuación";
        [SerializeField] string objectiveCompleteText = "Llega a la salida";
        [SerializeField] bool requiresShotgun = true;
        [SerializeField] bool requiresFinalWave = true;
        [SerializeField] bool requiresObjective;
        [SerializeField] CoreBoss coreBoss;
        [SerializeField] string nextSceneName;
        [SerializeField] float nextSceneDelay = 2.5f;

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
        public bool ObjectiveComplete { get; private set; }
        public string CompletionMessage { get; private set; } = string.Empty;

        void Start()
        {
            ShowTutorial = true;
            tutorialTimer = tutorialDuration;
            UpdateObjective();
        }

        void Update()
        {
            if (!LevelComplete && coreBoss != null && coreBoss.IsDead) CompleteLevel();

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
        }

        public void NotifyObjectiveComplete()
        {
            if (ObjectiveComplete) return;
            ObjectiveComplete = true;
            SetStatus("Objetivo completado");
        }

        public void ShowStatus(string message)
        {
            SetStatus(message);
        }

        public void StartFinalWave()
        {
            if (finalWaveStarted || LevelComplete) return;

            finalWaveStarted = true;
            if (finalWave != null) finalWave.SpawnWave();
            SetStatus("¡Oleada final!");
        }

        public void NotifyCoreDestroyed()
        {
            CompleteLevel();
        }

        public void TryCompleteLevel()
        {
            if (LevelComplete) return;

            if (requiresShotgun && !HasShotgun)
            {
                SetStatus("Necesitas la escopeta");
                return;
            }

            if (requiresFinalWave && !finalWaveCleared)
            {
                SetStatus("Elimina la oleada final");
                return;
            }

            if (requiresObjective && !ObjectiveComplete)
            {
                SetStatus("Completa el objetivo");
                return;
            }

            if (coreBoss != null && !coreBoss.IsDead)
            {
                SetStatus("Destruye el núcleo");
                return;
            }

            CompleteLevel();
        }

        void CompleteLevel()
        {
            if (LevelComplete) return;

            LevelComplete = true;
            CompletionMessage = coreBoss != null ? "CAMPAÑA COMPLETADA" : levelName + " completado";
            SetStatus(CompletionMessage);

            if (!string.IsNullOrEmpty(nextSceneName)) StartCoroutine(LoadNextScene());
        }

        IEnumerator LoadNextScene()
        {
            yield return new WaitForSeconds(nextSceneDelay);
            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
        }

        void UpdateObjective()
        {
            if (LevelComplete)
            {
                ObjectiveText = CompletionMessage;
                return;
            }

            if (coreBoss != null)
            {
                ObjectiveText = "NÚCLEO " + Mathf.CeilToInt(coreBoss.CurrentHealth) + "/" + Mathf.CeilToInt(coreBoss.MaxHealth)
                    + (coreBoss.IsOpen ? " · VULNERABLE" : " · PROTEGIDO");
                return;
            }

            if (requiresShotgun && !HasShotgun)
            {
                ObjectiveText = finalWaveCleared ? "Recoge la escopeta en la evacuación" : objectiveText;
                return;
            }

            if (requiresFinalWave && !finalWaveStarted)
            {
                ObjectiveText = objectiveText;
                return;
            }

            if (requiresFinalWave && finalWaveStarted && !finalWaveCleared)
            {
                ObjectiveText = "Elimina la oleada final";
                return;
            }

            if (requiresObjective && !ObjectiveComplete)
            {
                ObjectiveText = objectiveText;
                return;
            }

            ObjectiveText = objectiveCompleteText;
        }

        void SetStatus(string message)
        {
            StatusMessage = message;
            statusTimer = 2.5f;
        }
    }
}
