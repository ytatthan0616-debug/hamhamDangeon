using UnityEngine;

public class BossBarrier : MonoBehaviour
{
    [Header("バリアの設定")]
    public float barrierRadius = 2.0f; // バリアの大きさ（この中にモブを入れない）
    public float pushForce = 5.0f;     // 弾き出す強さ

    void Update()
    {
        // 自分がやられている、またはゲームオーバー時はバリアを消す
        Enemy myEnemy = GetComponent<Enemy>();
        if (myEnemy != null && myEnemy.currentHP <= 0) return;
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        // 自分の周囲（バリア内）にいる物体をすべて探す
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, barrierRadius);
        foreach (Collider2D c in cols)
        {
            // 自分自身は無視
            if (c.gameObject == this.gameObject) continue;

            // 相手が「Enemy（雑魚敵）」だったら外側へ押し出す！
            if (c.CompareTag("Enemy"))
            {
                Enemy otherEnemy = c.GetComponent<Enemy>();
                // ボス同士は干渉しない
                if (otherEnemy != null && otherEnemy.isBoss) continue;

                // 相手を外側へ押し出す計算
                Vector3 diff = c.transform.position - transform.position;
                float dist = diff.magnitude;

                if (dist < barrierRadius && dist > 0.01f)
                {
                    // バリアの中心（ボス）に近いほど、強力に外へ弾き飛ばす
                    float pushPower = (barrierRadius - dist) * pushForce;
                    c.transform.position += diff.normalized * pushPower * Time.deltaTime;
                }
            }
        }
    }

    // ★おまけ：Unityの編集画面でバリアの大きさが赤い円で見えるようになります！
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, barrierRadius);
    }
}