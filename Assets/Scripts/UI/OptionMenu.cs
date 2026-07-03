using UnityEngine;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    public Toggle damageToggle;

    void Start()
    {
        if (damageToggle != null && GameManager.instance != null)
        {
            // 現在のGameManagerの設定をUIに反映
            damageToggle.isOn = GameManager.instance.showDamagePopup;

            // トグルがクリックされたときに呼び出すメソッドを登録
            damageToggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    void OnToggleChanged(bool isOn)
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.showDamagePopup = isOn;
        }
    }
}