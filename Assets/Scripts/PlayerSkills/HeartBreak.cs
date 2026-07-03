using UnityEngine;
using System.Collections;

public class HeartBreakEffect : MonoBehaviour
{
    public void Initialize(float delay, float radius, float damage)
    {
        StartCoroutine(HeartBreakRoutine(delay, radius, damage));
    }

    IEnumerator HeartBreakRoutine(float delay, float radius, float damage)
    {
        yield return new WaitForSeconds(delay); // 指定時間待機して爆発

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null) e.TakeDamage(damage);
        }
        Destroy(gameObject, 0.5f); // 爆発の余韻を残して消滅
    }
}