using UnityEngine;
using System.Collections;

public class EnemyLaserAttack : MonoBehaviour
{
    [Header("レーザー設定")]
    public GameObject laserVisualPrefab;
    public float attackCooldown = 4.0f;
    public float laserDuration = 0.5f;
    public float laserWidth = 0.4f;
    public float laserDamage = 15f;
    public float maxDistance = 20f;

    private float timer = 0f;
    private Transform player;
    private Enemy enemyScript;
    private bool isAttacking = false;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isAttacking || enemyScript == null || enemyScript.currentHP <= 0 || player == null) return;
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        Camera cam = Camera.main;
        if (cam != null)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;
            if (Mathf.Abs(transform.position.x) > camWidth || Mathf.Abs(transform.position.y) > camHeight)
            {
                return;
            }
        }

        timer += Time.deltaTime;
        if (timer >= attackCooldown)
        {
            StartCoroutine(FireLaserRoutine());
            timer = 0f;
        }
    }

    IEnumerator FireLaserRoutine()
    {
        isAttacking = true;

        float originalSpeed = enemyScript.moveSpeed;
        enemyScript.moveSpeed = 0f;

        // 敵の画像を取得し、元の描画順を記憶しておく
        SpriteRenderer enemySr = GetComponentInChildren<SpriteRenderer>();
        int originalSortingOrder = enemySr != null ? enemySr.sortingOrder : 0;

        if (enemyScript.currentHP > 0 && player != null)
        {
            Vector2 startPos = transform.position;
            Vector2 finalDir = ((Vector2)player.position - startPos).normalized;
            float finalAngle = Mathf.Atan2(finalDir.y, finalDir.x) * Mathf.Rad2Deg;
            float actualDistance = maxDistance;

            RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, finalDir, actualDistance);
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    if (GameManager.instance != null) GameManager.instance.currentHP -= laserDamage;
                    break;
                }
            }

            if (laserVisualPrefab != null)
            {
                GameObject laser = Instantiate(laserVisualPrefab, startPos, Quaternion.Euler(0, 0, finalAngle));
                SpriteRenderer laserSr = laser.GetComponent<SpriteRenderer>();

                if (enemySr != null && laserSr != null)
                {
                    // ★修正：床に埋もれないようにしつつ、レーザーを「敵の裏側」に配置する
                    laserSr.sortingLayerID = enemySr.sortingLayerID;
                    laserSr.sortingOrder = 50;  // レーザーを50に
                    enemySr.sortingOrder = 51;  // 敵本体を51（レーザーより手前）に持ち上げる
                }

                StartCoroutine(AnimateLaser(laser, startPos, finalDir, actualDistance));
            }
        }

        // レーザーが消えるまで待機（硬直）
        yield return new WaitForSeconds(laserDuration);

        // 撃ち終わったら敵の描画順と移動速度を元に戻す
        if (enemySr != null) enemySr.sortingOrder = originalSortingOrder;
        enemyScript.moveSpeed = originalSpeed;
        isAttacking = false;
    }

    IEnumerator AnimateLaser(GameObject laser, Vector2 startPos, Vector2 dir, float distance)
    {
        SpriteRenderer laserSr = laser.GetComponent<SpriteRenderer>();

        float elapsed = 0f;
        while (elapsed < laserDuration)
        {
            if (laser == null) break;

            elapsed += Time.deltaTime;
            float t = elapsed / laserDuration;

            // 幅：発射直後は設定値より太く、そこからシュッと細く消える
            float currentWidth = Mathf.Lerp(laserWidth * 1.5f, 0f, t);

            // 色：一瞬「白」く光り、その後「黄色」→「赤（透明）」へ変化する
            Color currentColor = Color.white;
            if (t < 0.2f)
            {
                currentColor = Color.Lerp(Color.white, Color.yellow, t * 5f);
            }
            else
            {
                currentColor = Color.Lerp(Color.yellow, new Color(1f, 0f, 0f, 0f), (t - 0.2f) * 1.25f);
            }

            if (laserSr != null) laserSr.color = currentColor;

            // レーザーの中心座標とスケールの更新
            laser.transform.position = startPos + dir * (distance / 2f);
            laser.transform.localScale = new Vector3(distance, currentWidth, 1f);

            yield return null;
        }

        if (laser != null) Destroy(laser);
    }
}