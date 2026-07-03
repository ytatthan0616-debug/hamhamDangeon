using UnityEngine;

public class ExplodeOnDeath : MonoBehaviour
{
    public GameObject explosionPrefab; // 4枚目の血飛沫/爆発エフェクト
    public float explosionDamage = 15f;

    private Enemy enemyScript;
    private bool hasExploded = false;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
    }

    void Update()
    {
        // HPが0になった瞬間に一度だけ爆発をスポーンする
        if (enemyScript != null && enemyScript.currentHP <= 0 && !hasExploded)
        {
            hasExploded = true;
            if (explosionPrefab != null)
            {
                // 死亡した位置に爆発エフェクトを出す
                GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

                // ダメージ判定スクリプトがあれば設定する
                EnemyAreaAttack atk = exp.GetComponent<EnemyAreaAttack>();
                if (atk != null)
                {
                    atk.lifeTime = 0.6f;
                    atk.Initialize(explosionDamage * (GameManager.instance != null ? 1.0f + (GameManager.instance.currentWave * 0.05f) : 1f));
                }
                else
                {
                    // スクリプトが付いていなくても、画像だけ出して1秒後に消す
                    Destroy(exp, 1.0f);
                }
            }
        }
    }
}