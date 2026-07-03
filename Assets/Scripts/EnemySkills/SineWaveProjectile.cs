using UnityEngine;

public class SineWaveProjectile : MonoBehaviour
{
    public float speed = 4.0f;
    public float frequency = 8.0f;  // ウネウネの細かさ
    public float amplitude = 1.5f;  // ウネウネの横幅
    public float lifeTime = 3.0f;   // すぐ消えるように短め
    private float damage = 10f;

    private Vector2 startPos;
    private Vector2 forwardDir;
    private Vector2 rightDir;
    private float timer = 0f;
    private bool invert = false;

    public void Initialize(float dmg, Vector2 dir, bool invertWave)
    {
        damage = dmg;
        forwardDir = dir.normalized;
        // 進行方向に対して垂直（横）のベクトルを計算
        rightDir = new Vector2(-forwardDir.y, forwardDir.x);
        invert = invertWave;
        startPos = transform.position;

        // ドクロの画像を進行方向に向ける
        float angle = Mathf.Atan2(forwardDir.y, forwardDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float currentAmplitude = invert ? -amplitude : amplitude;

        // まっすぐ進む力 ＋ 横に揺れる力（サイン波）
        Vector2 linearPos = startPos + forwardDir * (speed * timer);
        Vector2 sineOffset = rightDir * (Mathf.Sin(timer * frequency) * currentAmplitude);

        transform.position = linearPos + sineOffset;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (GameManager.instance != null && !GameManager.instance.isGameOver)
            {
                GameManager.instance.currentHP -= damage;
            }
            Destroy(gameObject);
        }
    }
}