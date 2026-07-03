using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnLocationType { CenterRadius, ScreenTopOutside, ScreenBottomOutside, ScreenTopBottomRandomOutside }

    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        public int unlockWave = 1;
        public int maxWave = 999;
    }

    [Header("基本の敵リスト（出現ウェーブ指定）")]
    public List<EnemySpawnData> normalEnemies = new List<EnemySpawnData>();
    public List<EnemySpawnData> bossEnemies = new List<EnemySpawnData>();

    [System.Serializable]
    public class DifficultySettings
    {
        public int baseSpawnCount = 5;
        public float extraEnemyPerWave = 1.5f;
        public float baseSpawnInterval = 1.0f;
        public float intervalDecrease = 0.02f;
        public float minSpawnInterval = 0.1f;
        public float hpDamageBonusPerWave = 0.1f;
        public int bossWaveInterval = 10;
        public int bossCountBase = 1;
        public int bossCountIncrease = 1;
    }

    [Header("難易度の自動計算設定")]
    public DifficultySettings difficulty = new DifficultySettings();

    [Header("ボスの大きさ設定")]
    public float bossScaleMultiplier = 1.0f;

    [System.Serializable]
    public class CustomWave
    {
        public int waveNumber = 100;
        public GameObject[] specificEnemies;
        public int customSpawnCount = 1;
        public float customSpawnInterval = 0.5f;
        public bool isBossLocation = true;
    }

    [Header("★特定のウェーブだけ特別に設定する（イベント用）")]
    public List<CustomWave> customWaves = new List<CustomWave>();

    private Camera mainCamera;
    private bool isSpawning = false;
    private float checkTimer = 0f;

    private static EnemySpawner instance;

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        mainCamera = Camera.main;

        if (GameManager.instance != null)
        {
            GameManager.instance.currentWave = PlayerPrefs.GetInt("CurrentWaveAtStart", 1);
        }

        StartWave();
    }

    void Update()
    {
        if (GameManager.instance == null || isSpawning) return;

        checkTimer += Time.deltaTime;
        if (checkTimer >= 1.0f)
        {
            checkTimer = 0f;

            // ★軽量化：FindGameObjectsWithTagを廃止し、名簿のリストを直接確認するだけ！
            bool hasLivingEnemy = false;
            for (int i = 0; i < Enemy.activeEnemies.Count; i++)
            {
                if (Enemy.activeEnemies[i].currentHP > 0)
                {
                    hasLivingEnemy = true;
                    break;
                }
            }

            if (!hasLivingEnemy)
            {
                GameManager.instance.NextWave();
                StartWave();
            }
        }
    }

    void StartWave()
    {
        if (GameManager.instance == null) return;
        StartCoroutine(SpawnWaveRoutine(GameManager.instance.currentWave));
    }

    IEnumerator SpawnWaveRoutine(int wave)
    {
        isSpawning = true;
        yield return new WaitForSeconds(1.5f);

        CustomWave cw = customWaves.Find(w => w.waveNumber == wave);

        int spawnCount = 0;
        float spawnInterval = 0f;
        bool isBossBehavior = false;
        SpawnLocationType location = SpawnLocationType.ScreenTopBottomRandomOutside;

        if (cw != null)
        {
            spawnCount = cw.customSpawnCount;
            spawnInterval = cw.customSpawnInterval;
            isBossBehavior = cw.isBossLocation;
            if (isBossBehavior) location = SpawnLocationType.ScreenTopOutside;
        }
        else
        {
            isBossBehavior = (wave % difficulty.bossWaveInterval == 0);

            if (isBossBehavior)
            {
                int bossTimes = wave / difficulty.bossWaveInterval;
                spawnCount = difficulty.bossCountBase + (bossTimes - 1) * difficulty.bossCountIncrease;
                spawnInterval = difficulty.baseSpawnInterval;
                location = SpawnLocationType.ScreenTopOutside;
            }
            else
            {
                spawnCount = difficulty.baseSpawnCount + Mathf.FloorToInt(wave * difficulty.extraEnemyPerWave);
                spawnInterval = Mathf.Max(difficulty.minSpawnInterval, difficulty.baseSpawnInterval - (wave * difficulty.intervalDecrease));
            }
        }

        List<GameObject> availableNormals = new List<GameObject>();
        foreach (var e in normalEnemies)
        {
            if (e.enemyPrefab != null && wave >= e.unlockWave && wave <= e.maxWave)
            {
                availableNormals.Add(e.enemyPrefab);
            }
        }

        List<GameObject> availableBosses = new List<GameObject>();
        foreach (var e in bossEnemies)
        {
            if (e.enemyPrefab != null && wave >= e.unlockWave && wave <= e.maxWave)
            {
                availableBosses.Add(e.enemyPrefab);
            }
        }

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefabToSpawn = null;

            if (cw != null && cw.specificEnemies != null && cw.specificEnemies.Length > 0)
                prefabToSpawn = cw.specificEnemies[Random.Range(0, cw.specificEnemies.Length)];
            else if (isBossBehavior && availableBosses.Count > 0)
                prefabToSpawn = availableBosses[Random.Range(0, availableBosses.Count)];
            else if (availableNormals.Count > 0)
                prefabToSpawn = availableNormals[Random.Range(0, availableNormals.Count)];

            if (prefabToSpawn != null)
            {
                Vector2 spawnPos = CalculateSpawnPosition(location, isBossBehavior ? 3f : 1f);
                GameObject enemyObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

                if (isBossBehavior)
                {
                    enemyObj.transform.localScale = enemyObj.transform.localScale * bossScaleMultiplier;
                }

                Enemy enemyScript = enemyObj.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    float buffMultiplier = 1.0f + (wave * difficulty.hpDamageBonusPerWave);
                    enemyScript.maxHP *= buffMultiplier;
                    enemyScript.currentHP = enemyScript.maxHP;
                    enemyScript.touchDamage *= buffMultiplier;
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private Vector2 CalculateSpawnPosition(SpawnLocationType type, float offsetOrRadius)
    {
        Vector2 spawnPos = Vector2.zero;
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return Vector2.zero;

        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        float topWorld = mainCamera.transform.position.y + cameraHeight;
        float bottomWorld = mainCamera.transform.position.y - cameraHeight;
        float leftWorld = mainCamera.transform.position.x - cameraWidth;
        float rightWorld = mainCamera.transform.position.x + cameraWidth;

        switch (type)
        {
            case SpawnLocationType.CenterRadius: spawnPos = (Vector2)mainCamera.transform.position + Random.insideUnitCircle * offsetOrRadius; break;
            case SpawnLocationType.ScreenTopOutside: spawnPos.x = Random.Range(leftWorld, rightWorld); spawnPos.y = topWorld + offsetOrRadius; break;
            case SpawnLocationType.ScreenBottomOutside: spawnPos.x = Random.Range(leftWorld, rightWorld); spawnPos.y = bottomWorld - offsetOrRadius; break;
            case SpawnLocationType.ScreenTopBottomRandomOutside: spawnPos.x = Random.Range(leftWorld, rightWorld); spawnPos.y = (Random.value > 0.5f) ? (topWorld + offsetOrRadius) : (bottomWorld - offsetOrRadius); break;
        }
        return spawnPos;
    }
}