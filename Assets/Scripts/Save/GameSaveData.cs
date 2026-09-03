using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public int money = 0;

    // آپگرید همه‌ی استیشن‌ها (کوکینگ، کاتینگ، هر چی بعداً اضافه شد) با شناسه‌ی یکتا
    public List<StationSaveEntry> stationLevels = new List<StationSaveEntry>();

    // شناسه‌ی همه‌ی UnlockZone هایی که قبلاً باز شدن
    public List<string> unlockedZoneIds = new List<string>();
}

[Serializable]
public class StationSaveEntry
{
    public string id;
    public int level;
}