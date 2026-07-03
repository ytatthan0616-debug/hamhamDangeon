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
        // ★修正：透明度（アルファ）を破壊しないように色を染める
        // ==========================================
        int colorPhase = (GameManager.instance.currentWave / 10) % 6;
        Color[] bgColors = new Color[] {
            Color.white,                     // 0-9WAVE: 通常
            new Color(1f, 0.75f, 0.75f),     // 10-19WAVE: 赤
            new Color(0.75f, 1f, 0.75f),     // 20-29WAVE: 緑
            new Color(0.75f, 0.75f, 1f),     // 30-39WAVE: 青
            new Color(0.6f, 0.6f, 0.6f),     // 40-49WAVE: 暗め
            new Color(0.9f, 0.7f, 1f)        // 50-59WAVE: 紫
        };

        // ステージ内のすべての画像の色を、元の透明度を保ちながら掛け算（Tint）で染める
        SpriteRenderer[] srs = stageObj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            Color original = sr.color;
            Color tint = bgColors[colorPhase];
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