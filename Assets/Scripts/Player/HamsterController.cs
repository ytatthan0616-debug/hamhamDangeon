using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class HamsterController : MonoBehaviour
{
    [Header("移動・時間設定")]
    public float tapMoveTime = 0.5f;
    public float wanderSpeed = 1.0f;
    public float wanderInterval = 2.0f;
    public float eatDuration = 1.5f;

    [Header("ランダム移動の範囲")]
    public Vector2 wanderAreaMin = new Vector2(-2.5f, -4.5f);
    public Vector2 wanderAreaMax = new Vector2(2.5f, -3.0f);

    [Header("アニメーション設定")]
    public float bounceHeight = 0.3f;
    public float fastBounceSpeed = 20f;
    public float slowBounceSpeed = 8f;

    [Header("エフェクト設定")]
    public ParticleSystem eatingEffectPrefab;
    private ParticleSystem currentEatingEffect;
    private Vector2 initialFoodPosition;
    public float shakeAmount = 0.05f;

    private GameManager gameManager;

    private Transform visualTransform;
    private SpriteRenderer visualRenderer;

    private Vector2 logicalPosition;
    private Vector2 targetPosition;
    private Transform targetFood = null;
    private GameManager.FoodType targetFoodType;

    private float currentMoveSpeed = 0f;
    private float wanderTimer = 0f;
    private float bounceTimer = 0f;
    private float currentBounceSpeed = 0f;

    private float eatTimer = 0f;

    private enum State { Idle, Wandering, MovingToTap, MovingToFood, Eating }
    private State currentState = State.Idle;

    void Awake()
    {
        SpriteRenderer originalRenderer = GetComponent<SpriteRenderer>();
        if (originalRenderer != null)
        {
            GameObject visualObj = new GameObject("HamsterVisual");
            visualObj.transform.SetParent(this.transform);
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = Vector3.one;
            visualObj.transform.localRotation = Quaternion.identity;

            visualRenderer = visualObj.AddComponent<SpriteRenderer>();
            visualRenderer.sprite = originalRenderer.sprite;
            visualRenderer.color = originalRenderer.color;
            visualRenderer.sortingLayerID = originalRenderer.sortingLayerID;
            visualRenderer.sortingOrder = originalRenderer.sortingOrder;
            visualRenderer.material = originalRenderer.material;

            originalRenderer.enabled = false;
            visualTransform = visualObj.transform;
        }
    }

    void Start()
    {
        gameManager = GameManager.instance;

        logicalPosition = transform.position;
        currentBounceSpeed = slowBounceSpeed;

        if (gameManager != null && visualRenderer != null)
        {
            gameManager.RegisterHamster(visualRenderer);
        }
    }

    void Update()
    {
        HandleInput();
        UpdateState();
        UpdateAnimation();
    }

    void HandleInput()
    {
        if (Time.timeSinceLevelLoad < 0.5f) return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            bool isPointerOverUI = false;
            if (EventSystem.current != null)
            {
                if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
                {
                    isPointerOverUI = EventSystem.current.IsPointerOverGameObject(Touchscreen.current.touches[0].touchId.ReadValue());
                }
                else
                {
                    isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
                }
            }

            if (isPointerOverUI) return;
            if (Camera.main == null) return;

            Vector2 pointerPos = Pointer.current.position.ReadValue();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(pointerPos);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            bool isEating = (currentState == State.Eating);

            if (hit.collider != null && hit.collider.CompareTag("Food"))
            {
                FoodBehavior food = hit.collider.GetComponent<FoodBehavior>();
                if (food != null && food.transform == targetFood) return;

                if (food != null)
                {
                    if (isEating) FinishEating();

                    // ==========================================
                    // ★修正：タップした瞬間に即座に経験値を獲得する！
                    // ==========================================
                    if (gameManager != null)
                    {
                        gameManager.AddFoodCount(food.foodType);
                    }

                    // ★連打で経験値を何度も取れないように、このエサを判定から外す
                    hit.collider.tag = "Untagged";

                    targetFood = food.transform;
                    targetFoodType = food.foodType;
                    targetPosition = targetFood.position;

                    float distance = Vector2.Distance(logicalPosition, targetPosition);
                    currentMoveSpeed = distance / tapMoveTime;

                    currentState = State.MovingToFood;
                    currentBounceSpeed = fastBounceSpeed;
                }
            }
            else
            {
                if (isEating) FinishEating();

                targetPosition = mousePos;
                float distance = Vector2.Distance(logicalPosition, targetPosition);
                currentMoveSpeed = distance / tapMoveTime;

                targetFood = null;
                currentState = State.MovingToTap;
                currentBounceSpeed = fastBounceSpeed;
            }
        }
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case State.Idle:
                wanderTimer += Time.deltaTime;
                if (wanderTimer >= wanderInterval)
                {
                    float rx = Random.Range(wanderAreaMin.x, wanderAreaMax.x);
                    float ry = Random.Range(wanderAreaMin.y, wanderAreaMax.y);
                    targetPosition = new Vector2(rx, ry);

                    currentMoveSpeed = wanderSpeed;
                    currentState = State.Wandering;
                    currentBounceSpeed = slowBounceSpeed;
                    wanderTimer = 0f;
                }
                break;

            case State.Wandering:
            case State.MovingToTap:
                logicalPosition = Vector2.MoveTowards(logicalPosition, targetPosition, currentMoveSpeed * Time.deltaTime);

                if (Vector2.Distance(logicalPosition, targetPosition) < 0.01f)
                {
                    currentState = State.Idle;
                    currentBounceSpeed = slowBounceSpeed;
                }
                break;

            case State.MovingToFood:
                if (targetFood == null)
                {
                    currentState = State.Idle;
                    break;
                }

                targetPosition = targetFood.position;
                logicalPosition = Vector2.MoveTowards(logicalPosition, targetPosition, currentMoveSpeed * Time.deltaTime);

                if (Vector2.Distance(logicalPosition, targetPosition) < 0.1f)
                {
                    StartEating();
                }
                break;

            case State.Eating:
                if (targetFood != null)
                {
                    eatTimer += Time.deltaTime;

                    float rx = Random.Range(-shakeAmount, shakeAmount);
                    float ry = Random.Range(-shakeAmount, shakeAmount);
                    targetFood.position = initialFoodPosition + new Vector2(rx, ry);

                    if (eatTimer >= eatDuration) FinishEating();
                }
                else
                {
                    currentState = State.Idle;
                    currentBounceSpeed = slowBounceSpeed;
                }
                break;
        }
    }

    void UpdateAnimation()
    {
        bounceTimer += Time.deltaTime * currentBounceSpeed;

        float bounceY = Mathf.Abs(Mathf.Sin(bounceTimer)) * bounceHeight;

        if (visualTransform != null)
        {
            visualTransform.localPosition = new Vector3(0f, bounceY, 0f);
        }

        transform.position = new Vector3(logicalPosition.x, logicalPosition.y, transform.position.z);

        if (currentState == State.Wandering || currentState == State.MovingToTap || currentState == State.MovingToFood)
        {
            float diffX = targetPosition.x - logicalPosition.x;
            if (diffX > 0.05f)
            {
                if (visualRenderer != null) visualRenderer.flipX = true;
            }
            else if (diffX < -0.05f)
            {
                if (visualRenderer != null) visualRenderer.flipX = false;
            }
        }
    }

    void StartEating()
    {
        currentState = State.Eating;
        eatTimer = 0f;
        currentBounceSpeed = fastBounceSpeed * 0.8f;

        if (targetFood != null) initialFoodPosition = targetFood.position;

        if (eatingEffectPrefab != null && targetFood != null)
        {
            currentEatingEffect = Instantiate(eatingEffectPrefab, targetFood.position, Quaternion.identity);

            FoodBehavior food = targetFood.GetComponent<FoodBehavior>();
            if (food != null)
            {
                var mainModule = currentEatingEffect.main;
                mainModule.startColor = food.effectColor;
            }
            currentEatingEffect.Play();
        }
    }

    void FinishEating()
    {
        if (targetFood != null) Destroy(targetFood.gameObject);
        targetFood = null;

        if (currentEatingEffect != null)
        {
            currentEatingEffect.Stop();
            Destroy(currentEatingEffect.gameObject, 1.0f);
        }

        // ★修正：経験値の獲得処理は「タップした瞬間」に移動したので、ここは消去しました

        currentState = State.Idle;
        currentBounceSpeed = slowBounceSpeed;
        wanderTimer = 0f;
    }
}