using UnityEngine;
using System.Collections.Generic;

public class MeleeAttackEffect : MonoBehaviour
{
    [HideInInspector] public float damage;
    public float attackRadius = 1.5f;
    public float lifeTime = 0.5f;

    private List<Enemy> hitEnemies = new List<Enemy>();

    // ★軽量化：毎フレーム新しく「配列（ゴミ）」を作らないよう、あらかじめ20個分の使い回す箱を用意
    private Collider2D[] hitCache = new Collider2D[20];

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // ★軽量化：OverlapCircleNonAllocを使うことで、メモリを全く消費せずに当たり判定が可能！
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, attackRadius, hitCache);

        for (int i = 0; i < hitCount; i++)
        {
            Enemy enemy = hitCache[i].GetComponent<Enemy>();
            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                enemy.TakeDamage(damage);
                hitEnemies.Add(enemy);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}