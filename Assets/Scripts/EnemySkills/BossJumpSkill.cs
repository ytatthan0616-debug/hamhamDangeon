using UnityEngine;
using System.Collections;

public class BossJumpSkill : MonoBehaviour
{
    [Header("ジャンプスキルの設定")]
    public float skillCooldown = 15.0f;
    public float jumpHeight = 6.0f;
    public float jumpUpSpeed = 15.0f;
    public float slamDownSpeed = 40.0f;
    public float hangTime = 0.8f;

    [Header("衝撃波の設定")]
    public float shockwaveDamage = 20f;
    public float pushbackTime = 0.2f;

    // ★追加：エフェクトのプレハブ
    public GameObject shockwaveEffectPrefab;
    public float effectScale = 2.0f;

    private float timer = 0f;
    private Enemy enemyScript;
    private Collider2D bossCollider;
    private bool canUseSkill = false;
    private bool isJumping = false;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        bossCollider = GetComponent<Collider2D>();
        timer = skillCooldown;

        if (GetComponent<BossDescending>() == null)
        {
            canUseSkill = true;
        }
    }

    public void StartSkillRoutine()
    {
        canUseSkill = true;
        timer = skillCooldown;
    }

    void Update()
    {
        if (!canUseSkill || isJumping) return;
        if (enemyScript != null && enemyScript.currentHP <= 0) return;
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = skillCooldown;
            StartCoroutine(JumpRoutine());
        }
    }

    IEnumerator JumpRoutine()
    {
        isJumping = true;

        if (enemyScript != null) enemyScript.enabled = false;
        if (bossCollider != null) bossCollider.enabled = false;

        Vector3 startPos = transform.position;
        Vector3 peakPos = startPos + new Vector3(0, jumpHeight, 0);

        while (transform.position.y < peakPos.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, peakPos, jumpUpSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(hangTime);

        while (transform.position.y > startPos.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, slamDownSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = startPos;

        // ★追加：着地した瞬間に衝撃波エフェクトを発生させる！
        if (shockwaveEffectPrefab != null)
        {
            GameObject fx = Instantiate(shockwaveEffectPrefab, transform.position, Quaternion.identity);
            fx.transform.localScale = Vector3.one * effectScale;
            // エフェクトがずっと残り続けないように、1.5秒後に自動で消す
            Destroy(fx, 1.5f);
        }

        if (GameManager.instance != null && !GameManager.instance.isGameOver)
        {
            GameManager.instance.currentHP -= shockwaveDamage;
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                if (enemyScript != null && enemyScript.damageTextPrefab != null)
                {
                    Vector3 popupPos = player.transform.position + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0f);
                    GameObject textObj = Instantiate(enemyScript.damageTextPrefab, popupPos, Quaternion.identity);
                    DamageText popup = textObj.GetComponent<DamageText>();
                    if (popup != null) popup.Setup(Mathf.RoundToInt(shockwaveDamage).ToString(), Color.yellow);
                }

                StartCoroutine(PushBackPlayer(player.transform));
            }
        }

        yield return new WaitForSeconds(1.0f);
        if (enemyScript != null) enemyScript.enabled = true;
        if (bossCollider != null) bossCollider.enabled = true;

        isJumping = false;
    }

    IEnumerator PushBackPlayer(Transform playerTransform)
    {
        Vector3 playerStartPos = playerTransform.position;
        Vector3 targetPos = playerStartPos;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 dir = (playerStartPos - transform.position).normalized;

            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;
            float margin = 1.0f;

            targetPos = playerStartPos + dir * 15.0f;
            targetPos.x = Mathf.Clamp(targetPos.x, -camWidth + margin, camWidth - margin);
            targetPos.y = Mathf.Clamp(targetPos.y, -camHeight + margin, camHeight - margin);
        }

        float elapsed = 0f;
        while (elapsed < pushbackTime)
        {
            if (playerTransform == null) break;
            elapsed += Time.deltaTime;

            float t = elapsed / pushbackTime;
            t = 1f - Mathf.Pow(1f - t, 4f);

            playerTransform.position = Vector3.Lerp(playerStartPos, targetPos, t);
            yield return null;
        }
    }
}