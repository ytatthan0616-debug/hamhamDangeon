using UnityEngine;
using System.Collections;

public class BossTsujigiriSkill : MonoBehaviour
{
    [Header("辻斬り設定")]
    public float skillCooldown = 12.0f;
    public float moveToTopSpeed = 8.0f;
    public float dashSpeed = 30.0f;
    public float dashDamage = 30f;

    [Header("画面全体エフェクト設定")]
    public GameObject crossSlashPrefab;
    public int slashCount = 15;
    public float slashDamage = 20f;
    public float explosionSpreadDelay = 0.05f;

    private float timer = 0f;
    private Enemy enemyScript;
    private bool isExecuting = false;
    private bool canUseSkill = false;
    private Transform player;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        timer = 8.0f;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (GetComponent<BossDescending>() == null) canUseSkill = true;
    }

    public void StartSkillRoutine() { canUseSkill = true; }

    void Update()
    {
        if (!canUseSkill || isExecuting) return;
        if (enemyScript != null && enemyScript.currentHP <= 0) return;
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = skillCooldown;
            StartCoroutine(TsujigiriRoutine());
        }
    }

    IEnumerator TsujigiriRoutine()
    {
        isExecuting = true;

        float originalMoveSpeed = enemyScript != null ? enemyScript.moveSpeed : 1.0f;
        if (enemyScript != null) enemyScript.moveSpeed = 0f;

        // ★修正1：壁との衝突（ガクガク）を防ぐため、スキル中は当たり判定を一時的に消す
        // これにより、ダッシュ中は完全無敵で壁をすり抜けるようになります
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Camera cam = Camera.main;
        if (cam == null) yield break;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        if (TickerMessageUI.instance != null) TickerMessageUI.instance.ShowBossSkillMessage();

        // 画面の「完全な外側」の上空へ退避
        Vector3 topPosition = new Vector3(cam.transform.position.x, cam.transform.position.y + camHeight + 2.0f, 0f);

        while (Vector2.Distance(transform.position, topPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, topPosition, moveToTopSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // プレイヤーの方向へ一瞬で通り抜ける（画面外まで）
        Vector3 dashTarget = transform.position;
        if (player != null)
        {
            Vector3 dashDirection = (player.position - transform.position).normalized;
            dashTarget = player.position + dashDirection * 25.0f; // 画面外まで完全に突き抜ける距離
        }

        bool hasHitPlayer = false;

        // 猛スピードでダッシュ
        while (Vector2.Distance(transform.position, dashTarget) > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, dashTarget, dashSpeed * Time.deltaTime);

            if (!hasHitPlayer && player != null && Vector2.Distance(transform.position, player.position) < 1.5f)
            {
                hasHitPlayer = true;
                if (GameManager.instance != null && !GameManager.instance.isGameOver)
                {
                    GameManager.instance.currentHP -= dashDamage * (1.0f + (GameManager.instance.currentWave * 0.05f));
                }
            }

            // 完全に画面外に出たらダッシュ終了して次のフェーズへ
            if (Mathf.Abs(transform.position.y) > camHeight + 2.0f || Mathf.Abs(transform.position.x) > camWidth + 2.0f)
            {
                break;
            }

            yield return null;
        }

        // 画面いっぱいに十字斬撃エフェクト
        if (crossSlashPrefab != null)
        {
            for (int i = 0; i < slashCount; i++)
            {
                float randX = Random.Range(-camWidth + 1.0f, camWidth - 1.0f);
                float randY = Random.Range(-camHeight + 1.0f, camHeight - 1.0f);
                Vector3 spawnPos = new Vector3(cam.transform.position.x + randX, cam.transform.position.y + randY, 0f);

                float[] angles = { 0f, 45f, 90f };
                float randAngle = angles[Random.Range(0, angles.Length)];

                GameObject slash = Instantiate(crossSlashPrefab, spawnPos, Quaternion.Euler(0, 0, randAngle));

                EnemyAreaAttack areaAttack = slash.GetComponent<EnemyAreaAttack>();
                if (areaAttack != null)
                {
                    areaAttack.Initialize(slashDamage * (GameManager.instance != null ? 1.0f + (GameManager.instance.currentWave * 0.05f) : 1f));
                }

                yield return new WaitForSeconds(explosionSpreadDelay);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // ==========================================
        // ★修正2：画面上部から再び降りてくる（一連のアクション化）
        // ==========================================
        // どこに飛んで行っても、一旦画面の真上にワープさせる
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y + camHeight + 2.0f, 0f);

        // 降りてくる目標地点（画面の上半分あたり）
        Vector3 descendTarget = new Vector3(cam.transform.position.x, cam.transform.position.y + camHeight * 0.4f, 0f);

        while (Vector2.Distance(transform.position, descendTarget) > 0.1f)
        {
            // スッと降りてくる
            transform.position = Vector2.MoveTowards(transform.position, descendTarget, moveToTopSpeed * 0.5f * Time.deltaTime);
            yield return null;
        }

        // スキル終了。当たり判定と移動を元に戻す
        if (col != null) col.enabled = true;
        if (enemyScript != null) enemyScript.moveSpeed = originalMoveSpeed;
        isExecuting = false;
    }
}