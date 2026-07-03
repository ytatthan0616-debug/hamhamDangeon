using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AllClearMenuUI : MonoBehaviour
{
    [Header("UIパネル・ボタンの設定")]
    public GameObject explanationPanel;       // 全クリ特典を説明するポップアップパネル
    public Button closePanelButton;           // 説明パネルを閉じるボタン

    void Start()
    {
        // 繰り返しプレイ（周回）機能は廃止したため、リピートボタン関連の処理をすべて削除しました．
        // 100ウェーブクリアの勝利パネルはダンジョン内で表示されるため、こちらの自動ポップアップも不要になります．

        // ボタンのクリックイベントを登録
        if (closePanelButton != null)
        {
            closePanelButton.onClick.AddListener(ClosePanel);
        }
    }

    void ClosePanel()
    {
        if (explanationPanel != null)
        {
            explanationPanel.SetActive(false);
        }
    }
}