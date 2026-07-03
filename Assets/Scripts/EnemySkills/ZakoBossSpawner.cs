using UnityEngine;
using System.Collections;

public class ZakoBossSpawner : MonoBehaviour
{
    [Header("雑魚ボスのプレハブ（複数登録可能）")]
    public GameObject[] zakoBossPrefabs;

    [Header("ラッシュの設定")]
    public float rushInterval = 45.0f;
    public int spawnCount = 20;
    public float spawnDelay = 0.1f;

    private float timer = 0f;

    void Start()
    {
        timer = rushInterval * 0.5f;
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = rushInterval;
            StartCoroutine(SpawnRushRoutine());
        }
    }

    IEnumerator SpawnRushRoutine()
    {
        if (zakoBossPrefabs == null || zakoBossPrefabs.Length == 0) yield break;

        Camera cam = Camera.main;
        if (cam == null) yield break;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        for (int i = 0; i < spawnCount; i++)
        {
            if (GameManager.instance != null && GameManager.instance.isGameOver) yield break;

            GameObject prefabToSpawn = zakoBossPrefabs[Random.Range(0, zakoBossPrefabs.Length)];

            float spawnX = Random.Range(-camWidth + 0.5f, camWidth - 0.5f);

            // ★修正：常に下から（画面の少し外側）出すように変更
            float spawnY = -camHeight - 1.5f;

            Vector2 spawnPos = new Vector2(spawnX, spawnY);

            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}