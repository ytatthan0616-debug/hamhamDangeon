using UnityEngine;
using System.Collections;

public class BossSineSkullSkill : MonoBehaviour
{
    [Header("ドクロ放射設定")]
    public float skillCooldown = 9.0f;
    public GameObject skullPrefab;
    public float damage = 20f;
    public int projectileCount = 10;

    private float timer = 0f;
    private Enemy enemyScript;
    private bool canUseSkill = false;
    private bool invertNext = false; // 次回逆向きにするフラグ

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        timer = 8.0f;
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
            StartCoroutine(FireSkulls());
        }
    }

    IEnumerator FireSkulls()
    {
        if (TickerMessageUI.instance != null) TickerMessageUI.instance.ShowBossSkillMessage();

        float angleStep = 360f / projectileCount;
        float currentDamage = damage * (GameManager.instance != null ? 1.0f + (GameManager.instance.currentWave * 0.05f) : 1f);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject skull = Instantiate(skullPrefab, transform.position, Quaternion.identity);
            SineWaveProjectile script = skull.GetComponent<SineWaveProjectile>();
            if (script != null)
            {
                script.Initialize(currentDamage, dir, invertNext);
            }
        }

        // 次撃つ時は波の向きを逆にする
        invertNext = !invertNext;
        yield return null;
    }
}