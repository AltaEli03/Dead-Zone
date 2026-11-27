using DeadZone.Data;
using UnityEngine;
using UnityEngine.Pool;

namespace DeadZone.Enemies
{
    public sealed class ZombieSpawner : MonoBehaviour
    {
        [SerializeField] EnemyHealth zombiePrefab;
        [SerializeField] EnemyData zombieData;
        [SerializeField] WaveData waveData;
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] bool spawnOnStart = true;

        ObjectPool<EnemyHealth> pool;
        int aliveCount;

        public int AliveCount => aliveCount;
        public bool WaveCleared { get; private set; }
        int WaveSize => waveData != null ? waveData.zombieCount : 0;

        void Awake()
        {
            pool = new ObjectPool<EnemyHealth>(CreateZombie, OnGet, OnRelease, DestroyZombie, true, WaveSize, WaveSize);
        }

        void Start()
        {
            if (spawnOnStart) SpawnWave();
        }

        public void SpawnWave()
        {
            WaveCleared = false;
            aliveCount = WaveSize;

            for (int i = 0; i < WaveSize; i++)
            {
                EnemyHealth zombie = pool.Get();
                Transform point = spawnPoints.Length > 0 ? spawnPoints[i % spawnPoints.Length] : transform;
                zombie.transform.position = point.position;
                zombie.Initialize(zombieData, this);
            }
        }

        public void OnZombieDied(EnemyHealth zombie)
        {
            aliveCount = Mathf.Max(0, aliveCount - 1);
            pool.Release(zombie);
            if (aliveCount == 0) WaveCleared = true;
        }

        EnemyHealth CreateZombie()
        {
            EnemyHealth zombie = Instantiate(zombiePrefab);
            zombie.gameObject.SetActive(false);
            return zombie;
        }

        void OnGet(EnemyHealth zombie)
        {
            zombie.gameObject.SetActive(true);
        }

        void OnRelease(EnemyHealth zombie)
        {
            zombie.gameObject.SetActive(false);
        }

        void DestroyZombie(EnemyHealth zombie)
        {
            if (zombie != null) Destroy(zombie.gameObject);
        }
    }
}
