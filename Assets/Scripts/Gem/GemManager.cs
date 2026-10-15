using System;
using UnityEngine;

public class GemManager : MonoBehaviour
{
    public static GemManager Instance { get; private set; }
    public static event Action<int> OnGemsChanged;

    [SerializeField] private int gems;

    public int Gems => gems;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (SaveManager.Instance != null)
            gems = SaveManager.Instance.Data.gems;

        OnGemsChanged?.Invoke(gems);
    }

    public void AddGems(int amount)
    {
        if (amount <= 0)
            return;

        gems += amount;
        OnGemsChanged?.Invoke(gems);
        SyncToSave();
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0)
            return true;

        if (gems < amount)
            return false;

        gems -= amount;
        OnGemsChanged?.Invoke(gems);
        SyncToSave();
        return true;
    }

    private void SyncToSave()
    {
        if (SaveManager.Instance == null)
            return;

        SaveManager.Instance.Data.gems = gems;
        SaveManager.Instance.RequestSave();
    }

    [ContextMenu("Debug/+10 Gems")]
    private void DebugAdd()
    {
        AddGems(10);
    }
}