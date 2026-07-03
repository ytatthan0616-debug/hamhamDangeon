using UnityEngine;

public class HomingOrbProjectile : MonoBehaviour
{
    public float speed = 2.5f;           // ゆっくり追従
    public float rotateSpeed = 120f;     // 曲がる速さ（大きいほど急カーブできる）
    public float damage = 10f;
    public float homingDuration = 4.0f;  // 何秒間ホーミングするか
    public float totalLifeTime = 10.0f;  // 画面外に消えるまでの最大時間

    private Transform target;
    private Vector2 currentDirection;
    private float timer = 0f;

    public void Initialize(float dmg)
    {
        damage = dmg;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            // 最初の進行方向をターゲットに向ける
            currentDirection = (target.position - transform.position).normalized;
        }
        else
        {
            currentDirection = Vector2.down;
        }

        Destroy(gameObject, totalLifeTime);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < homingDuration && target != null)
        {
            // 目標の方向ベクトル
            Vector2 targetDirection = (target.position - transform.position).normalized;

            // 現在の角度と目標の角度をそれぞれ計算
            float currentAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

            // 進行方向の角度だけを滑らかにターゲットへ近づける
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotateSpeed * Time.deltaTime);

            // 計算した新しい角度を、再び進行方向のベクトルに戻す
            currentDirection = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));
        }

        // ★修正：画像の回転（transform.rotation）を削除！
        // 丸いオーブは回転させるとエフェクトが壊れるため、位置の移動だけを行う
        transform.position += (Vector3)currentDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (GameManager.instance != null && !GameManager.instance.isGameOver)
            {
                GameManager.instance.currentHP -= damage;
            }
            Destroy(gameObject);
        }
    }
}