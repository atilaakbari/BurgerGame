using UnityEngine;

public static class GameRuntime
{
    public const int TargetFrameRate = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        Application.targetFrameRate = TargetFrameRate;
        QualitySettings.vSyncCount = 0;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Debug.unityLogger.filterLogType = LogType.Error;

        QualitySettings.pixelLightCount = 1;
        QualitySettings.shadowDistance = 28f;
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.skinWeights = SkinWeights.TwoBones;
        QualitySettings.particleRaycastBudget = 16;
        QualitySettings.asyncUploadTimeSlice = 2;
        QualitySettings.asyncUploadBufferSize = 16;

        /*if (Object.FindFirstObjectByType<MoneyHUD>() == null)
        {
            GameObject hud = new GameObject("MoneyHUD");
            hud.AddComponent<MoneyHUD>();
        }*/
    }
}
