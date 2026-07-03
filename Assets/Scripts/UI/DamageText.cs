using UnityEngine;

public class DamageText : MonoBehaviour
{
    private TextMesh textMesh;
    private MeshRenderer meshRenderer; // ★追加：描画順を変えるために必要
    private float moveSpeed = 1.2f;    // 少しだけ上昇を速くして見やすく
    private float duration = 0.6f;
    private float timer = 0f;
    private Color startColor;

    void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        meshRenderer = GetComponent<MeshRenderer>(); // ★追加

        // ★重要：プログラムから強制的に描画順を「最前面」に引き上げる
        if (meshRenderer != null)
        {
            // ハムスターや敵が使っているSortingLayerの名前（"Default" など）に合わせます
            meshRenderer.sortingLayerName = "Default";
            // 既存のキャラクター（Order: 10〜50程度）よりも圧倒的に大きい数値を指定して手前に出す
            meshRenderer.sortingOrder = 500;
        }
    }

    public void Setup(string text, Color color)
    {
        if (textMesh == null) textMesh = GetComponent<TextMesh>();
        textMesh.text = text;
        textMesh.color = color;
        startColor = color;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 1. 上方向にふわっと移動
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. 徐々に透明にする (フェードアウト)
        float alpha = Mathf.Lerp(1f, 0f, timer / duration);
        if (textMesh != null)
        {
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}