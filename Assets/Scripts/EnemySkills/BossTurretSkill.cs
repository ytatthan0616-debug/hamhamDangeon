using UnityEngine;

public class BossTurretSkill : MonoBehaviour
{
    [Header("タレット設定")]
    public GameObject turretPrefab;
    public float skillCooldown = 6.0f;

    [Header("画面端からの余白（画面外にはみ出さないように調整）")]
    public float marginX = 1.0f;
    public float marginY = 1.5f;

    private float timer = 0f;
    private Enemy enemyScript;
    private bool canUseSkill = false;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        timer = 0.5f; // 着地後すぐに1回目を出す

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
        if (enemyScript != null && enemyScript.currentHP <= 0) return;
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = skillCooldown;
            SpawnTurret();
        }
    }

    void SpawnTurret()
    {
        if (turretPrefab == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // カメラの視界情報から、実際の画面の左下と右上のワールド座標を取得する
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        // 4つのカドの座標を計算（余白分だけ画面の内側にずらす）
        Vector2[] corners = new Vector2[]
        {
            new Vector2(bottomLeft.x + marginX, topRight.y - marginY),   // 左上
            new Vector2(topRight.x - marginX, topRight.y - marginY),     // 右上
            new Vector2(bottomLeft.x + marginX, bottomLeft.y + marginY), // 左下
            new Vector2(topRight.x - marginX, bottomLeft.y + marginY)    // 右下
        };

        Vector2 spawnPos = corners[0];

        // ==========================================
        // ★修正：ハムスターの位置を探して、一番遠いカドを選ぶ
        // ==========================================
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float maxDistance = 0f;
            foreach (Vector2 corner in corners)
            {
                // ハムスターとカドの距離を計算
                float dist = Vector2.Distance(corner, player.transform.position);

                // より遠いカドを見つけたら、それを採用する
                if (dist > maxDistance)
                {
                    maxDistance = dist;
                    spawnPos = corner;
                }
            }
        }
        else
        {
            // 万が一プレイヤーが見つからない場合はランダム
            spawnPos = corners[Random.Range(0, corners.Length)];
        }

        Instantiate(turretPrefab, spawnPos, Quaternion.identity);
    }
}