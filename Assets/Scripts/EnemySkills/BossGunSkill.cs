using UnityEngine;
using System.Collections;

public class BossGunSkill : MonoBehaviour
{
    [Header("銃スキルの設定")]
    public float skillCooldown = 4.0f; // 撃つ間隔
    public GameObject normalProjectilePrefab; // 通常の弾（正方形）
    public GameObject homingProjectilePrefab; // ホーミング弾
    public float damage = 10f;

    private Enemy enemyScript;
    private bool canUseSkill = false;
    private float timer = 0f;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        timer = 2.0f; // 着地後、少し待ってから撃ち始める

        if (GetComponent<BossDescending>() == null) canUseSkill = true;
    }

    public void StartSkillRoutine()
    {
        canUseSkill = true;
    }

    void Update()
    {
        if (!canUseSkill) return;
        if (enemyScript != null && enemyScript.currentHP <= 0) return;
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = skillCooldown;
            StartCoroutine(FireRandomSkill());
        }
    }

    IEnumerator FireRandomSkill()
    {
        // 3つのパターンからランダムで1つ選んで攻撃！
        int rand = Random.Range(0, 3);
        if (rand == 0) yield return StartCoroutine(FireRadial());
        else if (rand == 1) yield return StartCoroutine(FireSpread());
        else yield return StartCoroutine(FireHoming());
    }

    IEnumerator FireRadial()
    {
        // パターン①：円形に放射（12方向）
        if (TickerMessageUI.instance != null) TickerMessageUI.instance.ShowBossSkillMessage();

        int count = 12;
        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count);
            FireNormalProjectile(angle);
        }
        yield return null;
    }

    IEnumerator FireSpread()
    {
        // パターン②：狙ったところに乱射（マシンガンのように時間差で）
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        if (TickerMessageUI.instance != null) TickerMessageUI.instance.ShowBossSkillMessage();

        Vector2 baseDir = (player.transform.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        int count = 8;
        for (int i = 0; i < count; i++)
        {
            // ハムスターの方向から少しだけ角度をずらして乱射
            float angle = baseAngle + Random.Range(-20f, 20f);
            FireNormalProjectile(angle);
            yield return new WaitForSeconds(0.1f); // 0.1秒間隔で撃つ
        }
    }

    IEnumerator FireHoming()
    {
        // パターン③：ホーミング弾を3発撃つ
        if (TickerMessageUI.instance != null) TickerMessageUI.instance.ShowBossSkillMessage();

        int count = 3;
        for (int i = 0; i < count; i++)
        {
            if (homingProjectilePrefab != null)
            {
                // ボスの周囲の少しズレた位置から発射
                Vector2 spawnPos = transform.position + (Vector3)Random.insideUnitCircle;
                GameObject proj = Instantiate(homingProjectilePrefab, spawnPos, Quaternion.identity);
                HomingEnemyProjectile script = proj.GetComponent<HomingEnemyProjectile>();
                if (script != null) script.Initialize(damage);
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    void FireNormalProjectile(float angle)
    {
        if (normalProjectilePrefab == null) return;
        Vector2 fireDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        GameObject proj = Instantiate(normalProjectilePrefab, transform.position, Quaternion.identity);
        EnemyProjectile script = proj.GetComponent<EnemyProjectile>();
        if (script != null) script.Initialize(fireDirection, damage);
    }
}