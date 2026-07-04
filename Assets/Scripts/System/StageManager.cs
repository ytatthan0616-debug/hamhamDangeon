using UnityEngine;
using TMPro;

public class StageManager : MonoBehaviour
{
    [Header("ここに作成したステージのプレハブを順番に登録")]
    public GameObject[] stagePrefabs;

    [Header("各ステージの名前（UI表示用）")]
    public string[] stageNames;

    [Header("ハムスターの初期スタート位置")]
    public Transform playerSpawnPoint;

    void Start()
    {
        if (GameManager.instance == null || stagePrefabs.Length == 0) return;

        int stageIndex = GameManager.instance.currentStageIndex % stagePrefabs.Length;

        // 生成したステージを変数「stageObj」として記憶する
        GameObject stageObj = Instantiate(stagePrefabs[stageIndex], Vector3.zero, Quaternion.identity);

        // ==========================================
        // ★修正：100WAVEまで色を用意し、110以降はランダム
        // ==========================================
        int colorPhase = GameManager.instance.currentWave / 10;

        Color[] bgColors = new Color[] {
            Color.white,                     // 0-9WAVE: 通常
            new Color(0.75f, 1f, 0.75f),     // 10-19WAVE: 緑
            new Color(0.75f, 1f, 1f),        // 20-29WAVE: 水色
            new Color(0.75f, 0.75f, 1f),     // 30-39WAVE: 青
            new Color(0.85f, 0.6f, 1f),      // 40-49WAVE: 紫
            new Color(1f, 0.75f, 0.9f),      // 50-59WAVE: ピンク
            new Color(1f, 0.75f, 0.75f),     // 60-69WAVE: 赤
            new Color(1f, 0.8f, 0.7f),       // 70-79WAVE: 朱色
            new Color(1f, 0.85f, 0.6f),      // 80-89WAVE: オレンジ
            new Color(1f, 0.95f, 0.6f),      // 90-99WAVE: ゴールド
            Color.white                      // 100-109WAVE: 最終決戦（白）
        };

        Color tint = Color.white;

        if (colorPhase <= 10)
        {
            tint = bgColors[colorPhase];
        }
        else
        {
            // 110WAVE以降：暗くなりすぎないよう0.6f〜1.0fの間でランダムな色を生成
            // シード値をWAVE数（colorPhase）で固定し、再起動しても同じWAVEなら同じ色になるようにする
            Random.InitState(colorPhase);
            tint = new Color(Random.Range(0.6f, 1f), Random.Range(0.6f, 1f), Random.Range(0.6f, 1f));
            // ゲーム内の他のランダム要素に影響が出ないようシード値をリセット
            Random.InitState(System.Environment.TickCount);
        }

        // ステージ内のすべての画像の色を、元の透明度を保ちながら掛け算（Tint）で染める
        SpriteRenderer[] srs = stageObj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            Color original = sr.color;
            // r, g, bだけを掛け算し、透明度(a)は元のまま維持する！
            sr.color = new Color(original.r * tint.r, original.g * tint.g, original.b * tint.b, original.a);
        }

        // UIへのタイトル反映
        DungeonUIBridge uiBridge = Object.FindAnyObjectByType<DungeonUIBridge>();
        if (uiBridge != null && stageNames != null && stageIndex < stageNames.Length)
        {
            if (uiBridge.stageTitlePanel != null)
            {
                TextMeshProUGUI titleText = uiBridge.stageTitlePanel.GetComponentInChildren<TextMeshProUGUI>();
                if (titleText != null) titleText.text = stageNames[stageIndex];
            }
        }

        // ハムスターの位置をスタート地点に移動
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.position;
        }
    }
}