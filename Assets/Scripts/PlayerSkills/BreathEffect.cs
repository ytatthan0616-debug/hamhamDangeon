using UnityEngine;
using System.Collections;

public class BreathEffect : MonoBehaviour
{
    private Transform owner;
    private float range;

    public void Initialize(float duration, float attackRange, float damagePerTick, Transform hamster)
    {
        owner = hamster;
        range = attackRange;
        StartCoroutine(BreathRoutine(duration, damagePerTick));
    }

    IEnumerator BreathRoutine(float duration, float damage)
    {
        float elapsed = 0f;
        float tick = 0.25f;

        while (elapsed < duration)
        {
            if (owner != null)
            {
                // ハムスターの位置に追従
                transform.position = owner.position;

                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                Transform nearest = null;
                float minDist = Mathf.Infinity;
                foreach (var e in enemies)
                {
                    float dist = Vector2.Distance(owner.position, e.transform.position);
                    if (dist < minDist) { minDist = dist; nearest = e.transform; }
                }

                if (nearest != null)
                {
                    // 敵の方向を3Dのベクトルとして計算
                    Vector3 dir3D = new Vector3(nearest.position.x - owner.position.x, nearest.position.y - owner.position.y, 0).normalized;

                    // ★大修正：3D用エフェクトの発射口（Z軸）を、敵のいる方向（XY平面）に無理やり向ける魔法のコード！
                    if (dir3D != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(dir3D, Vector3.back);
                    }

                    Collider2D[] hits = Physics2D.OverlapCircleAll(owner.position, range);
                    foreach (var hit in hits)
                    {
                        Enemy e = hit.GetComponent<Enemy>();
                        if (e != null)
                        {
                            Vector2 dirToEnemy = (hit.transform.position - owner.position).normalized;
                            if (Vector2.Angle(dir3D, dirToEnemy) < 45f) e.TakeDamage(damage);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(tick);
            elapsed += tick;
        }
        Destroy(gameObject);
    }
}