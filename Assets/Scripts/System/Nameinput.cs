using UnityEngine;
using TMPro;

public class NameInputBridge : MonoBehaviour
{
    private TMP_InputField inputField;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();

        if (inputField != null)
        {
            inputField.onEndEdit.AddListener(OnNameSubmit);
        }
    }

    void OnEnable()
    {
        // ステータスパネルを開くたびに、最新の名前を読み込んで表示する
        if (GameManager.instance != null && inputField != null)
        {
            inputField.text = GameManager.instance.GetDisplayName();
        }
    }

    public void OnNameSubmit(string newName)
    {
        if (GameManager.instance != null)
        {
            // 全部消して空欄で確定されたら、カスタムネームをリセット（空文字にする）
            if (string.IsNullOrWhiteSpace(newName))
            {
                newName = "";
            }

            GameManager.instance.ChangeHamsterName(newName);

            // 入力欄の表示を「図鑑のデフォルト名」に更新してあげる
            inputField.text = GameManager.instance.GetDisplayName();
        }
    }
}