using UnityEngine;
using System.Collections;

public class ExplodingMob : MonoBehaviour
{
    private float lifeTime;
    private float explosionRadius;
    private float explosionDamage;
    private GameObject explosionPrefab;

    public void Initialize(float lifeTime, float radius, float damage, GameObject fxPrefab, Vector2 startPos)
    {
        this.lifeTime = lifeTime;
        this.explosionRadius = radius;
        this.explosionDamage = damage;
        this.explosionPrefab = fxPrefab;

        // 投げたような動きを開始
        StartCoroutine(ThrowRoutine(startPos));
    }

    IEnumerator ThrowRoutine(Vector2 startPos)
    {
        float throwDuration = 0.5f;
        Vector2 targetPos = (Vector2)transform.position;
        float elapsed = 0f;

        // ★追加：最終的な大きさをここで指定します（例として0.5倍に設定）
        // もっと小さくしたい場合は 0.3f などに変更してください．
        Vector3 finalScale = new Vector3(0.5f, 0.5f, 1f);

        transform.localScale = Vector3.zero;

        while (elapsed < throwDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / throwDuration;

            // ★修正：0から finalScale の大きさに向かって徐々に大きくする
            transform.localScale = Vector3.Lerp(Vector3.zero, finalScale, t);

            float height = Mathf.Sin(t * Mathf.PI) * 1.5f;
            transform.position = Vector2.Lerp(startPos, targetPos, t) + new Vector2(0, height);

            yield return null;
        }

        // ★修正：着地した時に確実に指定した大きさにする
        transform.localScale = finalScale;

        StartCoroutine(MobRoutine());
    }

    IEnumerator MobRoutine()
    {
        float elapsed = 0f;
        float speed = 1.5f;
        Vector2 direction = Random.insideUnitCircle.normalized;

        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        while (elapsed < lifeTime)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            Vector2 pos = transform.position;

            float topBound = cam.transform.position.y + camHeight - 0.5f;
            float bottomBound = cam.transform.position.y - camHeight + 0.5f;
            float leftBound = cam.transform.position.x - camWidth + 0.5f;
            float rightBound = cam.transform.position.x + camWidth - 0.5f;

            if (pos.x > rightBound) { pos.x = rightBound; direction.x *= -1; }
            else if (pos.x < leftBound) { pos.x = leftBound; direction.x *= -1; }
            if (pos.y > topBound) { pos.y = topBound; direction.y *= -1; }
            else if (pos.y < bottomBound) { pos.y = bottomBound; direction.y *= -1; }

            transform.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            fx.transform.localScale = Vector3.one * explosionRadius;
            Destroy(fx, 1.0f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null) e.TakeDamage(explosionDamage);
        }

        Destroy(gameObject);
    }
}