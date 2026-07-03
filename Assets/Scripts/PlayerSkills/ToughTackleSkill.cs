using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ToughTackleSkill : MonoBehaviour
{
    // ★インスペクターで進化段階ごとに設定できるパラメータの設計図
    [System.Serializable]
    public class TackleTier
    {
        public float cooldown = 2.0f;      // 攻撃速度（何秒に1回タックルするか）
        public float dashSpeed = 15f;      // 突進の速さ
        public float hitboxWidth = 2.0f;   // 当たり判定の幅（横幅）
        public float hitboxLength = 2.0f;  // 当たり判定の長さ（前方向への伸び）
        public int targetCount = 1;        // 狙う敵の数（2以上なら別の敵に残像が突撃する）
        public float effectScale = 1.0f;   // エフェクトの大きさ倍率
    }

    [Header("進化段階ごとのタックル設定（上から第1〜第5形態）")]
    public TackleTier[] tackleTiers = new TackleTier[5];

    [Header("演出用プレハブ")]
    public ParticleSystem tackleEffectPrefab; // 用意したパーティクルをここに登録

    private float attackTimer = 0f;

    void Update()
    {
        // GameManagerが存在し、かつタフタイプに進化している時のみ作動する
        if (GameManager.instance == null || GameManager.instance.currentEvolution != GameManager.EvolvedType.Tough)
            return;

        // 現在の進化段階（1〜5）に合わせてパラメータの配列を参照する
        int tierIndex = Mathf.Clamp(GameManager.instance.evolutionTier - 1, 0, tackleTiers.Length - 1);
        TackleTier currentTier = tackleTiers[tierIndex];

        attackTimer += Time.deltaTime;

        // 攻撃速度（クールダウン）に達したら突撃を実行
        if (attackTimer >= currentTier.cooldown)
        {
            ExecuteTackle(currentTier);
            attackTimer = 0f;
        }
    }

    void ExecuteTackle(TackleTier tier)
    {
        // 画面内の敵をすべて取得
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (allEnemies.Length == 0) return;

        // ★遠くの敵を狙う計算式：距離を計算し、降順（遠い順）に並べ替える
        var sortedEnemies = allEnemies
            .OrderByDescending(e => Vector2.Distance(transform.position, e.transform.position))
            .ToList();

        // 狙う数（targetCount）と実際の敵の数のうち、少ない方を採用（重複を防ぐ）
        int count = Mathf.Min(tier.targetCount, sortedEnemies.Count);

        for (int i = 0; i < count; i++)
        {
            Transform target = sortedEnemies[i].transform;

            // 1体目のターゲット（一番遠い敵）にはハムスター本体が突撃する
            if (i == 0)
            {
                StartCoroutine(DashRoutine(target, tier, true));
            }
            // 2体目以降には、本体は動かずエフェクトだけを飛ばす（マルチロックオン）
            else
            {
                StartCoroutine(DashRoutine(target, tier, false));
            }
        }
    }

    IEnumerator DashRoutine(Transform target, TackleTier tier, bool isMainBody)
    {
        if (target == null) yield break;

        Vector2 startPos = transform.position;
        Vector2 targetPos = target.position;

        // 進行方向のベクトルと、画像を回転させるための角度を計算
        Vector2 direction = (targetPos - startPos).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // エフェクトを生成し、進行方向に向けて大きさを変える
        ParticleSystem effect = null;
        if (tackleEffectPrefab != null)
        {
            effect = Instantiate(tackleEffectPrefab, startPos, Quaternion.Euler(0, 0, angle));
            effect.transform.localScale = Vector3.one * tier.effectScale;
        }

        // 距離と速度から到達にかかる時間を逆算
        float distance = Vector2.Distance(startPos, targetPos);
        float travelTime = distance / tier.dashSpeed;
        float elapsedTime = 0f;

        // ダッシュの移動処理
        while (elapsedTime < travelTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / travelTime;
            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, t);

            if (isMainBody) transform.position = currentPos;
            if (effect != null) effect.transform.position = currentPos;

            yield return null;
        }

        // 最終地点に到達
        if (isMainBody) transform.position = targetPos;
        if (effect != null) effect.transform.position = targetPos;

        // 範囲ダメージの発生
        DealDamageInArea(targetPos, direction, tier, angle);

        // パーティクルを少し残して自然に消す
        if (effect != null) Destroy(effect.gameObject, 0.5f);
    }

    void DealDamageInArea(Vector2 hitPos, Vector2 direction, TackleTier tier, float angle)
    {
        float damage = GameManager.instance.attackPower;

        // ★「前方向」に判定を出すための幾何学計算
        // ボックスの中心座標 = 衝突地点 + (進行方向ベクトル × (長さの半分))
        Vector2 boxCenter = hitPos + direction * (tier.hitboxLength / 2f);
        Vector2 boxSize = new Vector2(tier.hitboxLength, tier.hitboxWidth);

        // Physics2D.OverlapBoxAll を使って、傾いた長方形の範囲内にいる敵をすべて取得
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle);

        foreach (Collider2D hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null) enemy.TakeDamage(damage);
            }
        }
    }
}