using UnityEngine;

public class DeliveryMoney : MonoBehaviour
{
    public enum MoneyType
    {
        Delivery,
        Eating
    }

    [SerializeField] private int amount = 10;

    private DeliveryStation deliveryStation;
    private MoneyType moneyType;
    private bool collected;

    public void Setup(int moneyAmount, DeliveryStation station)
    {
        amount = moneyAmount;
        deliveryStation = station;
        moneyType = MoneyType.Delivery;
        collected = false;
    }

    public void SetupEatingMoney(int moneyAmount, DeliveryStation station)
    {
        amount = moneyAmount;
        deliveryStation = station;
        moneyType = MoneyType.Eating;
        collected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        collected = true;

        if (MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(amount);

        if (deliveryStation != null)
        {
            if (moneyType == MoneyType.Delivery)
                deliveryStation.OnMoneyCollected(this);
            else
                deliveryStation.OnEatingMoneyCollected(this);

            deliveryStation.RecycleMoney(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
