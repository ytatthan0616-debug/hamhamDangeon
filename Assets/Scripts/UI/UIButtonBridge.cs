using UnityEngine;

public class UIButtonBridge : MonoBehaviour
{
    [Header("ウィンドウUI設定")]
    public GameObject statusWindow;
    public GameObject menuWindow;
    public GameObject hpWarningPanel;
    public GameObject SelectStartPanel;

    void Start()
    {
        if (hpWarningPanel != null) hpWarningPanel.SetActive(false);
        if (statusWindow != null) statusWindow.SetActive(false);
        if (menuWindow != null) menuWindow.SetActive(false);
        if (SelectStartPanel != null) SelectStartPanel.SetActive(false);
    }

    public void ClickOpenStatus() { if (statusWindow != null) statusWindow.SetActive(true); }
    public void ClickCloseStatus() { if (statusWindow != null) statusWindow.SetActive(false); }
    public void ClickOpenMenu() { if (menuWindow != null) menuWindow.SetActive(true); }
    public void ClickCloseMenu() { if (menuWindow != null) menuWindow.SetActive(false); }

    // UIButtonBridge.cs
    public void ClickGoToDungeon()
    {
        if (GameManager.instance == null) return;

        int savedWave = PlayerPrefs.GetInt("SavedWave", 1);
        Debug.Log("ダンジョンボタン押下: SavedWave=" + savedWave);

        // HPチェック（必要なら残す）
        if (GameManager.instance.currentHP < GameManager.instance.maxHP * 0.3f)
        {
            if (hpWarningPanel != null) hpWarningPanel.SetActive(true);
        }
        else if (savedWave > 1)
        {
            // ここでパネルを表示し、あとはパネル上の「続きから」「最初から」ボタンに任せる
            if (SelectStartPanel != null)
            {
                SelectStartPanel.SetActive(true);
                Debug.Log("パネルを表示しました。パネル上のボタンでダンジョンへ移動してください。");
            }
        }
        else
        {
            GameManager.instance.GoToDungeon();
        }
    }

    public void ClickCloseSelectPanel()
    {
        if (SelectStartPanel != null) SelectStartPanel.SetActive(false);
    }
    public void ClickStartFromBeginning()
    {
        PlayerPrefs.SetInt("SavedWave", 1); // 1にリセット
        PlayerPrefs.Save();
        GameManager.instance.currentWave = 1; // マネージャーのウェーブも戻す
        GameManager.instance.GoToDungeon();
    }

    // プレイヤーが「続きから」を選んだとき
    public void ClickContinueFromSaved()
    {
        GameManager.instance.currentWave = PlayerPrefs.GetInt("SavedWave", 1);
        GameManager.instance.GoToDungeon();
    }

    public void ClickCloseHpWarning()
    {
        if (hpWarningPanel != null) hpWarningPanel.SetActive(false);
    }

    // ==========================================
    // ★追加：図鑑の切り替え命令をGameManagerに伝える中継機能
    // ==========================================
    public void ClickPokedexNext()
    {
        if (GameManager.instance != null) GameManager.instance.PokedexNext();
    }

    public void ClickPokedexPrev()
    {
        if (GameManager.instance != null) GameManager.instance.PokedexPrev();
    }

    public void ClickRecoverFullHPWithAd()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.RecoverFullHPWithAd();

            // 育成画面で回復したあと、念のためメニューを閉じるなら以下の1行を追加してもOKです
            if (menuWindow != null) menuWindow.SetActive(false);
        }
    }

    public void ClickContinueWithAd()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ContinueWithAd();
        }
    }
}