using UnityEngine;

public class EnemyTurret : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float fireInterval = 0.5f;
    public float projectileDamage = 10f;
    public float lifeTime = 5f;

    [Header("弾幕の設定")]
    public int projectileCount = 3; // ★追加：一度に撃つ数
    public float spreadAngle = 30f; // ★追加：弾と弾の間の広がり角度

    private float fireTimer = 0f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Fire();
        }
    }

    void Fire()
    {
        if (projectilePrefab == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // ★修正1：画面のど真ん中（カメラの位置）に向かう「基本の角度」を計算する
        Vector2 centerPos = cam.transform.position;
        Vector2 baseDirection = (centerPos - (Vector2)transform.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        // ★修正2：扇状に弾を配置して撃つ
        // (例：3発で30度間隔なら、-30度、0度、+30度の方向に撃つ)
        float startAngle = baseAngle - (spreadAngle * (projectileCount - 1) / 2f);

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = startAngle + (spreadAngle * i);

            // 角度を「飛んでいくベクトル（方向）」に変換
            Vector2 fireDirection = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));

            GameObject arrow = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            EnemyProjectile script = arrow.GetComponent<EnemyProjectile>();

            if (script != null)
            {
                script.Initialize(fireDirection, projectileDamage);
            }
        }
    }
}