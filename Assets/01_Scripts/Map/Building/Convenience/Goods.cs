using UnityEngine;

public class Goods : Interactable
{
    protected override void Interact()
    {
        BuyGoods();
    }

    private void BuyGoods()
    {
        Debug.Log("이 상품을 구매하시겠습니까?");
    }
}
