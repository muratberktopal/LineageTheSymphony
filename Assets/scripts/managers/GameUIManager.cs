using UnityEngine;
using TMPro; // TextMeshPro kütüphanesi

// ============================================================
// GameUIManager.cs
// Ekranda Odun, Yemek, Zaman ve Nüfus bilgilerini gösterir.
// ============================================================
public class GameUIManager : MonoBehaviour
{
    [Header("UI Metinleri (Inspector'dan Baðla)")]
    public TextMeshProUGUI resourceText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI populationText;

    void Update()
    {
        UpdateResources();
        UpdateTime();
        UpdatePopulation();
    }

    void UpdateResources()
    {
        if (ResourceManager.Instance == null || resourceText == null) return;

        // Kaynaklarý tam sayý (F0) olarak gösteriyoruz ki küsuratlar göz yormasýn
        float w = ResourceManager.Instance.woodCount;
        float f = ResourceManager.Instance.foodCount;
        float s = ResourceManager.Instance.stoneCount;
        float wa = ResourceManager.Instance.waterCount;

        resourceText.text = $"Odun: {w:F0} | Yemek: {f:F0} | Su: {wa:F0} | Taþ: {s:F0}";
    }

    void UpdateTime()
    {
        if (TimeManager.Instance == null || timeText == null) return;

        int day = TimeManager.Instance.totalDays;
        string timeStr = TimeManager.Instance.GetTimeString();
        string season = TimeManager.Instance.CurrentSeasonName();

        timeText.text = $"Gün: {day} | Saat: {timeStr} | Mevsim: {season}";
    }

    void UpdatePopulation()
    {
        if (GenerationManager.Instance == null || populationText == null) return;

        int pop = GenerationManager.Instance.GetTotalAliveCount();
        int gen = GenerationManager.Instance.GetCurrentGeneration();

        populationText.text = $"Nüfus: {pop} | Nesil: {gen}";
    }
}