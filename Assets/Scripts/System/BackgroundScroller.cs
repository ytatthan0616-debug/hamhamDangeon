using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("スクロール速度")]
    public float scrollSpeed = 2.0f;

    [Header("背景画像の縦幅（Yサイズ）")]
    public float backgroundHeight = 10.0f;

    [Header("背景画像の登録（設定忘れ防止）")]
    public Transform bg1;
    public Transform bg2;

    [Header("ステージ色変更の設定")]
    public int wavesPerStage = 10;
    public float colorTransitionSpeed = 1.5f;

    // ★修正1：publicのままだとUnityが過去の設定（全部白）を優先してしまうため、
    // privateにしてプログラム内の色を強制適用します！
    // ★修正1：0〜100WAVEまでの色を登録
    private Color[] stageColors = new Color[] {
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

    private SpriteRenderer sr1;
    private SpriteRenderer sr2;
    private Color targetColor = Color.white;
    private int lastWave = -1;

    void Start()
    {
        // ★修正2：インスペクターでBG1, BG2をセットし忘れても、自動で子オブジェクトを探す安全装置を追加
        if (bg1 == null && transform.childCount > 0) bg1 = transform.GetChild(0);
        if (bg2 == null && transform.childCount > 1) bg2 = transform.GetChild(1);

        if (bg1 != null) sr1 = bg1.GetComponent<SpriteRenderer>();
        if (bg2 != null) sr2 = bg2.GetComponent<SpriteRenderer>();

        if (GameManager.instance != null)
        {
            UpdateTargetColor(GameManager.instance.currentWave);
            lastWave = GameManager.instance.currentWave;
            if (sr1 != null) sr1.color = targetColor;
            if (sr2 != null) sr2.color = targetColor;
        }
    }

    void Update()
    {
        if (bg1 != null) bg1.position += Vector3.down * scrollSpeed * Time.deltaTime;
        if (bg2 != null) bg2.position += Vector3.down * scrollSpeed * Time.deltaTime;

        if (bg1 != null && bg1.position.y <= -backgroundHeight)
        {
            bg1.position += new Vector3(0f, backgroundHeight * 2f, 0f);
        }

        if (bg2 != null && bg2.position.y <= -backgroundHeight)
        {
            bg2.position += new Vector3(0f, backgroundHeight * 2f, 0f);
        }

        if (GameManager.instance != null)
        {
            int currentWave = GameManager.instance.currentWave;
            if (currentWave != lastWave)
            {
                UpdateTargetColor(currentWave);
                lastWave = currentWave;
            }
        }

        if (sr1 != null && sr2 != null)
        {
            // 目標の色に向かって、毎フレーム少しずつ滑らかに色を変化させる
            sr1.color = Color.Lerp(sr1.color, targetColor, Time.deltaTime * colorTransitionSpeed);
            sr2.color = sr1.color;
        }
    }

    void UpdateTargetColor(int currentWave)
    {
        // 進行度合いを計算：現在のWAVE ÷ 10
        int stageIndex = currentWave / wavesPerStage;

        if (stageIndex <= 10)
        {
            targetColor = stageColors[stageIndex];
        }
        else
        {
            // 110WAVE以降はランダム（StageManagerと同じ色になるようにシードを同期）
            Random.InitState(stageIndex);
            targetColor = new Color(Random.Range(0.6f, 1f), Random.Range(0.6f, 1f), Random.Range(0.6f, 1f));
            Random.InitState(System.Environment.TickCount);
        }
    }
}