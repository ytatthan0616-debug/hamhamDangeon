using UnityEngine;

public class FoodBehavior : MonoBehaviour
{
    // エサの種類を判別するためだけのシンプルな設計にします
    public GameManager.FoodType foodType;



    [Header("エフェクト設定")]
    public Color effectColor = new Color(1f, 1f, 1f, 0.5f);
}