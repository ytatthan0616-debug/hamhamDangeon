using UnityEngine;
using System.Collections;

public class BossRainSkill : MonoBehaviour
{
    [Header("発動モード設定")]
    public bool isOneTimeHpTrigger = false;

    [Header("雨スキルの設定")]
    public GameObject rainEffectPrefab;
    public float skillCooldown = 10.0f;
    public float rainDuration = 3.0f;

    [Header("ダメージ設定")]
    public float damagePercentPerTick = 0.05f;
    public float tickInterval = 1.0f;

    [Header("発動条件（銃ボス用）")]
    public float triggerHpPercent = 0.4f;
    private bool hasTriggered = false;

    private float timer = 0f;
    private Enemy enemyScript;
    private bool canUseSkill = false;

    // ★追加：生成した雨を記憶しておく変数
    private GameObject activeRainObj = null;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        timer = 0.5f;

        if (GetComponent<BossDescending>() == null)
        {
            canUseSkill = true;
        }
    }

    public void StartSkillRoutine()
    {
        canUseSkill = true;
    }

    void Update()
    {
        if (!canUseSkill) return;

        // ★修正：ボスがやられたら、降っている雨も強制的に消す
        if (enemyScript != null && enemyScript.currentHP <= 0)
        {
            if (activeRainObj != null) Destroy(activeRainObj);
            return;
        }

        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            if (activeRainObj != null) Destroy(activeRainObj);
            return;
        }

        if (isOneTimeHpTrigger)
        {
            if (!hasTriggered && enemyScript != null && enemyScript.currentHP <= enemyScript.maxHP * triggerHpPercent)
            {
                hasTriggered = true;
                if (TickerMessageUI.instance != null) TickerMessageUI.instance.ShowMessage("<color=#ff0000>The boss goes into a rage! A deadly rain begins to fall!!</color>");
                StartCoroutine(RainRoutine());
            }
        }
        else
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = skillCooldown;
                StartCoroutine(RainRoutine());
            }
        }
    }

    IEnumerator RainRoutine()
    {
        // 前回の雨がもし残っていたら消す
        if (activeRainObj != null) Destroy(activeRainObj);

        if (rainEffectPrefab != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 spawnPos = cam.transform.position + new Vector3(0, cam.orthographicSize, 0);
                spawnPos.z = 0;
                // ★修正：生成した雨を activeRainObj に記憶しておく
                activeRainObj = Instantiate(rainEffectPrefab, spawnPos, Quaternion.identity);
            }
        }

        float elapsed = 0f;
        float nextTick = tickInterval;

        while (elapsed < rainDuration)
        {
            if (GameManager.instance != null && GameManager.instance.isGameOver) break;
            if (enemyScript != null && enemyScript.currentHP <= 0) break; // ボスが死んだらループを抜ける

            elapsed += Time.deltaTime;

            if (elapsed >= nextTick)
            {
                nextTick += tickInterval;

                if (GameManager.instance != null)
                {
                    float damage = GameManager.instance.maxHP * damagePercentPerTick;
                    GameManager.instance.currentHP -= damage;

                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null && enemyScript != null && enemyScript.damageTextPrefab != null)
                    {
                        Vector3 popupPos = player.transform.position + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0f);
                        GameObject textObj = Instantiate(enemyScript.damageTextPrefab, popupPos, Quaternion.identity);
                        DamageText popup = textObj.GetComponent<DamageText>();
                        if (popup != null) popup.Setup(Mathf.RoundToInt(damage).ToString(), Color.magenta);
                    }
                }
            }
            yield return null;
        }

        // 時間が来たら雨を破壊する
        if (activeRainObj != null)
        {
            Destroy(activeRainObj);
            activeRainObj = null;
        }
    }

    // ★追加：スクリプトが破棄された時（ボスが消滅する時）の最後の安全装置
    void OnDestroy()
    {
        if (activeRainObj != null) Destroy(activeRainObj);
    }
}