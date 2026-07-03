using UnityEngine;
using UnityEngine.UI;
using TMPro; // ★追加：TextMeshProを扱うために必要

public class AdBuffButton : MonoBehaviour
{
    private Button myButton;

    // ★修正：Text型からTextMeshProUGUI型に変更
    public TextMeshProUGUI buttonText;

    void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnClicked);
    }

    void Update()
    {
        if (GameManager.instance == null) return;

        if (GameManager.instance.isAdBuffActive)
        {
            // バフ作動中はボタンを押せなくし、残り時間を表示
            myButton.interactable = false;
            int minutes = Mathf.FloorToInt(GameManager.instance.adBuffTimer / 60f);
            int seconds = Mathf.FloorToInt(GameManager.instance.adBuffTimer % 60f);

            if (buttonText != null)
            {
                buttonText.text = "ボーナス中 " + minutes.ToString() + ":" + string.Format("{0:00}", seconds);
            }
        }
        else
        {
            // バフが切れたら再び押せるようにする
            myButton.interactable = true;
            if (buttonText != null)
            {
                buttonText.text = "広告を見てEXP＆エサ2倍！";
            }
        }
    }

    void OnClicked()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ActivateAdBuff();
        }
    }
}