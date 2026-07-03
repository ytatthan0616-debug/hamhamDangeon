using UnityEngine;
using System.Collections;

public class BossRandomSlashSkill : MonoBehaviour
{
    [Header("ランダム斬撃設定")]
    public float skillCooldown = 6.0f;
    public GameObject slashPrefab;
    public float damage = 15f;
    public int baseSlashCount = 3;

    private float timer = 0f;
    private Enemy enemyScript;
    private bool canUseSkill = false;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        timer = 3.0f;
        if (GetComponent<BossDescending>() == null) canUseSkill = true;
    }

    public void StartSkillRoutine() { canUseSkill = true; }

    void Update()
    {
        if (!canUseSkill) return;
        if (enemyScript != null && enemyScript.currentHP <= 0) return;
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = skillCooldown;
            StartCoroutine(SpawnRandomSlashes());
        }
    }

    IEnumerator SpawnRandomSlashes()
    {
        if (slashPrefab == null) yield break;
        if (TickerMessageUI.instance != null) TickerMessageUI.instance.ShowBossSkillMessage();

        int spawnCount = baseSlashCount;

        // ★HP割合やWAVE数に応じて斬撃の数を増やす
        if (enemyScript != null)
        {
            float hpRate = enemyScript.currentHP / enemyScript.maxHP;
            if (hpRate <= 0.6f) spawnCount += 2; // HP60%以下で+2個
            if (hpRate <= 0.3f) spawnCount += 3; // HP30%以下でさらに+3個
        }
        if (GameManager.instance != null && GameManager.instance.currentWave >= 50) spawnCount += 2;

        Camera cam = Camera.main;
        if (cam == null) yield break;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        for (int i = 0; i < spawnCount; i++)
        {
            // 画面内のランダムな位置
            float randX = Random.Range(-camWidth + 0.5f, camWidth - 0.5f);
            float randY = Random.Range(-camHeight + 0.5f, camHeight - 0.5f);
            Vector3 spawnPos = new Vector3(cam.transform.position.x + randX, cam.transform.position.y + randY, 0);

            // 少し角度をランダムにして出現
            float randomRotation = Random.Range(0f, 360f);
            GameObject slash = Instantiate(slashPrefab, spawnPos, Quaternion.Euler(0, 0, randomRotation));

            EnemyAreaAttack script = slash.GetComponent<EnemyAreaAttack>();
            // ダメージもWAVEに合わせてスケールさせる
            if (script != null) script.Initialize(damage * (GameManager.instance != null ? 1.0f + (GameManager.instance.currentWave * 0.05f) : 1f));

            yield return new WaitForSeconds(0.1f); // 少しバラバラに出現させる
        }
    }
}