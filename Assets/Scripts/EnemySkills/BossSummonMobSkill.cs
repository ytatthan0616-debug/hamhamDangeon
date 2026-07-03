using UnityEngine;
using System.Collections;

public class BossSummonMobSkill : MonoBehaviour
{
    [Header("モブ召喚設定")]
    public float skillCooldown = 20.0f;
    public GameObject bgEffectPrefab;
    public GameObject mobPrefab;
    public int mobCount = 4;

    private float timer = 0f;
    private Enemy enemyScript;
    private bool canUseSkill = false;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        timer = 12.0f;
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
            StartCoroutine(SummonRoutine());
        }
    }

    IEnumerator SummonRoutine()
    {
        if (TickerMessageUI.instance != null) TickerMessageUI.instance.ShowBossSkillMessage();
        Camera cam = Camera.main;
        if (cam == null) yield break;

        // ① 画面下部に背景エフェクトを発生
        Vector3 bgPos = new Vector3(cam.transform.position.x, cam.transform.position.y - cam.orthographicSize + 2.5f, 0);
        if (bgEffectPrefab != null)
        {
            GameObject bg = Instantiate(bgEffectPrefab, bgPos, Quaternion.identity);
            Destroy(bg, 5.0f); // 5秒後に勝手に消える
        }

        yield return new WaitForSeconds(1.0f); // エフェクトが出てから少し待つ

        // ② エフェクトの真ん中（bgPos）から雑魚敵をわらわらと湧き出させる
        for (int i = 0; i < mobCount; i++)
        {
            // 真ん中から少しだけランダムにズラす
            Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
            Vector3 spawnPos = bgPos + (Vector3)randomOffset;

            if (mobPrefab != null) Instantiate(mobPrefab, spawnPos, Quaternion.identity);

            // わらわら感を出すために0.1秒ずつズラして出現させる
            yield return new WaitForSeconds(0.1f);
        }
    }
}