using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // ==========================================
            // ★追加：シーン移動による「UI参照はがれ」を直す処理
            // ==========================================
            // 新しいシーンで読み込まれた図鑑アイコンのリストを、生き残っているGameManagerに渡す
            if (this.pokedexIcons != null && this.pokedexIcons.Length > 0)
            {
                instance.pokedexIcons = this.pokedexIcons;
            }

            // もしレベルアップエフェクトなどもインスペクターで設定していれば引き継ぐ
            if (this.uiLevelUpEffect != null)
            {
                instance.uiLevelUpEffect = this.uiLevelUpEffect;
            }

            // 引き継ぎが終わったら、即座に図鑑の画像を最新の状態に更新する
            instance.UpdatePokedexUI();

            // 引き継ぎ終わったダミーのGameManagerは消滅させる
            Destroy(gameObject);
        }
    }

    [Header("UI設定")]
    public TextMeshProUGUI levelText;
    public Slider hpBar;
    public Slider expBar;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI statsText;
    public Image hamsterImageUI;
    public TextMeshProUGUI descriptionText;

    [Header("名前設定")]
    public string customHamsterName = "";
    [HideInInspector] public TextMeshProUGUI nameTextUI;

    [System.Serializable]
    public class HamsterForm
    {
        public string formName = "名前未設定";
        [TextArea(2, 4)] public string description = "説明文をここに書きます．";
        public Sprite sprite;
    }

    [Header("図鑑・形態設定")]
    public HamsterForm normalForm;
    public HamsterForm[] smartForms = new HamsterForm[5];
    public HamsterForm[] toughForms = new HamsterForm[5];
    public HamsterForm[] fighterForms = new HamsterForm[5];

    [Header("図鑑（Pokedex）システム設定")]
    public int currentPokedexIndex = 0;
    public bool[] unlockedForms = new bool[16];
    public Image[] pokedexIcons = new Image[16];
    public Color lockedColor = new Color(0f, 0f, 0f, 0.7f);

    [Header("ダンジョンUI設定")]
    public TextMeshProUGUI dungeonHpText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI scoreText;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public GameObject clearPanel;
    public bool isGameOver = false;
    private bool isClear = false;

    // ★追加：コンティニューしたかを記憶するフラグ
    [HideInInspector] public bool hasContinued = false;

    [Header("システム設定")]
    public bool showDamagePopup = true;
    public bool showEnemyShadows = true;

    [Header("育成・戦闘パラメータ")]
    public int currentLevel = 1;
    public int totalFoodEaten = 0;
    public int currentWave = 1;
    public int currentScore = 0;
    public int currentStageIndex = 0;
    public int currentExp = 0;
    public int currentMaxExp = 30;
    public int seedBaseExp = 3;
    public int vegetableBaseExp = 4;
    public int meatBaseExp = 4;
    public int expBonusPerStage = 1;

    [Header("ステータス")]
    public float maxHP = 10f;
    public float currentHP = 10f;
    public float attackPower = 10f;
    public float intelligence = 10f;

    [HideInInspector] public int seedCount = 0;
    [HideInInspector] public int vegetableCount = 0;
    [HideInInspector] public int meatCount = 0;

    public enum FoodType { Seed, Vegetable, Meat }
    public enum EvolvedType { None, Smart, Tough, Fighter }

    [HideInInspector] public EvolvedType currentEvolution = EvolvedType.None;
    [HideInInspector] public int evolutionTier = 0;

    [System.Serializable]
    public class StatGrowth
    {
        public float baseHp, hpMult, baseAtk, atkMult, baseInt, intMult;
    }

    [Header("タイプ別 成長率設定")]
    public StatGrowth normalGrowth = new StatGrowth { baseHp = 5f, hpMult = 1.2f, baseAtk = 2f, atkMult = 0.5f, baseInt = 2f, intMult = 0.5f };
    public StatGrowth smartGrowth = new StatGrowth { baseHp = 4f, hpMult = 1.0f, baseAtk = 1.5f, atkMult = 0.4f, baseInt = 5f, intMult = 2.0f };
    public StatGrowth toughGrowth = new StatGrowth { baseHp = 8f, hpMult = 2.5f, baseAtk = 2f, atkMult = 0.5f, baseInt = 1f, intMult = 0.2f };
    public StatGrowth fighterGrowth = new StatGrowth { baseHp = 5f, hpMult = 1.5f, baseAtk = 4f, atkMult = 2.0f, baseInt = 1f, intMult = 0.2f };

    [Header("進化レベル設定")]
    public int[] evolutionLevels = new int[] { 10, 20, 30, 40, 50 };

    [Header("進化設定（それぞれの大きさ倍率）")]
    public Vector3 normalScale = Vector3.one;
    public Vector3 evolvedScale = new Vector3(1.2f, 1.2f, 1f);

    [HideInInspector] public Sprite savedHamsterSprite;
    [HideInInspector] public Vector3 savedHamsterScale = Vector3.one;
    private SpriteRenderer hamsterSprite;

    [Header("広告バフ・エフェクト設定")]
    public bool isAdBuffActive = false;
    public float adBuffDuration = 180f;
    public float adBuffTimer = 0f;
    public ParticleSystem hamsterLevelUpEffectPrefab;
    public ParticleSystem uiLevelUpEffect;
    private bool isPaused = false;
    public ParticleSystem evolutionEffectPrefab;

    [Header("課金・倍速設定")]
    public bool isSpeedUpPurchased = false;
    public bool isSpeedUpActive = false;

    [Header("復活エフェクト")]
    public GameObject reviveEffectPrefab;

    public bool isFirstAllClear = false;
    public int totalAllClearCount = 0;

    public float FinalMaxHP => maxHP * (1.0f + 0.01f * totalAllClearCount);
    public float FinalAttackPower => attackPower * (1.0f + 0.01f * totalAllClearCount);
    public float FinalIntelligence => intelligence * (1.0f + 0.01f * totalAllClearCount);

    public void RegisterHamster(SpriteRenderer renderer)
    {
        hamsterSprite = renderer;
        if (savedHamsterSprite != null)
        {
            hamsterSprite.sprite = savedHamsterSprite;
            hamsterSprite.transform.localScale = savedHamsterScale;
        }
        else { ApplyCurrentHamsterLook(); }
    }

    public void SetupUIReferences(TextMeshProUGUI lvl, Slider hp, Slider exp, TextMeshProUGUI hpt, TextMeshProUGUI expt, GameObject statP, TextMeshProUGUI statT, Image imgUI, TextMeshProUGUI descT, Animator statAnim, GameObject menP, Animator menAnim)
    {
        levelText = lvl; hpBar = hp; expBar = exp; hpText = hpt; expText = expt;
        statsText = statT; hamsterImageUI = imgUI; descriptionText = descT;
        UpdateUI();
    }

    public void SetupDungeonUIReferences(TextMeshProUGUI dHpText, GameObject pMenuPanel, TextMeshProUGUI wText, TextMeshProUGUI sText)
    {
        dungeonHpText = dHpText; pauseMenuPanel = pMenuPanel; waveText = wText; scoreText = sText;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        UpdateDungeonHpUI(); UpdateWaveAndScoreUI();
    }

    void Start()
    {
        customHamsterName = PlayerPrefs.GetString("CustomHamsterName", "");

        LoadPokedex(); UnlockForm(0);
        currentPokedexIndex = GetCurrentFormIndex();
        UpdateUI();

        currentWave = PlayerPrefs.GetInt("SavedWave", 1);
        UpdateUI();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "DungeonScene" && currentHP < maxHP)
        {
            currentHP += maxHP * 0.01f * Time.deltaTime;
            if (currentHP > maxHP) currentHP = maxHP;
            UpdateHPBar();
        }
        UpdateDungeonHpUI();
        if (currentHP <= 0 && !isGameOver && !isClear && SceneManager.GetActiveScene().name == "DungeonScene")
        {
            isGameOver = true; currentHP = 0; UpdateDungeonHpUI(); ShowGameOver();
        }
        if (isAdBuffActive)
        {
            adBuffTimer -= Time.deltaTime;
            if (adBuffTimer <= 0f) { isAdBuffActive = false; adBuffTimer = 0f; }
        }
    }

    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(customHamsterName)) return customHamsterName;

        HamsterForm currentForm = GetFormFromIndex(GetCurrentFormIndex());
        return currentForm != null ? currentForm.formName : "名前未設定";
    }

    public void ChangeHamsterName(string newName)
    {
        customHamsterName = newName.Trim();
        PlayerPrefs.SetString("CustomHamsterName", customHamsterName);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void AddScore(int amount) { currentScore += amount; UpdateWaveAndScoreUI(); }

    public void NextWave()
    {
        if (currentWave == 100 && !isFirstAllClear) { CompleteFinalStage(); return; }

        currentWave++;

        if (TickerMessageUI.instance != null) TickerMessageUI.instance.ShowWaveMessage(currentWave);

        if ((currentWave - 1) % 5 == 0)
        {
            PlayerPrefs.SetInt("SavedWave", currentWave);
            PlayerPrefs.Save();
        }

        if (currentWave % 5 == 0) currentStageIndex++;
        if (currentWave % 10 == 0) GiveWaveClearBonus();
        UpdateWaveAndScoreUI();
    }

    void GiveWaveClearBonus()
    {
        int bonusExp = 50 + (currentStageIndex * 20);
        currentExp += bonusExp;
        if (currentExp >= currentMaxExp && currentLevel < 100) LevelUp();
        UpdateUI();
    }

    public void AddFoodCount(FoodType type)
    {
        totalFoodEaten++;
        int baseExp = 0;
        if (type == FoodType.Seed) { seedCount++; baseExp = seedBaseExp; }
        else if (type == FoodType.Vegetable) { vegetableCount++; baseExp = vegetableBaseExp; }
        else if (type == FoodType.Meat) { meatCount++; baseExp = meatBaseExp; }

        if (currentLevel >= 100) { UpdateUI(); return; }

        int expToAdd = Mathf.CeilToInt(baseExp + (currentStageIndex * expBonusPerStage));
        if (isFirstAllClear) expToAdd *= 2;
        if (isAdBuffActive) expToAdd *= 2;

        currentExp += expToAdd;
        if (currentExp >= currentMaxExp && currentLevel < 100) LevelUp();
        CheckEvolution(); UpdateUI();
    }

    void LevelUp()
    {
        currentLevel++;
        StatGrowth cg = normalGrowth;
        if (currentEvolution == EvolvedType.Smart) cg = smartGrowth;
        else if (currentEvolution == EvolvedType.Tough) cg = toughGrowth;
        else if (currentEvolution == EvolvedType.Fighter) cg = fighterGrowth;

        maxHP += cg.baseHp + (currentLevel * cg.hpMult);
        attackPower += cg.baseAtk + (currentLevel * cg.atkMult);
        intelligence += cg.baseInt + (currentLevel * cg.intMult);
        currentHP = maxHP;

        currentExp -= currentMaxExp;
        currentMaxExp = Mathf.FloorToInt(currentMaxExp * 1.15f);
        if (currentLevel >= 100) { currentLevel = 100; currentExp = 0; }

        if (hamsterSprite != null && hamsterLevelUpEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(hamsterLevelUpEffectPrefab, hamsterSprite.transform.position, Quaternion.identity);
            Destroy(effect.gameObject, 2.0f);
        }
        if (uiLevelUpEffect != null) uiLevelUpEffect.Play();
    }

    public void UpdateUI()
    {
        if (currentLevel >= 100)
        {
            if (levelText != null) levelText.text = "Lv: MAX";
            if (expText != null) expText.text = "MAX / MAX";
            if (expBar != null) { expBar.maxValue = 1f; expBar.value = 1f; }
        }
        else
        {
            if (levelText != null) levelText.text = "Lv: " + currentLevel;
            if (expBar != null) { expBar.maxValue = currentMaxExp; expBar.value = currentExp; }
            if (expText != null) expText.text = currentExp + " / " + currentMaxExp;
        }

        if (statsText != null)
        {
            string lvStr = currentLevel >= 100 ? "MAX" : currentLevel.ToString();
            statsText.text = $"レベル: {lvStr}\nＨ Ｐ: {Mathf.FloorToInt(maxHP)}\n攻撃力: {Mathf.FloorToInt(attackPower)}\n賢 さ: {Mathf.FloorToInt(intelligence)}";
        }

        if (nameTextUI != null) nameTextUI.text = GetDisplayName();

        UpdateHPBar(); UpdatePokedexUI();
    }

    void UpdateHPBar()
    {
        if (hpBar != null) hpBar.value = currentHP / maxHP;
        if (hpText != null) hpText.text = Mathf.FloorToInt(currentHP) + " / " + Mathf.FloorToInt(maxHP);
    }

    public void UpdateDungeonHpUI()
    {
        if (dungeonHpText != null) dungeonHpText.text = "HP: " + Mathf.FloorToInt(currentHP) + " / " + Mathf.FloorToInt(maxHP);
        if (hpBar != null && SceneManager.GetActiveScene().name == "DungeonScene") hpBar.value = currentHP / maxHP;
    }

    public void UpdateWaveAndScoreUI()
    {
        if (waveText != null) waveText.text = "Wave: " + currentWave;
        if (scoreText != null)
        {
            int displayScore = Mathf.Min(currentScore, 99999999);
            scoreText.text = "Score " + displayScore.ToString("D8");
        }
    }

    void CheckEvolution()
    {
        bool evolvedThisTime = false;
        for (int i = 0; i < evolutionLevels.Length; i++)
        {
            if (currentLevel >= evolutionLevels[i] && evolutionTier == i)
            {
                evolutionTier = i + 1;
                evolvedThisTime = true;
                if (evolutionTier == 1)
                {
                    if (seedCount >= vegetableCount && seedCount >= meatCount) currentEvolution = EvolvedType.Smart;
                    else if (vegetableCount >= seedCount && vegetableCount >= meatCount) currentEvolution = EvolvedType.Tough;
                    else currentEvolution = EvolvedType.Fighter;
                }
            }
        }

        ApplyCurrentHamsterLook();

        if (evolvedThisTime)
        {
            UnlockForm(GetCurrentFormIndex());
            currentPokedexIndex = GetCurrentFormIndex();

            if (hamsterSprite != null && evolutionEffectPrefab != null)
            {
                ParticleSystem effect = Instantiate(evolutionEffectPrefab, hamsterSprite.transform.position, Quaternion.identity);
                Destroy(effect.gameObject, 3.0f);
            }
        }
    }

    public void ApplyCurrentHamsterLook()
    {
        if (hamsterSprite == null) return;
        hamsterSprite.color = Color.white;
        HamsterForm currentForm = GetFormFromIndex(GetCurrentFormIndex());

        if (currentForm != null && currentForm.sprite != null)
        {
            hamsterSprite.sprite = currentForm.sprite;
            hamsterSprite.transform.localScale = (evolutionTier == 0) ? normalScale : evolvedScale;
        }

        savedHamsterSprite = hamsterSprite.sprite;
        savedHamsterScale = hamsterSprite.transform.localScale;
    }

    public void PokedexNext() { currentPokedexIndex++; if (currentPokedexIndex > 15) currentPokedexIndex = 0; UpdatePokedexUI(); }
    public void PokedexPrev() { currentPokedexIndex--; if (currentPokedexIndex < 0) currentPokedexIndex = 15; UpdatePokedexUI(); }

    public void UpdatePokedexUI()
    {
        for (int i = 0; i < 16; i++)
        {
            if (i < pokedexIcons.Length && pokedexIcons[i] != null)
            {
                HamsterForm form = GetFormFromIndex(i);
                pokedexIcons[i].sprite = (form != null) ? form.sprite : null;
                pokedexIcons[i].color = unlockedForms[i] ? Color.white : lockedColor;
            }
        }

        if (hamsterImageUI != null && descriptionText != null)
        {
            HamsterForm targetForm = GetFormFromIndex(currentPokedexIndex);
            if (unlockedForms[currentPokedexIndex] && targetForm != null)
            {
                hamsterImageUI.sprite = targetForm.sprite;
                hamsterImageUI.color = Color.white;
                descriptionText.text = $"【{targetForm.formName}】\n{targetForm.description}";
            }
            else
            {
                hamsterImageUI.sprite = (targetForm != null) ? targetForm.sprite : null;
                hamsterImageUI.color = lockedColor;
                descriptionText.text = "？？？？\n（まだ発見されていない形態です）";
            }
        }
    }

    public int GetCurrentFormIndex()
    {
        if (evolutionTier == 0) return 0;
        int offset = (currentEvolution == EvolvedType.Smart) ? 1 : (currentEvolution == EvolvedType.Tough) ? 6 : 11;
        return offset + (evolutionTier - 1);
    }

    public HamsterForm GetFormFromIndex(int index)
    {
        if (index == 0) return normalForm;
        if (index >= 1 && index <= 5 && index - 1 < smartForms.Length) return smartForms[index - 1];
        if (index >= 6 && index <= 10 && index - 6 < toughForms.Length) return toughForms[index - 6];
        if (index >= 11 && index <= 15 && index - 11 < fighterForms.Length) return fighterForms[index - 11];
        return normalForm;
    }

    public void UnlockForm(int index) { unlockedForms[index] = true; PlayerPrefs.SetInt("Pokedex_" + index, 1); PlayerPrefs.Save(); }
    void LoadPokedex() { for (int i = 0; i < 16; i++) { unlockedForms[i] = PlayerPrefs.GetInt("Pokedex_" + i, 0) == 1; } unlockedForms[0] = true; }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = isSpeedUpActive ? 2f : 1f;
        }

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(isPaused);
    }

    public void ReturnToIkusei()
    {
        Time.timeScale = 1f;
        isSpeedUpActive = false;
        isPaused = false;
        isGameOver = false;
        hasContinued = false; // ★追加：育成に戻る時も確実にコンティニュー権をリセット
        if (isClear) isClear = false;
        if (currentHP <= 0) currentHP = 1;
        SceneManager.LoadScene("IKUSEIScene");
    }

    public void GoToDungeon()
    {
        if (hamsterSprite != null)
        {
            savedHamsterSprite = hamsterSprite.sprite;
            savedHamsterScale = hamsterSprite.transform.localScale;
        }

        // ★修正：ダンジョンに入った時だけスコアとコンティニュー権をリセットする！
        currentScore = 0;
        hasContinued = false;

        PlayerPrefs.SetInt("CurrentWaveAtStart", currentWave);
        PlayerPrefs.Save();

        SceneManager.LoadScene("DungeonScene");
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // ★修正：ここに残っていた currentScore = 0; を完全に削除しました！（スコアが消えなくなります）

        DungeonUIBridge uiBridge = Object.FindAnyObjectByType<DungeonUIBridge>();
        if (uiBridge != null) uiBridge.ShowGameOver();
    }

    public void CompleteFinalStage() { isClear = true; totalAllClearCount++; isFirstAllClear = true; if (clearPanel != null) clearPanel.SetActive(true); }
    public void ActivateAdBuff() { isAdBuffActive = true; adBuffTimer = adBuffDuration; }

    public void RecoverFullHPWithAd()
    {
        currentHP = maxHP;
        UpdateUI();
        UpdateDungeonHpUI();
    }

    public void ContinueWithAd()
    {
        // ★修正：絶対に1回しかコンティニューできないように強固にブロック！
        if (hasContinued) return;

        hasContinued = true; // コンティニューしたことを記録
        currentHP = maxHP;
        isGameOver = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        GameObject hamster = GameObject.FindGameObjectWithTag("Player");
        if (hamster != null)
        {
            StartCoroutine(ReviveEffectRoutine(hamster));
        }

        UpdateUI();
        UpdateDungeonHpUI();

        Time.timeScale = isSpeedUpActive ? 2f : 1f;
    }

    IEnumerator ReviveEffectRoutine(GameObject hamster)
    {
        SpriteRenderer[] srs = hamster.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            if (sr != null && sr.sprite != null) sr.enabled = false;
        }

        if (reviveEffectPrefab != null)
        {
            GameObject fx = Instantiate(reviveEffectPrefab, hamster.transform.position, Quaternion.identity);
            Destroy(fx, 2.0f);
        }

        yield return new WaitForSeconds(0.5f);

        foreach (var sr in srs)
        {
            if (sr != null && sr.sprite != null)
            {
                sr.enabled = true;
                sr.color = Color.white;
            }
        }

        Collider2D col = hamster.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    public void PurchaseSpeedUp()
    {
        isSpeedUpPurchased = true;
        PlayerPrefs.SetInt("IsSpeedUpPurchased", 1);
        PlayerPrefs.Save();
    }

    public void ToggleSpeedUp()
    {
        if (!isSpeedUpPurchased) return;

        isSpeedUpActive = !isSpeedUpActive;

        if (!isPaused)
        {
            Time.timeScale = isSpeedUpActive ? 2f : 1f;
        }
    }
}