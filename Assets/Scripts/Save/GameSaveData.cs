using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public int playerLevel = 1;
    public int currentXP = 0;
    public int money = 0;

    public List<StationSaveEntry> stationLevels = new List<StationSaveEntry>();
    public List<ZoneSaveEntry> zones = new List<ZoneSaveEntry>();
    public List<MoneyPileEntry> moneyPiles = new List<MoneyPileEntry>();

    public int totalBurgersDelivered = 0;
    public List<AchievementSaveEntry> achievements = new List<AchievementSaveEntry>();

    public void EnsureLists()
    {
        if (stationLevels == null)
            stationLevels = new List<StationSaveEntry>();

        if (zones == null)
            zones = new List<ZoneSaveEntry>();

        if (moneyPiles == null)
            moneyPiles = new List<MoneyPileEntry>();

        if (achievements == null)
            achievements = new List<AchievementSaveEntry>();

        if (totalBurgersDelivered < 0)
            totalBurgersDelivered = 0;
    }
}

[Serializable]
public class StationSaveEntry
{
    public string id;
    public int level;
}

[Serializable]
public class ZoneSaveEntry
{
    public string id;
    public bool unlocked;
    public int remainingCost;
}

[Serializable]
public class MoneyPileEntry
{
    public string stationId;
    public int amount;
}

[Serializable]
public class AchievementSaveEntry
{
    public string id;
    public bool unlocked;
    public int progress;
}