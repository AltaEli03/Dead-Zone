using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeadZone.Core
{
    public sealed class AudioManager : MonoBehaviour
    {
        public enum Sfx
        {
            Shot,
            Hit,
            Explosion,
            Hurt,
            Pickup,
            Heal,
            Scream,
            Zombie,
            Core
        }

        static AudioManager instance;

        AudioSource sfxSource;
        AudioSource musicSource;
        AudioClip[] sfxClips;
        AudioClip menuMusic;
        AudioClip levelMusic;
        AudioClip victoryMusic;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (instance != null) return;

            GameObject go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            go.AddComponent<AudioManager>();
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 0.85f;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.32f;

            LoadClips();
            GameSettings.Apply();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void LoadClips()
        {
            sfxClips = new AudioClip[]
            {
                Resources.Load<AudioClip>("Audio/shot"),
                Resources.Load<AudioClip>("Audio/hit"),
                Resources.Load<AudioClip>("Audio/explosion"),
                Resources.Load<AudioClip>("Audio/hurt"),
                Resources.Load<AudioClip>("Audio/pickup"),
                Resources.Load<AudioClip>("Audio/heal"),
                Resources.Load<AudioClip>("Audio/scream"),
                Resources.Load<AudioClip>("Audio/zombie"),
                Resources.Load<AudioClip>("Audio/core")
            };

            menuMusic = Resources.Load<AudioClip>("Audio/music_menu");
            levelMusic = Resources.Load<AudioClip>("Audio/music_level");
            victoryMusic = Resources.Load<AudioClip>("Audio/music_victory");
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this) instance = null;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (musicSource == null) return;

            AudioClip clip;
            float pitch = 1f;

            if (scene.name == "MainMenu" || scene.name == "CharacterSelect")
            {
                clip = menuMusic;
            }
            else if (scene.name == "Victory")
            {
                clip = victoryMusic;
            }
            else
            {
                clip = levelMusic;
                int level = ParseLevel(scene.name);
                pitch = level > 0 ? 1f + (level - 1) * 0.025f : 1f;
            }

            if (clip == null) return;

            musicSource.pitch = pitch;
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.Play();
        }

        static int ParseLevel(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || !sceneName.StartsWith("Level")) return 0;
            return int.TryParse(sceneName.Substring(5), out int level) ? level : 0;
        }

        public static void PlaySfx(Sfx sfx)
        {
            if (instance == null || instance.sfxClips == null) return;

            int index = (int)sfx;
            if (index < 0 || index >= instance.sfxClips.Length) return;

            AudioClip clip = instance.sfxClips[index];
            if (clip != null) instance.sfxSource.PlayOneShot(clip, 0.85f);
        }
    }
}
