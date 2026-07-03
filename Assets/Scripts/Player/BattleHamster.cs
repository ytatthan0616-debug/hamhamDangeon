using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
// ★軽量化：using System.Linq; を削除（LINQはメモリを食うため使わない！）

public class BattleHamster : MonoBehaviour
{
    [Header("現在のステータス")]
    public float currentHp;
    public float attackPower;
    public float intelligence;
    public int level;

    public enum HamsterType { Normal, Smart, Tough, Fighter }
    [Header("AI設定")]
    public HamsterType myType;
    public float moveSpeed = 2.0f;
    public float centripetalForce = 1.0f;
    public float centripetalThreshold = 3.0f;

    [Header("防御・回避システム")]
    public float bossBarrierRadius = 2.5f;
    public float bossPushForce = 15.0f;
    public float warpCooldown = 5.0f;
    public float evadeSpeed = 12.0f;
    public float evadeDistance = 4.0f;

    private float warpTimer = 0f;
    private float previousHP;
    private bool isEvading = false;

    [Header("プレハブ・ターゲット")]
    public GameObject slashPrefab;
    public GameObject magicPrefab;
    public ParticleSystem tackleEffectPrefab;
    public GameObject targetMarkerPrefab;
    public GameObject damageTextPrefab;

    [Header("★新スキル用プレハブ")]
    public GameObject poisonPrefab;
    public GameObject heartBreakPrefab;
    public GameObject breathPrefab;
    public GameObject crashPrefab;
    public GameObject explodingMobPrefab;
    public GameObject mobExplosionPrefab;

    private GameObject currentMarker;
    private Transform clickedTarget;

    [System.Serializable]
    public class TackleTier { public float cooldown = 2.0f; public float dashSpeed = 15f; public float hitboxWidth = 2.0f; public int targetCount = 1; public float effectScale = 1.0f; }
    [System.Serializable]
    public class MagicTier { public float cooldown = 1.5f; public int targetCount = 1; public float effectScale = 1.0f; }
    [System.Serializable]
    public class SwordTier { public float cooldown = 1.0f; public float attackRange = 5.0f; public int targetCount = 1; public float effectScale = 1.0f; public float damageMultiplier = 1.0f; }
    [System.Serializable]
    public class PoisonTier { public float cooldown = 8.0f; public float duration = 5.0f; public float radius = 3.0f; public float damageTickMult = 0.2f; public float effectScale = 1.0f; }
    [System.Serializable]
    public class HeartBreakTier { public float cooldown = 10.0f; public float delay = 1.5f; public float radius = 1.5f; public float damageMultiplier = 5.0f; public float effectScale = 1.0f; }
    [System.Serializable]
    public class BreathTier { public float cooldown = 6.0f; public float duration = 2.5f; public float range = 5.0f; public float damageTickMult = 0.5f; public float effectScale = 1.0f; }
    [System.Serializable]
    public class CrashTier { public float cooldown = 5.0f; public float range = 3.0f; public float damageMultiplier = 3.0f; public float effectScale = 1.0f; }
    [System.Serializable]
    public class ExplodingMobTier { public float cooldown = 12.0f; public int minCount = 1; public int maxCount = 3; public float lifeTime = 3.0f; public float explosionRadius = 2.0f; public float damageMultiplier = 2.0f; public float effectScale = 1.0f; }

    [System.Serializable]
    public class SkillSet
    {
        public bool useTackle, useMagic, useSword, usePoison, useHeartBreak, useBreath, useCrash, useExplodingMob;
        public TackleTier tackleSettings = new TackleTier();
        public MagicTier magicSettings = new MagicTier();
        public SwordTier swordSettings = new SwordTier();
        public PoisonTier poisonSettings = new PoisonTier();
        public HeartBreakTier heartBreakSettings = new HeartBreakTier();
        public BreathTier breathSettings = new BreathTier();
        public CrashTier crashSettings = new CrashTier();
        public ExplodingMobTier explodingMobSettings = new ExplodingMobTier();
    }

    [Header("進化段階ごとのスキル設定")]
    public SkillSet[] smartSkills = new SkillSet[5];
    public SkillSet[] toughSkills = new SkillSet[5];
    public SkillSet[] fighterSkills = new SkillSet[5];

    [Header("基本攻撃設定（見習い用）")]
    public float attackInterval = 1.5f;

    private float normalTimer = 0f;
    private float tackleTimer = 0f;
    private float magicTimer = 0f;
    private float swordTimer = 0f;
    private float poisonTimer = 0f;
    private float heartBreakTimer = 0f;
    private float breathTimer = 0f;
    private float crashTimer = 0f;
    private float explodingMobTimer = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform nearestEnemy;
    private float searchTimer = 0f;
    private enum ActionState { Approach, Flee, Wander, ReturnToCenter }
    private ActionState currentState = ActionState.Wander;
    private float actionTimer = 0f;
    private Vector2 wanderDirection;
    private Transform visualTransform;
    private SpriteRenderer visualRenderer;
    private float bounceTimer = 0f;
    private bool isDead = false;
    private bool isTackling = false;
    private bool isChanneling = false;

    // ★軽量化：一時計算用のリスト（毎回newしない）
    private List<Enemy> targetCache = new List<Enemy>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        SpriteRenderer originalRenderer = GetComponent<SpriteRenderer>();
        if (originalRenderer != null)
        {
            GameObject visualObj = new GameObject("HamsterVisual");
            visualObj.transform.SetParent(this.transform);
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = Vector3.one;
            visualObj.transform.localRotation = Quaternion.identity;
            visualRenderer = visualObj.AddComponent<SpriteRenderer>();

            Sprite defaultSprite = originalRenderer.sprite;
            if (defaultSprite == null && GameManager.instance != null && GameManager.instance.normalForm != null)
                defaultSprite = GameManager.instance.normalForm.sprite;
            visualRenderer.sprite = defaultSprite;
            visualRenderer.color = originalRenderer.color;
            visualRenderer.sortingLayerID = originalRenderer.sortingLayerID;
            visualRenderer.sortingOrder = originalRenderer.sortingOrder;
            visualRenderer.material = originalRenderer.material;
            originalRenderer.enabled = false;
            visualTransform = visualObj.transform;
            spriteRenderer = visualRenderer;
        }
    }

    void Start()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.RegisterHamster(spriteRenderer);
            GameManager.instance.UpdateDungeonHpUI();
            currentHp = GameManager.instance.maxHP;
            attackPower = GameManager.instance.attackPower;
            intelligence = GameManager.instance.intelligence;
            level = GameManager.instance.currentLevel;
            DetermineType();

            previousHP = GameManager.instance.currentHP;
        }

        FindNearestEnemy();
        DecideNextAction();
        searchTimer = 0f;
    }

    void Update()
    {
        if (GameManager.instance != null)
        {
            if (GameManager.instance.isGameOver)
            {
                if (!isDead)
                {
                    isDead = true;
                    isTackling = false;
                    isChanneling = false;
                    rb.linearVelocity = Vector2.zero;
                }
                return;
            }
            else if (isDead)
            {
                isDead = false;
                transform.position = new Vector3(0f, -3.5f, 0f);
            }

            if (warpTimer > 0f) warpTimer -= Time.deltaTime;

            float currentHpNow = GameManager.instance.currentHP;
            if (currentHpNow < previousHP)
            {
                float damageTaken = previousHP - currentHpNow;
                ShowDamagePopup(damageTaken);
                StartCoroutine(FlashDamageEffect());

                if (warpTimer <= 0f && !isTackling && !isChanneling && !isEvading)
                {
                    StartCoroutine(EvadeRoutine());
                    warpTimer = warpCooldown;
                }
            }
            previousHP = currentHpNow;
        }

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            bool isPointerOverUI = false;
            if (EventSystem.current != null)
            {
                if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0) isPointerOverUI = EventSystem.current.IsPointerOverGameObject(Touchscreen.current.touches[0].touchId.ReadValue());
                else isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
            }

            if (!isPointerOverUI)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector2 pointerPos = Pointer.current.position.ReadValue();
                    Vector2 mousePos = cam.ScreenToWorldPoint(pointerPos);
                    Collider2D hit = Physics2D.OverlapPoint(mousePos);
                    if (hit != null && hit.CompareTag("Enemy")) SetClickedTarget(hit.transform);
                }
            }
        }

        if (!isTackling && !isChanneling && !isEvading)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= 0.5f) { FindNearestEnemy(); searchTimer = 0f; }
            actionTimer -= Time.deltaTime;
            if (actionTimer <= 0f) DecideNextAction();
            MoveTopDown();
        }
        else if (isChanneling || isEvading) rb.linearVelocity = Vector2.zero;

        UpdateAttacks();
        ApplyBossBarrier();
    }

    void ShowDamagePopup(float damageAmount)
    {
        if (damageTextPrefab != null && GameManager.instance != null && GameManager.instance.showDamagePopup)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0f);
            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            DamageText popup = textObj.GetComponent<DamageText>();
            if (popup != null) popup.Setup(Mathf.RoundToInt(damageAmount).ToString(), Color.yellow);
        }
    }

    private IEnumerator FlashDamageEffect()
    {
        if (visualRenderer != null)
        {
            Color originalColor = visualRenderer.color;
            visualRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.4f);
            yield return new WaitForSeconds(0.1f);
            if (!isDead && visualRenderer != null)
                visualRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }
    }

    void ApplyBossBarrier()
    {
        Vector2 pushVelocity = Vector2.zero;
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, bossBarrierRadius);
        foreach (Collider2D c in cols)
        {
            if (c.CompareTag("Enemy"))
            {
                Enemy enemyScript = c.GetComponent<Enemy>();
                if (enemyScript != null && enemyScript.isBoss && enemyScript.hasBarrier && enemyScript.currentHP > 0)
                {
                    Vector3 diff = transform.position - c.transform.position;
                    float dist = diff.magnitude;
                    if (dist < bossBarrierRadius && dist > 0.01f)
                    {
                        float pushPower = (bossBarrierRadius - dist) * bossPushForce;
                        pushVelocity += (Vector2)diff.normalized * pushPower;
                    }
                }
            }
        }
        if (pushVelocity != Vector2.zero) rb.linearVelocity += pushVelocity;
    }

    IEnumerator EvadeRoutine()
    {
        // 変更なしのため省略（そのままにしてあります）
        isEvading = true;
        rb.linearVelocity = Vector2.zero;
        Camera cam = Camera.main;
        if (cam != null)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;
            float margin = 1.0f;
            float currentSpeed = evadeSpeed;
            float currentDist = evadeDistance;
            bool isWarp = false;

            if (myType == HamsterType.Smart) { if (GameManager.instance != null && GameManager.instance.evolutionTier >= 4) { currentDist = 5.0f; isWarp = true; } else { currentSpeed = 15.0f; currentDist = 2.0f; isWarp = false; } }
            else if (myType == HamsterType.Tough) { currentSpeed = 4.0f; currentDist = 0.5f; }
            else if (myType == HamsterType.Fighter) { currentSpeed = 8.0f; currentDist = 3.0f; }

            Vector2 bestTargetPos = transform.position;
            float bestScore = float.MinValue;
            float randomOffset = Random.Range(0f, 360f);

            for (int i = 0; i < 8; i++)
            {
                float angle = (i * 45f + randomOffset) * Mathf.Deg2Rad;
                Vector2 testDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 testPos = (Vector2)transform.position + testDir * currentDist;
                float clampedX = Mathf.Clamp(testPos.x, -camWidth + margin, camWidth - margin);
                float clampedY = Mathf.Clamp(testPos.y, -camHeight + margin, camHeight - margin);
                Vector2 clampedPos = new Vector2(clampedX, clampedY);
                float travelDist = Vector2.Distance(transform.position, clampedPos);
                float distToEdgeX = Mathf.Min(Mathf.Abs(clampedPos.x - (-camWidth + margin)), Mathf.Abs((camWidth - margin) - clampedPos.x));
                float distToEdgeY = Mathf.Min(Mathf.Abs(clampedPos.y - (-camHeight + margin)), Mathf.Abs((camHeight - margin) - clampedPos.y));
                float minEdgeDist = Mathf.Min(distToEdgeX, distToEdgeY);
                float score = (travelDist * 2f) + minEdgeDist;

                if (score > bestScore) { bestScore = score; bestTargetPos = clampedPos; }
            }

            Vector2 targetPos = bestTargetPos;

            if (isWarp)
            {
                if (visualRenderer != null) visualRenderer.enabled = false;
                yield return new WaitForSeconds(0.1f);
                transform.position = targetPos;
                if (visualRenderer != null) visualRenderer.enabled = true;
            }
            else
            {
                while (Vector2.Distance(transform.position, targetPos) > 0.1f)
                {
                    if (GameManager.instance != null && GameManager.instance.isGameOver) break;
                    Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
                    if (direction.x > 0.1f) { if (visualRenderer != null) visualRenderer.flipX = true; } else if (direction.x < -0.1f) { if (visualRenderer != null) visualRenderer.flipX = false; }
                    bounceTimer += Time.deltaTime * (currentSpeed * 1.5f);
                    float bounceY = Mathf.Abs(Mathf.Sin(bounceTimer)) * 0.3f;
                    if (visualTransform != null) visualTransform.localPosition = new Vector3(0f, bounceY, 0f);
                    transform.position = Vector2.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);
                    yield return null;
                }
            }
        }
        if (visualTransform != null) visualTransform.localPosition = Vector3.zero;
        currentState = ActionState.Wander;
        isEvading = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, bossBarrierRadius);
    }

    void SetClickedTarget(Transform target)
    {
        clickedTarget = target;
        if (currentMarker != null) Destroy(currentMarker);
        if (targetMarkerPrefab != null) currentMarker = Instantiate(targetMarkerPrefab, target.position, Quaternion.identity, target);
    }

    // ★軽量化：名簿(Enemy.activeEnemies)を使って最速で検索
    void FindNearestEnemy()
    {
        if (clickedTarget != null)
        {
            Enemy e = clickedTarget.GetComponent<Enemy>();
            if (e != null && e.currentHP > 0) { nearestEnemy = clickedTarget; return; }
            else { clickedTarget = null; if (currentMarker != null) Destroy(currentMarker); }
        }

        float minDistSqr = float.MaxValue;
        Enemy closest = null;
        Vector2 myPos = transform.position;

        for (int i = 0; i < Enemy.activeEnemies.Count; i++)
        {
            Enemy e = Enemy.activeEnemies[i];
            if (e == null || e.currentHP <= 0) continue;

            float distSqr = (myPos - (Vector2)e.transform.position).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                minDistSqr = distSqr;
                closest = e;
            }
        }
        nearestEnemy = (closest != null) ? closest.transform : null;
    }

    void Die() { isDead = true; rb.linearVelocity = Vector2.zero; if (visualTransform != null) visualTransform.localPosition = Vector3.zero; Collider2D col = GetComponent<Collider2D>(); if (col != null) col.enabled = false; StartCoroutine(FadeOutAndDisappear()); }
    private IEnumerator FadeOutAndDisappear() { if (visualRenderer != null) { float fadeDuration = 1.0f; float timer = 0f; Color startColor = visualRenderer.color; while (timer < fadeDuration) { timer += Time.deltaTime; float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration); visualRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha); yield return null; } visualRenderer.enabled = false; } }
    void DetermineType() { if (level < 10) myType = HamsterType.Normal; else { if (GameManager.instance.currentEvolution == GameManager.EvolvedType.Smart) myType = HamsterType.Smart; else if (GameManager.instance.currentEvolution == GameManager.EvolvedType.Tough) myType = HamsterType.Tough; else myType = HamsterType.Fighter; } }

    void DecideNextAction()
    {
        actionTimer = Random.Range(1.0f, 2.5f);
        float distFromCenter = Vector2.Distance(transform.position, Vector2.zero);

        if (distFromCenter > 4.5f || Random.value < 0.35f)
        {
            currentState = ActionState.ReturnToCenter;
            return;
        }

        if (nearestEnemy == null)
        {
            currentState = ActionState.Wander;
            wanderDirection = Random.insideUnitCircle.normalized;
            return;
        }

        float distance = Vector2.Distance(transform.position, nearestEnemy.position);

        switch (myType)
        {
            case HamsterType.Fighter:
            case HamsterType.Tough:
                if (Random.value > 0.4f) currentState = ActionState.Approach;
                else currentState = ActionState.Wander;
                break;
            case HamsterType.Smart:
            case HamsterType.Normal:
                if (distance < 5f && Random.value > 0.3f) currentState = ActionState.Flee;
                else currentState = ActionState.Wander;
                break;
        }

        if (currentState == ActionState.Wander)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector2 dirToCenter = (Vector2.zero - (Vector2)transform.position).normalized;
            wanderDirection = (randomDir + dirToCenter * 0.5f).normalized;
        }
    }

    void MoveTopDown()
    {
        Vector2 velocity = Vector2.zero;

        if (currentState == ActionState.ReturnToCenter)
        {
            float distToCenter = Vector2.Distance(transform.position, Vector2.zero);
            if (distToCenter > 1.0f)
            {
                Vector2 dirToCenter = (Vector2.zero - (Vector2)transform.position).normalized;
                velocity = dirToCenter * (moveSpeed * 0.8f);
            }
            else DecideNextAction();
        }
        else if (nearestEnemy != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, nearestEnemy.position);
            Vector2 dirToEnemy = (nearestEnemy.position - transform.position).normalized;
            if (currentState == ActionState.Approach)
            {
                if (distanceToEnemy > 0.5f) velocity = dirToEnemy * moveSpeed;
            }
            else if (currentState == ActionState.Flee)
            {
                velocity = -dirToEnemy * (moveSpeed * 1.5f);
            }
        }

        if (currentState == ActionState.Wander) velocity = wanderDirection * (moveSpeed * 0.7f);

        float distFromCenter = Vector2.Distance(transform.position, Vector2.zero);
        bool isStickingToEnemy = (nearestEnemy != null && Vector2.Distance(transform.position, nearestEnemy.position) < 1.0f);

        if (distFromCenter > centripetalThreshold && !isStickingToEnemy)
        {
            Vector2 dirToCenter = (Vector2.zero - (Vector2)transform.position).normalized;
            velocity += dirToCenter * centripetalForce;
        }

        if (velocity.x > 0.2f) { if (visualRenderer != null) visualRenderer.flipX = true; } else if (velocity.x < -0.2f) { if (visualRenderer != null) visualRenderer.flipX = false; }

        bounceTimer += Time.deltaTime * 8f;
        float bounceY = Mathf.Abs(Mathf.Sin(bounceTimer)) * 0.3f;
        if (visualTransform != null) visualTransform.localPosition = new Vector3(0f, bounceY, 0f);

        rb.linearVelocity = velocity;
    }

    void UpdateAttacks()
    {
        if (GameManager.instance == null) return;
        if (isChanneling || isTackling || isEvading) return;

        normalTimer += Time.deltaTime;
        tackleTimer += Time.deltaTime;
        magicTimer += Time.deltaTime;
        swordTimer += Time.deltaTime;
        poisonTimer += Time.deltaTime;
        heartBreakTimer += Time.deltaTime;
        breathTimer += Time.deltaTime;
        crashTimer += Time.deltaTime;
        explodingMobTimer += Time.deltaTime;

        if (nearestEnemy == null) return;

        if (level < 10 || GameManager.instance.evolutionTier == 0)
        {
            if (normalTimer >= attackInterval) { PerformNormalAttack(); normalTimer = 0f; }
            return;
        }

        int tierIndex = Mathf.Clamp(GameManager.instance.evolutionTier - 1, 0, 4);
        SkillSet currentSkillSet = null;

        if (myType == HamsterType.Smart && tierIndex < smartSkills.Length) currentSkillSet = smartSkills[tierIndex];
        else if (myType == HamsterType.Tough && tierIndex < toughSkills.Length) currentSkillSet = toughSkills[tierIndex];
        else if (myType == HamsterType.Fighter && tierIndex < fighterSkills.Length) currentSkillSet = fighterSkills[tierIndex];

        if (currentSkillSet == null)
        {
            if (normalTimer >= attackInterval) { PerformNormalAttack(); normalTimer = 0f; }
            return;
        }

        if (currentSkillSet.useBreath && breathTimer >= currentSkillSet.breathSettings.cooldown) { StartCoroutine(SpawnBreath(currentSkillSet.breathSettings)); breathTimer = 0f; return; }
        if (currentSkillSet.useTackle && tackleTimer >= currentSkillSet.tackleSettings.cooldown) { ExecuteTackle(currentSkillSet.tackleSettings); tackleTimer = 0f; return; }
        if (currentSkillSet.useMagic && magicTimer >= currentSkillSet.magicSettings.cooldown) { ExecuteMagic(currentSkillSet.magicSettings); magicTimer = 0f; }
        if (currentSkillSet.useSword && swordTimer >= currentSkillSet.swordSettings.cooldown) { ExecuteSword(currentSkillSet.swordSettings); swordTimer = 0f; }
        if (currentSkillSet.usePoison && poisonTimer >= currentSkillSet.poisonSettings.cooldown) { SpawnPoison(currentSkillSet.poisonSettings); poisonTimer = 0f; }
        if (currentSkillSet.useHeartBreak && heartBreakTimer >= currentSkillSet.heartBreakSettings.cooldown) { SpawnHeartBreak(currentSkillSet.heartBreakSettings); heartBreakTimer = 0f; }
        if (currentSkillSet.useCrash && crashTimer >= currentSkillSet.crashSettings.cooldown) { SpawnCrash(currentSkillSet.crashSettings); crashTimer = 0f; }
        if (currentSkillSet.useExplodingMob && explodingMobTimer >= currentSkillSet.explodingMobSettings.cooldown) { SpawnExplodingMob(currentSkillSet.explodingMobSettings); explodingMobTimer = 0f; }
    }

    // ★軽量化：独自ソート関数（LINQを使わず、クリック優先＆距離順にソートする超速処理）
    int SortEnemiesByDistance(Enemy a, Enemy b)
    {
        bool aClicked = (clickedTarget != null && a.transform == clickedTarget);
        bool bClicked = (clickedTarget != null && b.transform == clickedTarget);
        if (aClicked && !bClicked) return -1;
        if (!aClicked && bClicked) return 1;
        float distA = (transform.position - a.transform.position).sqrMagnitude;
        float distB = (transform.position - b.transform.position).sqrMagnitude;
        return distA.CompareTo(distB);
    }

    void SpawnCrash(CrashTier tier)
    {
        if (nearestEnemy == null) return;
        targetCache.Clear();

        float rangeSqr = tier.range * tier.range;
        for (int i = 0; i < Enemy.activeEnemies.Count; i++)
        {
            Enemy e = Enemy.activeEnemies[i];
            if ((transform.position - e.transform.position).sqrMagnitude <= rangeSqr)
            {
                targetCache.Add(e);
            }
        }

        if (targetCache.Count > 0 && crashPrefab != null)
        {
            targetCache.Sort(SortEnemiesByDistance);
            Enemy target = targetCache[0];
            GameObject fx = Instantiate(crashPrefab, target.transform.position, Quaternion.identity);
            fx.transform.localScale = Vector3.one * tier.effectScale;
            CrashEffect script = fx.GetComponent<CrashEffect>();
            if (script != null) script.Initialize(target, attackPower * tier.damageMultiplier);
        }
    }

    void ExecuteMagic(MagicTier tier)
    {
        if (Enemy.activeEnemies.Count == 0) return;

        targetCache.Clear();
        targetCache.AddRange(Enemy.activeEnemies);
        targetCache.Sort(SortEnemiesByDistance);

        int count = Mathf.Min(tier.targetCount, targetCache.Count);
        for (int i = 0; i < count; i++)
        {
            Transform target = targetCache[i].transform;
            if (magicPrefab != null)
            {
                GameObject magic = Instantiate(magicPrefab, transform.position, Quaternion.identity);
                magic.transform.localScale = Vector3.one * tier.effectScale;
                MagicProjectile script = magic.GetComponent<MagicProjectile>();
                if (script != null) script.SetTarget(target);
            }
        }
    }

    void ExecuteSword(SwordTier tier)
    {
        if (Enemy.activeEnemies.Count == 0) return;

        targetCache.Clear();
        float rangeSqr = tier.attackRange * tier.attackRange;
        for (int i = 0; i < Enemy.activeEnemies.Count; i++)
        {
            if ((transform.position - Enemy.activeEnemies[i].transform.position).sqrMagnitude <= rangeSqr)
            {
                targetCache.Add(Enemy.activeEnemies[i]);
            }
        }

        int count = Mathf.Min(tier.targetCount, targetCache.Count);
        if (count == 0) return;

        targetCache.Sort(SortEnemiesByDistance);

        for (int i = 0; i < count; i++)
        {
            Transform target = targetCache[i].transform;
            if (slashPrefab != null)
            {
                float randomAngle = Random.Range(0f, 360f);
                GameObject effect = Instantiate(slashPrefab, target.position, Quaternion.Euler(0, 0, randomAngle));
                effect.transform.localScale = Vector3.one * tier.effectScale;
                MeleeAttackEffect effectScript = effect.GetComponent<MeleeAttackEffect>();
                if (effectScript != null) effectScript.damage = attackPower * tier.damageMultiplier;
            }
        }
    }

    void ExecuteTackle(TackleTier tier)
    {
        if (Enemy.activeEnemies.Count == 0) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        targetCache.Clear();
        Vector2 camPos = cam.transform.position;

        for (int i = 0; i < Enemy.activeEnemies.Count; i++)
        {
            Vector2 ePos = Enemy.activeEnemies[i].transform.position;
            if (ePos.x >= camPos.x - camWidth + 0.5f && ePos.x <= camPos.x + camWidth - 0.5f &&
                ePos.y >= camPos.y - camHeight + 0.5f && ePos.y <= camPos.y + camHeight - 0.5f)
            {
                targetCache.Add(Enemy.activeEnemies[i]);
            }
        }

        if (targetCache.Count == 0) return;
        targetCache.Sort(SortEnemiesByDistance);

        int count = Mathf.Min(tier.targetCount, targetCache.Count);
        for (int i = 0; i < count; i++)
        {
            StartCoroutine(DashRoutine(targetCache[i].transform, tier, i == 0));
        }
    }

    // 残りのスキル系関数は元のまま
    void SpawnPoison(PoisonTier tier) { if (poisonPrefab != null) { Vector2 randomOffset = Random.insideUnitCircle * 4.0f; Vector2 spawnPos = (Vector2)transform.position + randomOffset; GameObject fx = Instantiate(poisonPrefab, spawnPos, Quaternion.identity); fx.transform.localScale = fx.transform.localScale * tier.effectScale; PoisonEffect script = fx.GetComponent<PoisonEffect>(); if (script != null) script.Initialize(tier.duration, tier.radius, intelligence * tier.damageTickMult); } }
    void SpawnHeartBreak(HeartBreakTier tier) { if (nearestEnemy == null || heartBreakPrefab == null) return; GameObject fx = Instantiate(heartBreakPrefab, nearestEnemy.position, Quaternion.identity); fx.transform.localScale = Vector3.one * tier.effectScale; HeartBreakEffect script = fx.GetComponent<HeartBreakEffect>(); if (script != null) script.Initialize(tier.delay, tier.radius, attackPower * tier.damageMultiplier); }
    IEnumerator SpawnBreath(BreathTier tier) { isChanneling = true; if (breathPrefab != null) { GameObject fx = Instantiate(breathPrefab, transform.position, Quaternion.identity); fx.transform.localScale = fx.transform.localScale * tier.effectScale; BreathEffect script = fx.GetComponent<BreathEffect>(); if (script != null) script.Initialize(tier.duration, tier.range, intelligence * tier.damageTickMult, transform); } yield return new WaitForSeconds(tier.duration); isChanneling = false; }
    void SpawnExplodingMob(ExplodingMobTier tier) { if (explodingMobPrefab == null) return; int count = Random.Range(tier.minCount, tier.maxCount + 1); for (int i = 0; i < count; i++) { GameObject fx = Instantiate(explodingMobPrefab, transform.position, Quaternion.identity); fx.transform.localScale = fx.transform.localScale * tier.effectScale; ExplodingMob script = fx.GetComponent<ExplodingMob>(); if (script != null) script.Initialize(tier.lifeTime, tier.explosionRadius, attackPower * tier.damageMultiplier, mobExplosionPrefab, transform.position); } }
    void PerformNormalAttack() { if (slashPrefab != null && nearestEnemy != null) { if (Vector2.Distance(transform.position, nearestEnemy.position) <= 5.0f) { GameObject slash = Instantiate(slashPrefab, nearestEnemy.position, Quaternion.identity); MeleeAttackEffect effectScript = slash.GetComponent<MeleeAttackEffect>(); if (effectScript != null) effectScript.damage = attackPower; } } }

    IEnumerator DashRoutine(Transform target, TackleTier tier, bool isMainBody) { if (target == null) yield break; if (isMainBody) { isTackling = true; rb.linearVelocity = Vector2.zero; } Vector2 startPos = transform.position; Vector2 direction = ((Vector2)target.position - (Vector2)startPos).normalized; float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; ParticleSystem effect = null; if (tackleEffectPrefab != null) { effect = Instantiate(tackleEffectPrefab, startPos, Quaternion.Euler(0, 0, angle)); effect.transform.localScale = Vector3.one * tier.effectScale; } float distance = Vector2.Distance(startPos, target.position); float travelTime = distance / tier.dashSpeed; float elapsedTime = 0f; List<Enemy> hitEnemiesList = new List<Enemy>(); while (elapsedTime < travelTime) { if (GameManager.instance != null && GameManager.instance.isGameOver) yield break; elapsedTime += Time.deltaTime; float t = elapsedTime / travelTime; Vector2 currentPos = Vector2.Lerp(startPos, target.position, t); if (isMainBody) transform.position = currentPos; if (effect != null) effect.transform.position = currentPos; Collider2D[] hits = Physics2D.OverlapCircleAll(currentPos, tier.hitboxWidth / 2f); foreach (Collider2D hit in hits) { Enemy enemy = hit.GetComponent<Enemy>(); if (enemy != null && !hitEnemiesList.Contains(enemy)) { enemy.TakeDamage(attackPower); hitEnemiesList.Add(enemy); } } yield return null; } if (isMainBody) isTackling = false; if (effect != null) Destroy(effect.gameObject, 0.5f); }
}