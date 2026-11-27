using UnityEngine;

namespace DeadZone.Core
{
    public static class GameSettings
    {
        const string VolumeKey = "dz_master_volume";
        const int TargetFrameRate = 60;

        public static float MasterVolume { get; private set; } = 0.8f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            MasterVolume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
            Apply();
        }

        public static void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumeKey, MasterVolume);
            PlayerPrefs.Save();
            Apply();
        }

        public static void Apply()
        {
            AudioListener.volume = MasterVolume;
            Application.targetFrameRate = TargetFrameRate;
            QualitySettings.vSyncCount = 0;
        }
    }
}
