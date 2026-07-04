using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TickerMessageUI : MonoBehaviour
{
    public static TickerMessageUI instance;

    [Header("UI参照")]
    public GameObject tickerBackground;
    public RectTransform textRectTransform;
    public TextMeshProUGUI tickerText;

    [Header("設定")]
    public float scrollSpeed = 300f;

    private Queue<string> messageQueue = new Queue<string>();
    private bool isPlaying = false;
    private float canvasWidth;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (tickerBackground != null) tickerBackground.SetActive(false);

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        }
    }

    public void ShowMessage(string msg)
    {
        messageQueue.Enqueue(msg);
        if (!isPlaying) StartCoroutine(PlayTickerRoutine());
    }

    // ★敵の必殺技用（英語化）
    public void ShowBossSkillMessage()
    {
        string[] msgs = { "The enemy unleashes its power!!", "The air trembles...!", "Incoming powerful attack!!" };
        ShowMessage("<color=#ff4444>" + msgs[Random.Range(0, msgs.Length)] + "</color>");
    }

    // ★味方の必殺技用（英語化）
    public void ShowAllySkillMessage()
    {
        string[] msgs = { "Hamster activates a skill!!", "Hidden power awakens!", "Lethal strike!!" };
        ShowMessage("<color=#44ffff>" + msgs[Random.Range(0, msgs.Length)] + "</color>");
    }

    // ★WAVE開始時用（英語化）
    public void ShowWaveMessage(int wave)
    {
        if (wave % 10 == 0)
        {
            string[] msgs = { "The atmosphere shifted...", "The air feels heavy...", "A chilling presence approaches..." };
            ShowMessage("<color=#ffaa00>" + msgs[Random.Range(0, msgs.Length)] + "</color>");
        }
        else if (wave == 5)
        {
            ShowMessage("A horde of slimes is attacking!!");
        }
        else if (wave == 15)
        {
            ShowMessage("A swarm of swift enemies! Watch out!!");
        }
    }

    // ★追加：強敵出現用（英語化）
    public void ShowPowerfulEnemyMessage()
    {
        string[] msgs = {
            "WARNING! A powerful enemy has appeared!!",
            "DANGER! Massive energy detected!!",
            "CAUTION! A formidable foe approaches!!"
        };
        // 赤色で目立つように表示
        ShowMessage("<color=#ff0000>" + msgs[Random.Range(0, msgs.Length)] + "</color>");
    }

    IEnumerator PlayTickerRoutine()
    {
        isPlaying = true;
        if (tickerBackground != null) tickerBackground.SetActive(true);

        while (messageQueue.Count > 0)
        {
            string currentMsg = messageQueue.Dequeue();
            tickerText.text = currentMsg;

            tickerText.ForceMeshUpdate();
            float textWidth = tickerText.preferredWidth;

            float startX = canvasWidth / 2f + 50f;
            float endX = -canvasWidth / 2f - textWidth - 50f;

            textRectTransform.anchoredPosition = new Vector2(startX, textRectTransform.anchoredPosition.y);

            while (textRectTransform.anchoredPosition.x > endX)
            {
                textRectTransform.anchoredPosition -= new Vector2(scrollSpeed * Time.deltaTime, 0);
                yield return null;
            }
        }

        if (tickerBackground != null) tickerBackground.SetActive(false);
        isPlaying = false;
    }
}