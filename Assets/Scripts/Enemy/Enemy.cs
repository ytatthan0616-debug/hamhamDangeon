using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    // ★軽量化：生存している全敵キャラの名簿（シーン全体から探す処理をゼロにするため）
    public static List<Enemy> activeEnemies = new List<Enemy>();

    [Header("基礎ステータス（Lv1 / WAVE1）")]
    public float maxHP = 30f;
    [HideInInspector] public float currentHP;
    public float moveSpeed = 1.0f;

    [Header("攻撃設定")]
    public float touchDamage = 5f;
    public float attackRadius = 1.0f;

    [Header("成長率設定")]
    public float hpGrowthPerWave = 0.1f;
    public float damageGrowthPerWave = 0.05f;

    [Header("ボス設定")]
    public bool isBoss = false;
    public bool isRangedBoss = false;
    public bool hasBarrier = true;
    public float keepDistance = 6.0f;
    private Vector3 baseScale;

    [Header("アニメーション設定")]
    public bool enableBounce = true;
    public bool enableWalkSway = false;
    public float swayAngle = 6.0f;
    public float swaySpeed = 12.0f;

    [Header("ダメージエフェクト設定")]
    public GameObject damageTextPrefab;

    [Header("移動設定")]
    public float stopDistance = 0.8f;
    private Vector2 targetOffset;
    private float offsetChangeTimer = 0f;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private bool isDead = false;
    private Transform player;

    private float bounceTimer = 0f;
    private float personalBounceSpeed = 8f;
    private float damageCooldown = 0f;

    private enum EnemyState { Idle, Wander, Chase }
    private EnemyState currentState = EnemyState.Idle;
    private Vector2 wanderTarget;
    private float stateTimer = 0f;

    private float outsideTimer = 0f;
    private bool hasEnteredScreen = false;
    private Transform visualTransform;
    private Camera mainCam;

    // ★軽量化：生まれたら名簿に追加、消える時に名簿から削除
    void OnEnable() { activeEnemies.Add(this); }
    void OnDisable() { activeEnemies.Remove(this); }

    void Awake()
    {
        SpriteRenderer originalRenderer = GetComponent<SpriteRenderer>();
        if (originalRenderer != null && originalRenderer.transform == this.transform)
        {
            GameObject visualObj = new GameObject("EnemyVisual");
            visualObj.transform.SetParent(this.transform);
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = Vector3.one;

            spriteRenderer = visualObj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = originalRenderer.sprite;
            spriteRenderer.color = originalRenderer.color;
            spriteRenderer.sortingLayerID = originalRenderer.sortingLayerID;
            spriteRenderer.sortingOrder = originalRenderer.sortingOrder;

            originalRenderer.enabled = false;
            visualTransform = visualObj.transform;
        }
    }

    void Start()
    {
        if (GameManager.instance != null)
        {
            int wave = GameManager.instance.currentWave;
            float hpMult = 1.0f + ((wave - 1) * hpGrowthPerWave);
            float dmgMult = 1.0f + ((wave - 1) * damageGrowthPerWave);

            maxHP = maxHP * hpMult;
            touchDamage = touchDamage * dmgMult;
        }

        currentHP = maxHP;

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (visualTransform == null && spriteRenderer != null) visualTransform = spriteRenderer.transform;

        col = GetComponent<Collider2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        targetOffset = Random.insideUnitCircle.normalized * Random.Range(0.5f, stopDistance);
        offsetChangeTimer = Random.Range(1.0f, 3.0f);
        baseScale = transform.localScale;

        mainCam = Camera.main;

        if (!isBoss)
        {
            bounceTimer = Random.Range(0f, Mathf.PI * 2f);
            personalBounceSpeed = Random.Range(6.5f, 9.5f);
            moveSpeed *= Random.Range(0.85f, 1.15f);

            if (spriteRenderer != null)
            {
                Color.RGBToHSV(spriteRenderer.color, out float h, out float s, out float v);
                h += Random.Range(-0.08f, 0.08f);
                if (h < 0f) h += 1f;
                if (h > 1f) h -= 1f;

                s *= Random.Range(0.7f, 1.0f);
                v *= Random.Range(0.7f, 1.0f);

                spriteRenderer.color = Color.HSVToRGB(h, s, v);
            }
        }
        else
        {
            personalBounceSpeed = 8f;
        }
    }

    void Update()
    {
        if (isDead) return;
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        if (player != null)
        {
            // ★軽量化：平方根計算（sqrMagnitude）で距離チェックを軽くする
            float distToPlayerSqr = (transform.position - player.position).sqrMagnitude;
            if (distToPlayerSqr <= attackRadius * attackRadius)
            {
                if (damageCooldown <= 0f)
                {
                    DealDamageToPlayer();
                    damageCooldown = 1.0f;
                }
            }
        }

        if (damageCooldown > 0f) damageCooldown -= Time.deltaTime;

        Camera cam = mainCam;
        bool isOutside = false;

        if (cam != null)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;
            float margin = 1.5f;

            if (Mathf.Abs(transform.position.x) > (camWidth - margin) ||
                Mathf.Abs(transform.position.y) > (camHeight - margin))
            {
                isOutside = true;
            }

            if (col != null)
            {
                if (!hasEnteredScreen)
                {
                    if (isOutside) col.isTrigger = true;
                    else
                    {
                        col.isTrigger = false;
                        hasEnteredScreen = true;
                    }
                }
                else
                {
                    if (Mathf.Abs(transform.position.x) > camWidth || Mathf.Abs(transform.position.y) > camHeight)
                    {
                        col.isTrigger = true;
                        hasEnteredScreen = false;
                    }
                    else col.isTrigger = false;
                }
            }
        }

        if (isOutside)
        {
            outsideTimer += Time.deltaTime;
            if (outsideTimer >= 3.0f) currentState = EnemyState.Chase;
        }
        else outsideTimer = 0f;

        if (enableBounce)
        {
            bounceTimer += Time.deltaTime * personalBounceSpeed;
            float bounceY = Mathf.Abs(Mathf.Sin(bounceTimer)) * 0.3f;

            if (visualTransform != null && visualTransform != transform)
            {
                visualTransform.localPosition = new Vector3(0, bounceY, 0);
                visualTransform.localRotation = Quaternion.identity;
                transform.localScale = baseScale;
            }
        }
        else if (enableWalkSway)
        {
            bounceTimer += Time.deltaTime * (isBoss ? swaySpeed : swaySpeed * (personalBounceSpeed / 8f));
            float currentAngle = Mathf.Sin(bounceTimer) * swayAngle;

            if (visualTransform != null && visualTransform != transform)
            {
                visualTransform.localPosition = Vector3.zero;
                visualTransform.localRotation = Quaternion.Euler(0, 0, currentAngle);
                transform.localScale = baseScale;
            }
        }
        else
        {
            if (visualTransform != null && visualTransform != transform)
            {
                visualTransform.localPosition = Vector3.zero;
                visualTransform.localRotation = Quaternion.identity;
            }
            transform.localScale = baseScale;
        }

        if (isBoss) currentState = EnemyState.Chase;
        else
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f && outsideTimer < 3.0f)
            {
                int rand = Random.Range(0, 10);
                if (rand < 2) currentState = EnemyState.Idle;
                else if (rand < 5)
                {
                    currentState = EnemyState.Wander;
                    wanderTarget = transform.position + (Vector3)(Random.insideUnitCircle * 3f);
                }
                else currentState = EnemyState.Chase;

                stateTimer = Random.Range(1.0f, 3.0f);
            }
        }

        switch (currentState)
        {
            case EnemyState.Wander:
                transform.position = Vector2.MoveTowards(transform.position, wanderTarget, moveSpeed * 0.5f * Time.deltaTime);
                break;
            case EnemyState.Chase:
                if (player == null) break;

                offsetChangeTimer -= Time.deltaTime;
                if (offsetChangeTimer <= 0f)
                {
                    targetOffset = Random.insideUnitCircle.normalized * Random.Range(0.5f, stopDistance);
                    offsetChangeTimer = Random.Range(1.0f, 3.0f);
                }

                float distToPlayer = Vector2.Distance(transform.position, player.position);

                if (isRangedBoss)
                {
                    Vector3 newPos = transform.position;

                    if (distToPlayer < keepDistance)
                    {
                        Vector3 direction = (transform.position - player.position).normalized;
                        newPos += direction * moveSpeed * Time.deltaTime;
                    }
                    else if (distToPlayer > keepDistance + 1.0f)
                    {
                        Vector3 direction = (player.position - transform.position).normalized;
                        newPos += direction * moveSpeed * Time.deltaTime;
                    }
                    else
                    {
                        Vector3 trueTarget = player.position + (Vector3)targetOffset;
                        Vector3 direction = (trueTarget - transform.position).normalized;
                        newPos += direction * (moveSpeed * 0.5f) * Time.deltaTime;
                    }

                    if (cam != null)
                    {
                        float camHeight = cam.orthographicSize;
                        float camWidth = camHeight * cam.aspect;
                        newPos.x = Mathf.Clamp(newPos.x, -camWidth + 1.5f, camWidth - 1.5f);
                        newPos.y = Mathf.Clamp(newPos.y, -camHeight + 1.5f, camHeight - 1.5f);
                    }
                    transform.position = newPos;
                }
                else
                {
                    Vector3 trueTarget = player.position + (Vector3)targetOffset;
                    if (distToPlayer > 3.0f) trueTarget = player.position;

                    float distToTarget = Vector2.Distance(transform.position, trueTarget);

                    if (distToTarget > 0.1f && distToPlayer < 25f)
                    {
                        Vector3 direction = (trueTarget - transform.position).normalized;
                        transform.position += direction * moveSpeed * Time.deltaTime;
                    }
                }
                break;
            case EnemyState.Idle:
                break;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;

        if (GameManager.instance != null && GameManager.instance.showDamagePopup && damageTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0f);
            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            DamageText popup = textObj.GetComponent<DamageText>();
            if (popup != null) popup.Setup(Mathf.RoundToInt(damage).ToString(), Color.red);
        }

        if (spriteRenderer != null) StartCoroutine(FlashTransparent());
        if (currentHP <= 0) Die();
    }

    private void DealDamageToPlayer()
    {
        if (GameManager.instance != null && !isDead)
        {
            GameManager.instance.currentHP -= touchDamage;
        }
    }

    private IEnumerator FlashTransparent()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.4f);
            yield return new WaitForSeconds(0.1f);
            if (!isDead && spriteRenderer != null) spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (col != null) col.enabled = false;

        // ★軽量化：死んだ瞬間に名簿から外す（追尾対象から外れる）
        activeEnemies.Remove(this);

        int scoreValue = Mathf.FloorToInt(maxHP * 0.5f + (GameManager.instance.currentWave * 10));
        GameManager.instance.AddScore(scoreValue);

        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float fadeDuration = 1.0f;
        float timer = 0f;

        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                if (spriteRenderer != null) spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }
        else yield return new WaitForSeconds(fadeDuration);

        Destroy(gameObject);
    }
}