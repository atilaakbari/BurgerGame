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