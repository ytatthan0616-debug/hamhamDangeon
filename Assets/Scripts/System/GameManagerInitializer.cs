using UnityEngine;

public class GameManagerInitializer : MonoBehaviour
{
    public GameObject gameManagerPrefab; // GameManagerのプレハブを登録

    void Awake()
    {
        // もしGameManagerがシーンにいないなら、自動で呼び出す
        if (GameManager.instance == null)
        {
            if (gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }
            else
            {
                Debug.LogError("GameManagerのプレハブが登録されていません！");
            }
        }
    }
}