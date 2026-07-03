using UnityEngine;
using System.Collections;

public class BossDescending : MonoBehaviour
{
    public Vector2 targetPosition = new Vector2(0f, 2.5f);

    [Header("落下の設定")]
    public float gravity = 25.0f;
    private float currentSpeed = 0f;

    private bool isLanded = false;
    private Enemy enemyScript;
    private Collider2D bossCollider; // ★追加：ボスの当たり判定

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        bossCollider = GetComponent<Collider2D>(); // ★追加

        // ★修正：空から登場している間は、壁に引っかからないように当たり判定を消す！
        if (enemyScript != null) enemyScript.enabled = false;
        if (bossCollider != null) bossCollider.enabled = false;

        Camera cam = Camera.main;
        if (cam != null)
        {
            // 画面のはるか上空からスタート
            transform.position = new Vector3(transform.position.x, cam.transform.position.y + cam.orthographicSize + 4f, transform.position.z);
        }
    }

    void Update()
    {
        if (isLanded) return;

        // 重力でどんどん加速して落ちる
        currentSpeed += gravity * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPosition.x, targetPosition.y, transform.position.z), currentSpeed * Time.deltaTime);

        // 着地判定
        if (transform.position.y <= targetPosition.y + 0.5f)
        {
            isLanded = true;

            // ★修正：着地したら当たり判定と動きを元に戻す！
            if (enemyScript != null) enemyScript.enabled = true;
            if (bossCollider != null) bossCollider.enabled = true;

            // スキル起動
            BossSkill cloneSkill = GetComponent<BossSkill>();
            if (cloneSkill != null) cloneSkill.StartSkillRoutine();

            BossTurretSkill turretSkill = GetComponent<BossTurretSkill>();
            if (turretSkill != null) turretSkill.StartSkillRoutine();

            BossRainSkill rainSkill = GetComponent<BossRainSkill>();
            if (rainSkill != null) rainSkill.StartSkillRoutine();

            BossJumpSkill jumpSkill = GetComponent<BossJumpSkill>();
            if (jumpSkill != null) jumpSkill.StartSkillRoutine();

            // 前回追加した銃スキルも起動
            BossGunSkill gunSkill = GetComponent<BossGunSkill>();
            if (gunSkill != null) gunSkill.StartSkillRoutine();

            BossRandomSlashSkill slashSkill = GetComponent<BossRandomSlashSkill>();
            if (slashSkill != null) slashSkill.StartSkillRoutine();

            BossOrbSkill orbSkill = GetComponent<BossOrbSkill>();
            if (orbSkill != null) orbSkill.StartSkillRoutine();

            BossTsujigiriSkill tsujigiriSkill = GetComponent<BossTsujigiriSkill>();
            if (tsujigiriSkill != null) tsujigiriSkill.StartSkillRoutine();

            BossPoisonFlaskSkill poisonSkill = GetComponent<BossPoisonFlaskSkill>();
            if (poisonSkill != null) poisonSkill.StartSkillRoutine();

            // 2. 正弦波ドクロ放射スキル
            BossSineSkullSkill skullSkill = GetComponent<BossSineSkullSkill>();
            if (skullSkill != null) skullSkill.StartSkillRoutine();

            // 3. 怨霊背景＆雑魚召喚スキル
            BossSummonMobSkill summonSkill = GetComponent<BossSummonMobSkill>();
            if (summonSkill != null) summonSkill.StartSkillRoutine();
        }
    }
}