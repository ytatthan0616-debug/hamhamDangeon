using UnityEngine;
using System.Collections;

public class PoisonEffect : MonoBehaviour
{
    public void Initialize(float duration, float radius, float damagePerTick)
    {
        StartCoroutine(PoisonRoutine(duration, radius, damagePerTick));
    }

    IEnumerator PoisonRoutine(float duration, float radius, float damage)
    {
        float elapsed = 0f;
        float tick = 0.5f;
        while (elapsed < duration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                Enemy e = hit.GetComponent<Enemy>();
                if (e != null) e.TakeDamage(damage);
            }
            yield return new WaitForSeconds(tick);
            elapsed += tick;
        }
        Destroy(gameObject);
    }
}