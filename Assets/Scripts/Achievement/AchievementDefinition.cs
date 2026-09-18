using UnityEngine;

[CreateAssetMenu(fileName = "Achievement", menuName = "Burger/Achievement")]
public class AchievementDefinition : ScriptableObject
{
    public AchievementId id;
    public string title = "Achievement";
    [TextArea] public string description = "";
    public Sprite icon;

    [Header("Goal")]
    public int targetValue = 1;

    [Header("Reward (اختیاری)")]
    public int rewardMoney = 0;
    public int rewardXP = 0;
}