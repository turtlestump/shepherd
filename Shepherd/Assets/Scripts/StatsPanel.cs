using TMPro;
using UnityEngine;

public class SheepStatsPanel : MonoBehaviour
{
    [Header("Header")]
    public TMP_Text infoText;

    [Header("Stats")]
    public TMP_Text statsText;

    Sheep currentSheep;

    void OnEnable()
    {
        SheepIcon.OnSheepSelected += UpdatePanel;
    }

    void OnDisable()
    {
        SheepIcon.OnSheepSelected -= UpdatePanel;
    }

    void UpdatePanel(Sheep sheep)
    {
        currentSheep = sheep;

        if (sheep == null)
        {
            Clear();
            return;
        }

        infoText.SetText($"Name: {sheep.name}\n" +
                         $"Level: {sheep.level}");

        statsText.SetText(
            $"Strength: {sheep.strength}\n\n" +
            $"Resolve: {sheep.resolve}\n\n" +
            $"Charm: {sheep.charm}\n\n" +
            $"Speed: {sheep.speed}\n\n" +
            $"XP: {sheep.xp}/{sheep.xpToNextLevel}"
        );
    }

    void Clear()
    {
        infoText.SetText("");
        statsText.SetText("");
    }
}