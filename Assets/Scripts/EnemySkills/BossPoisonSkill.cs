using UnityEngine;
using System.Collections;

public class BossPoisonFlaskSkill : MonoBehaviour
{
    [Header("フラスコ投下設定")]
    public float skillCooldown = 15.0f;
    public GameObject flaskPrefab;     // カプセルなどの画像プレハブ
    public GameObject poisonGasPrefab; // 1枚目の毒ガスプレハブ
    public int minFlasks = 3;
    public int maxFlasks = 4;
    public float damage = 15f;
    public float gasLifeTime = 4.0f;   // 毒ガスが残る時間

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
            StartCoroutine(DropFlasks());
        }
    }

    IEnumerator DropFlasks()
    {
        if (TickerMessageUI.instance != null) TickerMessageUI.instance.ShowBossSkillMessage();

        int count = Random.Range(minFlasks, maxFlasks + 1);
        Camera cam = Camera.main;
        if (cam == null) yield break;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        for (int i = 0; i < count; i++)
        {
            // 画面内のランダムな目標地点
            float targetX = Random.Range(-camWidth + 1f, camWidth - 1f);
            float targetY = Random.Range(-camHeight + 1f, camHeight - 1f);
            Vector3 targetPos = new Vector3(cam.transform.position.x + targetX, cam.transform.position.y + targetY, 0);

            // 画面の少し上をスタート地点にする
            Vector3 startPos = new Vector3(targetPos.x, cam.transform.position.y + camHeight + 2f, 0);

            StartCoroutine(AnimateFlask(startPos, targetPos));
            yield return new WaitForSeconds(0.4f); // 少しズラしてポイポイ投げる
        }
    }

    IEnumerator AnimateFlask(Vector3 start, Vector3 target)
    {
        GameObject flask = null;
        if (flaskPrefab != null) flask = Instantiate(flaskPrefab, start, Quaternion.identity);

        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // フラスコが落下してくるアニメーション
            if (flask != null) flask.transform.position = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        if (flask != null) Destroy(flask);

        // 着弾したら毒ガスを発生させる
        if (poisonGasPrefab != null)
        {
            GameObject gas = Instantiate(poisonGasPrefab, target, Quaternion.identity);
            EnemyAreaAttack script = gas.GetComponent<EnemyAreaAttack>();
            if (script != null)
            {
                script.lifeTime = gasLifeTime; // 消えるまでの時間を上書き
                script.Initialize(damage * (GameManager.instance != null ? 1.0f + (GameManager.instance.currentWave * 0.05f) : 1f));
            }
        }
    }
}