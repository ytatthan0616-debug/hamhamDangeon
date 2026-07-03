using UnityEngine;

public class EnemyAreaAttack : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 0.5f;

    public void Initialize(float dmg)
    {
        damage = dmg;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (GameManager.instance != null && !GameManager.instance.isGameOver)
            {
                GameManager.instance.currentHP -= damage;
            }
        }
    }
}