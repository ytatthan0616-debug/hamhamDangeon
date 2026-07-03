using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DungeonUIBridge : MonoBehaviour
{
    [Header("ダンジョンのUIをここにセットしてください")]
    public TextMeshProUGUI dungeonHpText;
    public Slider hpBarSlider;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public GameObject stageTitlePanel;
    public GameObject clearPanel;
    public TextMeshProUGUI stageTitleText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI scoreText;
    public GameObject purchaseSpeedUpButton;
    public GameObject toggleSpeedUpButton;
    public TextMeshProUGUI toggleSpeedUpText;

    // ★追加：ここにコンティニューボタンを登録します！
    public GameObject continueButton;

    void Start()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.dungeonHpText = this.dungeonHpText;
            GameManager.instance.hpBar = this.hpBarSlider;
            GameManager.instance.pauseMenuPanel = this.pauseMenuPanel;
            GameManager.instance.gameOverPanel = this.gameOverPanel;
            GameManager.instance.clearPanel = this.clearPanel;

            GameManager.instance.waveText = this.waveText;
            GameManager.instance.scoreText = this.scoreText;

            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (clearPanel != null) clearPanel.SetActive(false);

            GameManager.instance.UpdateDungeonHpUI();
            GameManager.instance.UpdateWaveAndScoreUI();
        }

        if (stageTitlePanel != null)
        {
            StartCoroutine(ShowTitleRoutine());
        }
    }

    private IEnumerator ShowTitleRoutine()
    {
        stageTitlePanel.SetActive(true);
        if (stageTitleText != null)
        {
            stageTitleText.text = "えいきゅうダンジョン WAVE " + GameManager.instance.currentWave;
        }

        stageTitlePanel.SetActive(true);

        CanvasGroup canvasGroup = stageTitlePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = stageTitlePanel.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(2.0f);

        float fadeDuration = 1.0f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        stageTitlePanel.SetActive(false);
    }

    public void ClickTogglePause()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.TogglePause();
            if (GameManager.instance.pauseMenuPanel.activeSelf)
            {
                UpdatePauseMenuUI();
            }
        }
    }

    public void ClickReturnToIkusei()
    {
        if (GameManager.instance != null) GameManager.instance.ReturnToIkusei();
    }

    public void ClickContinueWithAd()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ContinueWithAd();

            EnemySpawner spawner = Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                // 敵の再スポーン等が必要な場合はここに追記
            }
        }
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);

        // ★修正：コンティニュー済みならボタンを非表示にする
        if (continueButton != null && GameManager.instance != null)
        {
            continueButton.SetActive(!GameManager.instance.hasContinued);
        }

        StartCoroutine(FlashScore(scoreText));

        GameObject hamster = GameObject.FindGameObjectWithTag("Player");
        if (hamster != null) StartCoroutine(FadeOut(hamster));
    }

    IEnumerator FadeOut(GameObject target)
    {
        Collider2D col = target.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>();
        float duration = 1.0f;

        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            foreach (var sr in srs)
            {
                if (sr != null && sr.sprite != null)
                {
                    Color c = sr.color;
                    sr.color = new Color(c.r, c.g, c.b, 1f - (t / duration));
                }
            }
            yield return null;
        }

        foreach (var sr in srs)
        {
            if (sr != null && sr.sprite != null) sr.enabled = false;
        }
    }

    IEnumerator FlashScore(TextMeshProUGUI text)
    {
        while (gameOverPanel.activeSelf)
        {
            text.alpha = 0.5f;
            yield return new WaitForSeconds(0.2f);
            text.alpha = 1.0f;
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void UpdatePauseMenuUI()
    {
        if (GameManager.instance == null) return;

        bool purchased = GameManager.instance.isSpeedUpPurchased;

        if (purchaseSpeedUpButton != null) purchaseSpeedUpButton.SetActive(!purchased);
        if (toggleSpeedUpButton != null) toggleSpeedUpButton.SetActive(purchased);

        if (toggleSpeedUpText != null)
        {
            toggleSpeedUpText.text = GameManager.instance.isSpeedUpActive ? "2倍速：ON" : "2倍速：OFF";
        }
    }

    public void ClickPurchaseSpeedUp()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.PurchaseSpeedUp();
            UpdatePauseMenuUI();
        }
    }

    public void ClickToggleSpeedUp()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ToggleSpeedUp();
            UpdatePauseMenuUI();
        }
    }
}