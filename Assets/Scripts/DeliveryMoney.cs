using UnityEngine;

public class DeliveryMoney : MonoBehaviour
{
    [Header("Money")]
    [SerializeField] private int amount = 10;

    private DeliveryStation deliveryStation;
    private bool collected;


    public void Setup(
        int moneyAmount,
        DeliveryStation station
    )
    {
        amount = moneyAmount;
        deliveryStation = station;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag("Player"))
            return;

        collected = true;


        // ????? ??? ??? ?? ????? ??? ????
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(amount);
        }
        else
        {
            Debug.LogError(
                "MoneyManager Instance not found!"
            );
        }


        // ??? ???? ?? Delivery Station
        if (deliveryStation != null)
        {
            deliveryStation.OnMoneyCollected();
        }


        Destroy(gameObject);
    }
}