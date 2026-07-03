using UnityEngine;
using System.Collections.Generic;

public class MagicProjectile : MonoBehaviour
{
    [Header("魔法の飛ぶスピード（数値を小さくすると遅くなります）")]
    public float speed = 3.5f; // ★5fから3.5fに変更して少し遅くしました

    [Header("ヒット後の演出（残る時間）")]
    public float lingerTime = 0.8f; // ★少し長めに残るようにしました（0.5f -> 0.8f）

    private Transform target;
    private float damage;
    private bool hasHit = false;

    // ★追加：最後に飛んでいた方向を記憶する変数
    private Vector2 lastDirection;

    private List<Enemy> hitEnemies = new List<Enemy>();

    public void SetTarget(Transform enemyTarget)
    {
        target = enemyTarget;

        // 最初の方向を記憶しておく
        if (target != null)
        {
            lastDirection = (target.position - transform.position).normalized;
        }

        if (GameManager.instance != null)
        {
            damage = GameManager.instance.intelligence;
        }

        // 画面外に飛んでいった場合のために、撃ってから3秒後には必ず消滅する安全装置
        Destroy(gameObject, 3f);
    }

    void Update()
    {
        if (hasHit) return;

        if (target != null)
        {
            // ターゲットが生きている間は、ターゲットの方向を更新して追いかける
            lastDirection = (target.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
        else
        {
            // ★修正：ターゲットが死んでいなくなった場合、最後に覚えていた方向にそのままのスピードで飛んでいく！
            transform.position += (Vector3)(lastDirection * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        CheckHit(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (hasHit)
        {
            CheckHit(other);
        }
    }

    void CheckHit(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemyScript = other.GetComponent<Enemy>();
            if (enemyScript != null) OnHitTarget(enemyScript);
        }
        else if (other.transform.parent != null)
        {
            Enemy enemyScript = other.transform.parent.GetComponent<Enemy>();
            if (enemyScript != null) OnHitTarget(enemyScript);
        }
    }

    void OnHitTarget(Enemy enemyScript)
    {
        if (hitEnemies.Contains(enemyScript)) return;

        enemyScript.TakeDamage(damage);
        hitEnemies.Add(enemyScript);

        if (!hasHit)
        {
            hasHit = true;
            Destroy(gameObject, lingerTime);
        }
    }
}