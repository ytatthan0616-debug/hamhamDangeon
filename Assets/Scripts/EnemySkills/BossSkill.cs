using UnityEngine;
using System.Collections;

public class BossSkill : MonoBehaviour
{
    [Header("分身スキルの設定")]
    public float skillInterval = 5.0f;
    public int cloneCount = 10;
    public float spawnRadius = 2.0f;

    private Enemy bossEnemyScript;
    private bool canUseSkill = false;

    void Start()
    {
        bossEnemyScript = GetComponent<Enemy>();
    }

    public void StartSkillRoutine()
    {
        canUseSkill = true;
        StartCoroutine(SkillRoutine());
    }

    IEnumerator SkillRoutine()
    {
        while (canUseSkill && bossEnemyScript != null && bossEnemyScript.currentHP > 0)
        {
            yield return new WaitForSeconds(skillInterval);
            // ★修正：順番に出すためにコルーチンを呼び出す
            StartCoroutine(SpawnClonesRoutine());
        }
    }

    IEnumerator SpawnClonesRoutine()
    {
        Vector3 bossScale = transform.localScale; // ボスの元の大きさを記憶

        for (int i = 0; i < cloneCount; i++)
        {
            float angle = i * (360f / cloneCount);
            Vector2 spawnDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            // 最終的に配置される円周上の座標
            Vector2 targetPos = (Vector2)transform.position + spawnDir * spawnRadius;

            // ★修正1：最初は離れた場所ではなく「ボスの中心（transform.position）」で生成する
            GameObject clone = Instantiate(gameObject, transform.position, Quaternion.identity);

            Destroy(clone.GetComponent<BossSkill>());
            Destroy(clone.GetComponent<BossDescending>());

            // ★修正：親（ボス）が当たり判定OFFの時にコピーされるとクローンもOFFになるため、強制的にONにする！
            Collider2D cloneCol = clone.GetComponent<Collider2D>();
            if (cloneCol != null) cloneCol.enabled = true;

            clone.transform.localScale = bossScale * 0.3f;

            Enemy cloneEnemy = clone.GetComponent<Enemy>();
            if (cloneEnemy != null)
            {
                // ★修正：HPを元の10%から2%へ大幅ダウン
                // 計算式： ボスの最大HP × 0.02
                cloneEnemy.maxHP = bossEnemyScript.maxHP * 0.02f;
                cloneEnemy.currentHP = cloneEnemy.maxHP;

                // ★修正：触れた時のダメージを元の50%から10%へ大幅ダウン
                // 計算式： ボスの接触ダメージ × 0.1
                cloneEnemy.touchDamage = bossEnemyScript.touchDamage * 0.1f;

                // ★追加：移動速度もボスの半分に落とし、逃げやすくする
                cloneEnemy.moveSpeed = bossEnemyScript.moveSpeed * 0.5f;

                // 広がっている最中はボスやプレイヤーを追いかけないようにする
                cloneEnemy.enabled = false;
            }

            // ★修正3：ボスの中心から目標位置へスーッと移動させるコルーチンを開始
            StartCoroutine(MoveCloneToPosition(clone, targetPos, cloneEnemy));

            // ★修正4：0.1秒待ってから次の子分を出す（ポン、ポン、ポンと順番に出る）
            yield return new WaitForSeconds(0.1f);
        }
    }

    // 子分が所定の位置までスーッと広がるアニメーション
    // BossSkill.cs の中にある MoveCloneToPosition メソッドを上書き
    IEnumerator MoveCloneToPosition(GameObject clone, Vector2 target, Enemy cloneEnemy)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        if (clone == null) yield break;
        Vector2 startPos = clone.transform.position;

        while (elapsed < duration)
        {
            // ★修正：移動中にクローンが倒されたら即座に処理を止める（エラー防止）
            if (clone == null || (cloneEnemy != null && cloneEnemy.currentHP <= 0)) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeOut = Mathf.Sin(t * Mathf.PI * 0.5f);
            clone.transform.position = Vector2.Lerp(startPos, target, easeOut);

            yield return null;
        }

        if (clone != null && cloneEnemy != null && cloneEnemy.currentHP > 0)
        {
            clone.transform.position = target;
            cloneEnemy.enabled = true;
        }
    }
}