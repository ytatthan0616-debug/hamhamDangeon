using UnityEngine;

public class CrashEffect : MonoBehaviour
{
    public void Initialize(Enemy targetEnemy, float damage)
    {
        if (targetEnemy != null)
        {
            targetEnemy.TakeDamage(damage);
        }
        Destroy(gameObject, 1.0f); // エフェクトを見せてから消す
    }
}