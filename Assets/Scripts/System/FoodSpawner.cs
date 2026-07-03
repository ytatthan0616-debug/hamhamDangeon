using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("エサのプレハブ（0:種， 1:野菜， 2:肉 の順番で入れてください）")]
    public GameObject[] foodPrefabs;

    public float spawnInterval = 3f;
    public int maxFoodCount = 15;

    public float minX = -2.5f;
    public float maxX = 2.5f;
    public float minY = -4.5f;
    public float maxY = -3.0f;

    public ParticleSystem spawnEffectPrefab;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // ★追加：基本の出現間隔を代入
        float currentInterval = spawnInterval;

        // ★追加：広告バフがONなら、出現間隔を半分（＝スピード2倍）にする
        if (GameManager.instance != null && GameManager.instance.isAdBuffActive)
        {
            currentInterval = spawnInterval / 2.0f;
        }

        // ★修正：固定の spawnInterval ではなく、計算された currentInterval で判定する
        if (timer >= currentInterval)
        {
            GameObject[] currentFoods = GameObject.FindGameObjectsWithTag("Food");

            if (currentFoods.Length < maxFoodCount)
            {
                SpawnFood();
            }

            timer = 0f;
        }
    }

    void SpawnFood()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        Vector2 spawnPos = new Vector2(randomX, randomY);

        // エサが1つも登録されていない場合はエラーを防ぐために処理を終了
        if (foodPrefabs == null || foodPrefabs.Length == 0) return;

        GameObject selectedPrefab = null;

        // ★修正：0〜99のランダムな数字を出して確率を分ける
        int rand = Random.Range(0, 100);

        if (rand < 50)
        {
            // 0〜49（50%の確率）: 種（配列の0番目）
            selectedPrefab = foodPrefabs[0];
        }
        else if (rand < 75)
        {
            // 50〜74（25%の確率）: 野菜（配列の1番目）
            // ※安全対策：配列が足りない場合は0番目を出す
            selectedPrefab = (foodPrefabs.Length > 1) ? foodPrefabs[1] : foodPrefabs[0];
        }
        else
        {
            // 75〜99（25%の確率）: 肉（配列の2番目）
            // ※安全対策：配列が足りない場合は0番目を出す
            selectedPrefab = (foodPrefabs.Length > 2) ? foodPrefabs[2] : foodPrefabs[0];
        }

        GameObject newFood = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

        if (spawnEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(spawnEffectPrefab, spawnPos, Quaternion.identity);

            FoodBehavior food = newFood.GetComponent<FoodBehavior>();
            if (food != null)
            {
                var mainModule = effect.main;
                mainModule.startColor = food.effectColor;
            }

            Destroy(effect.gameObject, 1.0f);
        }
    }
}