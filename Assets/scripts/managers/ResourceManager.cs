using UnityEngine;
using System.Collections.Generic;

// ============================================================
// ResourceManager.cs — Singleton
// Köy kaynaklarını yönetir
// ============================================================
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("Kaynaklar")]
    public float woodCount  = 10f;
    public float foodCount  = 10f;
    public float stoneCount = 0f;
    public float waterCount = 20f;

    [Header("Bozulma")]
    public float foodDecayPerDay = 2f;  // her gün ne kadar yiyecek bozulur

    [Header("Depo Kapasitesi")]
    public float maxWood  = 200f;
    public float maxFood  = 100f;
    public float maxStone = 200f;
    public float maxWater = 100f;

    // İcat bonusları
    bool hasStorage  = false;
    bool hasGranary  = false;

    void Awake() { Instance = this; }

    void Start()
    {
        InvokeRepeating(nameof(DailyDecay), TimeManager.Instance.dayDuration,
                                            TimeManager.Instance.dayDuration);
    }

    void DailyDecay()
    {
        // Depo yoksa yiyecek bozulur
        float decayMult = hasStorage ? 0.3f : 1f;
        decayMult       = hasGranary ? 0.1f : decayMult;

        foodCount = Mathf.Max(0, foodCount - foodDecayPerDay * decayMult);
    }

    // ── KAYNAK EKLEME/ÇIKARMA ─────────────────────────────

    public bool Spend(string resource, float amount)
    {
        switch(resource)
        {
            case "wood":
                if(woodCount < amount) return false;
                woodCount -= amount; return true;
            case "food":
                if(foodCount < amount) return false;
                foodCount -= amount; return true;
            case "stone":
                if(stoneCount < amount) return false;
                stoneCount -= amount; return true;
            case "water":
                if(waterCount < amount) return false;
                waterCount -= amount; return true;
        }
        return false;
    }

    public void Add(string resource, float amount)
    {
        switch(resource)
        {
            case "wood":  woodCount  = Mathf.Min(woodCount  + amount, maxWood);  break;
            case "food":  foodCount  = Mathf.Min(foodCount  + amount, maxFood);  break;
            case "stone": stoneCount = Mathf.Min(stoneCount + amount, maxStone); break;
            case "water": waterCount = Mathf.Min(waterCount + amount, maxWater); break;
        }
    }

    public float Get(string resource)
    {
        switch(resource)
        {
            case "wood":  return woodCount;
            case "food":  return foodCount;
            case "stone": return stoneCount;
            case "water": return waterCount;
        }
        return 0f;
    }

    // İcat sistemi entegrasyonu
    public void OnInvention(string item)
    {
        switch(item)
        {
            case "Storage":  hasStorage = true; maxFood += 50f; break;
            case "Granary":  hasGranary = true; maxFood += 100f; foodDecayPerDay = 0.5f; break;
        }
    }
}
