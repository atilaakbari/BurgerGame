using UnityEngine;

public class LevelUnlockActivator : MonoBehaviour
{
    [Header("Required Level")]
    [SerializeField] private int requiredLevel = 2;

    [Header("Zone")]
    [SerializeField] private GameObject unlockZone;

    [Header("Save Zone ID")]
    [SerializeField] private string zoneId =
        "Zone_SodaStation";

    private bool lastVisibleState;

    private void Start()
    {
        UpdateZone();
    }

    private void Update()
    {
        UpdateZone();
    }

    private void UpdateZone()
    {
        if (unlockZone == null)
            return;

        bool alreadyUnlocked =
            SaveManager.Instance != null &&
            !string.IsNullOrEmpty(zoneId) &&
            SaveManager.Instance
                .IsZoneUnlocked(zoneId);

        if (alreadyUnlocked)
        {
            if (unlockZone.activeSelf)
                unlockZone.SetActive(false);

            return;
        }

        int currentLevel = 1;

        if (XPManager.Instance != null)
        {
            currentLevel =
                XPManager.Instance.Level;
        }

        bool shouldBeVisible =
            currentLevel >= requiredLevel;

        if (
            lastVisibleState !=
            shouldBeVisible
        )
        {
            lastVisibleState =
                shouldBeVisible;

            unlockZone.SetActive(
                shouldBeVisible
            );
        }
        else if (
            unlockZone.activeSelf !=
            shouldBeVisible
        )
        {
            unlockZone.SetActive(
                shouldBeVisible
            );
        }
    }
}