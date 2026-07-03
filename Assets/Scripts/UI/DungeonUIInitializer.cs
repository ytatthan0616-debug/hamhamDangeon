using UnityEngine;
using TMPro;

public class DungeonUIInitializer : MonoBehaviour
{
    [Header("ダンジョンUIの参照")]
    public TextMeshProUGUI dungeonHpText;
    public GameObject pauseMenuPanel;
    public TextMeshProUGUI waveText;   // ★追加：ウェーブ数表示用
    public TextMeshProUGUI scoreText;  // ★追加：スコア表示用

    void Start()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SetupDungeonUIReferences(dungeonHpText, pauseMenuPanel, waveText, scoreText);
        }
    }
}