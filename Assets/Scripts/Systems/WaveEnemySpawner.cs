using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaveEnemyEntry
{
    public GameObject enemyPrefab;
    public int spawnWeight = 1;

    public bool IsValid()
    {
        if(enemyPrefab == null)
        {
            return false;
        }

        if(spawnWeight <= 0)
        {
            return false;
        }

        return true;
    }
}

[System.Serializable]
public class EnemyWaveSettings
{
    public string waveName = "Wave";
    public float waveDuration = 30.0f;
    public float spawnInterval = 3.0f;
    public int spawnCountPerInterval = 1;
    public int maxAliveEnemyCount = 5;
    public WaveEnemyEntry[] enemyEntries;
}

public class WaveEnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private EnemyWaveSettings[] waves;
    [SerializeField] private bool startOnPlay = true;
    [SerializeField] private bool spawnOnWaveStart = true;
    [SerializeField] private bool stopSpawningAfterLastWave = true;

    [SerializeField] private int currentWaveIndex = -1;
    [SerializeField] private float currentWaveTimer = 0.0f;
    [SerializeField] private float spawnTimer = 0.0f;
    [SerializeField] private bool isSpawning = false;

    [SerializeField] private bool allWavesFinished = false;

    private List<GameObject> aliveEnemies = new List<GameObject>();

    private void Awake()
    {
        if(playerTarget == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if(playerObject != null)
            {
                playerTarget = playerObject.transform;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(startOnPlay == true)
        {
            // 첫번째 웨이브 시작.
            StartFirstWave();
        }
    }

    // Update is called once per frame
    void Update()
    {
        CleanupEnemyList();

        if(isSpawning == false)
        {
            return;
        }

        UpdateWaveTimer();
        UpdateSpawnTimer();
    }

    public void StartFirstWave()
    {
        currentWaveIndex = -1;
        currentWaveTimer = 0.0f;
        spawnTimer = 0.0f;
        allWavesFinished = false;
        isSpawning = true;

        // 웨이브 시작 함수 호출.
        StartWave(0);
    }

    void StartWave(int waveIndex)
    {
        if(waveIndex >= waves.Length)
        {
            FinishAllWaves();
            return;
        }

        currentWaveIndex = waveIndex;
        currentWaveTimer = 0.0f;
        spawnTimer = 0.0f;

        EnemyWaveSettings currentWave = waves[currentWaveIndex];

        if(spawnOnWaveStart == true && currentWave != null)
        {
            // 적을 생성하는 함수 호출.
            SpawnEnemies(waves[currentWaveIndex].spawnCountPerInterval);
        }
    }

    void UpdateWaveTimer()
    {
        currentWaveTimer += Time.deltaTime;
        if(currentWaveTimer >= waves[currentWaveIndex].waveDuration)
        {
            StartWave(currentWaveIndex + 1);
        }
    }

    void UpdateSpawnTimer()
    {
        if(CanSpawnMoreEnemy() == false)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if(spawnTimer >= waves[currentWaveIndex].spawnInterval)
        {
            spawnTimer = 0.0f;

            // 적 생성.
            SpawnEnemies(waves[currentWaveIndex].spawnCountPerInterval);
        }
    }

    void CleanupEnemyList()
    {
        for(int i=aliveEnemies.Count-1; i>=0; --i)
        {
            GameObject enemyObject = aliveEnemies[i];

            if(enemyObject == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }

    bool CanSpawnMoreEnemy()
    {
        if(aliveEnemies.Count >= waves[currentWaveIndex].maxAliveEnemyCount)
        {
            return false;
        }

        return true;
    }

    void SpawnEnemies(int requestedSpawnCount)
    {
        if(CanSpawnMoreEnemy() == false)
        {
            return;
        }

        int availableSpawnCount = waves[currentWaveIndex].maxAliveEnemyCount - aliveEnemies.Count;
        int safeRequestedSpawnCount = Mathf.Max(0, requestedSpawnCount);
        int finalSpawnCount = Mathf.Min(safeRequestedSpawnCount, availableSpawnCount);

        for(int i=0; i<finalSpawnCount; ++i)
        {
            // 스폰 위치를 랜덤하게 추출.
            Transform spawnPoint = GetRandomSpawnPoint();

            // 적 프리팹을 랜덤하게 추출.
            GameObject enemyPrefab = GetRandomEnemyPrefabByWeight(waves[currentWaveIndex]);

            // 적 생성.
            SpawnEnemyAt(enemyPrefab, spawnPoint);
        }
    }

    Transform GetRandomSpawnPoint()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);

        return spawnPoints[randomIndex];
    }

    GameObject GetRandomEnemyPrefabByWeight(EnemyWaveSettings wave)
    {
        int totalWeight = 0;

        for (int i = 0; i < wave.enemyEntries.Length; ++i)
        {
            if (wave.enemyEntries[i].IsValid() == true)
            {
                totalWeight += wave.enemyEntries[i].spawnWeight;
            }
        }

        if(totalWeight <= 0)
        {
            return null;
        }

        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        for(int i=0; i < wave.enemyEntries.Length; ++i)
        {
            WaveEnemyEntry entry = wave.enemyEntries[i];

            if(entry.IsValid() == false)
            {
                continue;
            }

            cumulativeWeight += entry.spawnWeight;

            if(randomValue < cumulativeWeight)
            {
                return entry.enemyPrefab;
            }
        }

        return null;
    }

    void SpawnEnemyAt(GameObject enemyPrefab, Transform spawnPoint)
    {
        GameObject spawnEnemy = Instantiate(enemyPrefab, spawnPoint.position,
            spawnPoint.rotation);
        aliveEnemies.Add(spawnEnemy);

        EnemyTargetDetector targetDetector = spawnEnemy.GetComponent<EnemyTargetDetector>();

        if(targetDetector != null && playerTarget != null)
        {
            targetDetector.SetTarget(playerTarget);
        }
    }

    void FinishAllWaves()
    {
        allWavesFinished = true;

        if(stopSpawningAfterLastWave == true)
        {
            isSpawning = false;
        }
    }
}
