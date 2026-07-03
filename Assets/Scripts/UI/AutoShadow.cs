using UnityEngine;

public class AutoShadow : MonoBehaviour
{
    [Header("影の調整")]
    public float shadowOffset = -0.4f;
    public float shadowScaleX = 0.8f;
    public float shadowScaleY = 0.3f;
    public float shadowAlpha = 0.4f;

    [Header("影のレイヤー設定")]
    public int shadowSortingOrder = -20; // ★追加：絶対に他のキャラの下敷きになるように、数値をガッツリ下げる

    [Header("敵の影設定")]
    public bool isEnemyShadow = false;

    private SpriteRenderer targetRenderer;
    private SpriteRenderer shadowRenderer;

    void Start()
    {
        // 敵の影がOFF設定なら消滅
        if (isEnemyShadow && GameManager.instance != null && !GameManager.instance.showEnemyShadows)
        {
            Destroy(this);
            return;
        }

        // 本体の画像コンポーネントを探す
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            if (r.enabled && r.gameObject.name != "Shadow")
            {
                targetRenderer = r;
                break;
            }
        }

        if (targetRenderer == null) return;

        // 影オブジェクトを生成
        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(this.transform);

        shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
        shadowRenderer.color = new Color(0f, 0f, 0f, shadowAlpha);
        shadowRenderer.sortingLayerID = targetRenderer.sortingLayerID;

        // ★修正1：今まで「本体の1個奥」だったのをやめ、強制的に「-20」などの大底に配置して被りを防ぐ！
        shadowRenderer.sortingOrder = shadowSortingOrder;
    }

    void LateUpdate()
    {
        if (targetRenderer == null || shadowRenderer == null) return;

        // 本体の画像をリアルタイムにコピー
        shadowRenderer.sprite = targetRenderer.sprite;
        shadowRenderer.flipX = targetRenderer.flipX;
        shadowRenderer.color = new Color(0f, 0f, 0f, shadowAlpha);

        // ==========================================
        // ★修正2：進化による巨大化や、ぴょんぴょん跳ねる影響を完全に打ち消す！
        // ==========================================

        // ①大きさの補正（親が進化して何倍に大きくなっても、割り算をして設定した影のサイズを強引に維持する）
        float parentScaleX = Mathf.Max(Mathf.Abs(transform.localScale.x), 0.01f);
        float parentScaleY = Mathf.Max(Mathf.Abs(transform.localScale.y), 0.01f);
        shadowRenderer.transform.localScale = new Vector3(shadowScaleX / parentScaleX, shadowScaleY / parentScaleY, 1f);

        // ②位置の補正（親が上にぴょんっと跳ねても、影は地面の位置から動かないようにする）
        shadowRenderer.transform.position = new Vector3(transform.position.x, transform.position.y + shadowOffset, transform.position.z);
    }
}