using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneUIInitializer : MonoBehaviour
{
    [Header("GameManagerに登録するUIオブジェクトたち")]
    public TextMeshProUGUI levelText;
    public Slider hpBar;
    public Slider expBar;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI expText;
    public GameObject statusPanel;
    public TextMeshProUGUI statsText;
    public Image hamsterImageUI;
    public TextMeshProUGUI descriptionText;
    public Animator statusAnimator;
    public GameObject menuPanel;
    public Animator menuAnimator;

    [Header("★追加：ハムスターの名前を表示するテキスト")]
    public TextMeshProUGUI nameText;

    void Start()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SetupUIReferences(
                levelText, hpBar, expBar, hpText, expText,
                statusPanel, statsText, hamsterImageUI, descriptionText, statusAnimator,
                menuPanel, menuAnimator
            );

            // 追加：名前テキストを登録してUIを更新
            GameManager.instance.nameTextUI = this.nameText;
            GameManager.instance.UpdateUI();
        }
    }
}