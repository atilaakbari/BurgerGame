using System;
using UnityEngine;

public enum UpgradeCurrency
{
    Money = 0,
    Gem = 1
}

[Serializable]
public class UpgradeCost
{
    public UpgradeCurrency currency = UpgradeCurrency.Money;
    [Min(0)] public int amount = 0;
}

public static class UpgradePayment
{
    public static bool TryPay(UpgradeCost cost)
    {
        if (cost == null || cost.amount <= 0)
            return true;

        if (cost.currency == UpgradeCurrency.Gem)
        {
            if (GemManager.Instance == null)
                return false;
            return GemManager.Instance.TrySpend(cost.amount);
        }

        if (MoneyManager.Instance == null)
            return false;

        return MoneyManager.Instance.TrySpend(cost.amount);
    }

    public static bool CanAfford(UpgradeCost cost)
    {
        if (cost == null || cost.amount <= 0)
            return true;

        if (cost.currency == UpgradeCurrency.Gem)
            return GemManager.Instance != null && GemManager.Instance.Gems >= cost.amount;

        return MoneyManager.Instance != null && MoneyManager.Instance.Money >= cost.amount;
    }
}