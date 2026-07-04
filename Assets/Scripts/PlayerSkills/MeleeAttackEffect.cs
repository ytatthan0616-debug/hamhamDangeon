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

    // ★追加：新しい判定方法に必要なフィルター
    private ContactFilter2D filter;

    void Start()
    {
        Destroy(gameObject, lifeTime);

        // ★追加：プロジェクトの物理設定（トリガー検知など）をそのまま使うフィルターを作成
        filter = new ContactFilter2D();
        filter.useTriggers = Physics2D.queriesHitTriggers;
    }

    void Update()
    {
        // ★修正：警告が出ていた古い NonAlloc をやめて、最新の OverlapCircle に書き換え
        int hitCount = Physics2D.OverlapCircle(transform.position, attackRadius, filter, hitCache);

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