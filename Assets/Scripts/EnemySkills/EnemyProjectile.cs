using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 5f;
    private float damage = 10f;
    private Vector2 moveDirection;

    public void Initialize(Vector2 dir, float dmg)
    {
        moveDirection = dir.normalized;
        damage = dmg;

        // 進行方向に合わせて弾の画像を回転させる（上が正面の画像の場合 -90f）
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        Destroy(gameObject, 5f); // 5秒後に自動で消える
    }

    void Update()
    {
        // 指定された方向へまっすぐ進む
        transform.position += (Vector3)moveDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (GameManager.instance != null && !GameManager.instance.isGameOver)
            {
                GameManager.instance.currentHP -= damage;
            }
            Destroy(gameObject); // 当たったら消える
        }
    }
}