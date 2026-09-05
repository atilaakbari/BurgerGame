using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public int money = 0;

    // آپگرید همه‌ی استیشن‌ها (کوکینگ، کاتینگ، هر چی بعداً اضافه شد) با شناسه‌ی یکتا
    public List<StationSaveEntry> stationLevels = new List<StationSaveEntry>();

    // وضعیت همه‌ی UnlockZone ها - هم باز شده یا نه، هم اگه نشده چقدر پول باقی مونده
    public List<ZoneSaveEntry> zones = new List<ZoneSaveEntry>();

    // پول‌های نقدی که رو زمین (کنار DeliveryStation) مونده و برداشته نشده
    public List<MoneyPileEntry> moneyPiles = new List<MoneyPileEntry>();
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
    public int remainingCost; // فقط وقتی unlocked=false معنی داره
}

[Serializable]
public class MoneyPileEntry
{
    public string stationId;
    public int amount; // مجموع ارزش پول‌های نقدیِ روی زمین این ایستگاه
}