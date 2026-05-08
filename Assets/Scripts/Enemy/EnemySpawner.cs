using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 적 프리팹을 일정 시간마다 생성하는 스폰 시스템.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float spawnInterval = 3.0f;
    [SerializeField] private int spawnCountPerInterval = 1;
    [SerializeField] private int maxAliveEnemyCount = 5;

    private float spawnTimer = 0.0f;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private int selectedSpawnPoint = -1;

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
        if(spawnOnStart == true)
        {
            SpawnEnemies(spawnCountPerInterval);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CleanupEnemyList();
        UpdateSpawnTimer();
    }

    /// <summary>
    /// 이미 사망한 Enemy를 리스트에서 정리.
    /// </summary>
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

    /// <summary>
    /// 스폰 타이머를 증가시키고 시간이 됐을 때 적 생성을 시도.
    /// </summary>
    void UpdateSpawnTimer()
    {
        if(CanSpawnMoreEnemy() == false)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if(spawnTimer >= spawnInterval)
        {
            spawnTimer = 0.0f;
            SpawnEnemies(spawnCountPerInterval);
        }
    }

    /// <summary>
    /// 현재 적을 더 생성할 수 있는지 확인.
    /// </summary>
    /// <returns></returns>
    bool CanSpawnMoreEnemy()
    {
        if(aliveEnemies.Count >= maxAliveEnemyCount)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 스폰 포인트 목록에서 하나를 랜덤으로 선택.
    /// </summary>
    /// <returns></returns>
    Transform GetRandomSpawnPoint()
    {
        // 배열의 크기가 5일 경우 0, 5가 인자로 들어감.
        // 실제로는 0 ~ (5-1) 사이의 값이 추출된다.
        int randomIndex = Random.Range(0, spawnPoints.Length);

        while(randomIndex == selectedSpawnPoint)
        {
            randomIndex = Random.Range(0, spawnPoints.Length);
        }

        selectedSpawnPoint = randomIndex;

        return spawnPoints[randomIndex];
    }

    /// <summary>
    /// 요청한 수만큼 적 생성을 시도.
    /// </summary>
    /// <param name="requestSpawnCount"></param>
    void SpawnEnemies(int requestSpawnCount)
    {
        if(CanSpawnMoreEnemy() == false)
        {
            return;
        }

        int availableSpawnCount = maxAliveEnemyCount - aliveEnemies.Count;
        int finalSpawnCount = Mathf.Min(requestSpawnCount, availableSpawnCount);

        for(int i=0; i<finalSpawnCount; ++i)
        {
            Transform spawnPoint = GetRandomSpawnPoint();
            if(spawnPoint != null)
            {
                SpawnEnemy(spawnPoint);
            }
        }
    }

    /// <summary>
    /// 지정한 스폰 위치에 Enemy를 생성.
    /// </summary>
    /// <param name="spawnPoint"></param>
    void SpawnEnemy(Transform spawnPoint)
    {
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        aliveEnemies.Add(spawnedEnemy);

        EnemyTargetDetector targetDetector = spawnedEnemy.GetComponent<EnemyTargetDetector>();
        if(targetDetector != null)
        {
            targetDetector.SetTarget(playerTarget);
        }    
    }

    public int AliveEnemyCount
    {
        get { return aliveEnemies.Count; }
    }
}
