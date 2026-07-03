using UnityEngine;
using System.Collections;

public class BossOrbSkill : MonoBehaviour
{
    [Header("青オーブ設定")]
    public float skillCooldown = 8.0f;
    public GameObject blueOrbPrefab;
    public float damage = 20f;
    public int orbCount = 1;

    private float timer = 0f;
    private Enemy enemyScript;
    private bool canUseSkill = false;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        timer = 5.0f;
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
            StartCoroutine(SpawnOrbs());
        }
    }

    IEnumerator SpawnOrbs()
    {
        if (blueOrbPrefab == null) yield break;
        if (TickerMessageUI.instance != null) TickerMessageUI.instance.ShowBossSkillMessage();

        for (int i = 0; i < orbCount; i++)
        {
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 2f;
            GameObject orb = Instantiate(blueOrbPrefab, spawnPos, Quaternion.identity);

            HomingOrbProjectile script = orb.GetComponent<HomingOrbProjectile>();
            if (script != null) script.Initialize(damage * (GameManager.instance != null ? 1.0f + (GameManager.instance.currentWave * 0.05f) : 1f));

            yield return new WaitForSeconds(0.5f);
        }
    }
}