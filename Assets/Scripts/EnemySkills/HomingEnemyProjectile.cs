using UnityEngine;

public class HomingEnemyProjectile : MonoBehaviour
{
    public float speed = 4f;
    public float rotateSpeed = 200f;
    public float damage = 10f;
    public float lifeTime = 5f;
    public float rotationOffset = -90f;

    private Transform target;
    private Vector2 currentDirection;

    public void Initialize(float dmg)
    {
        damage = dmg;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            currentDirection = (target.position - transform.position).normalized;
        }
        else
        {
            currentDirection = Vector2.down;
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target != null)
        {
            Vector2 targetDirection = (target.position - transform.position).normalized;
            // 徐々にターゲットの方向へ向きを変える
            currentDirection = Vector3.RotateTowards(currentDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f).normalized;
        }

        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        // 前に飛ぶ！
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