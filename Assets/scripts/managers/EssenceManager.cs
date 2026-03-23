using UnityEngine;

// ============================================================
// EssenceManager.cs — Singleton
// Oyuncunun intervention para birimi
// Her gün hayatta kalan minyondan kazanılır
// Minyon ölünce büyük miktar bırakır
// ============================================================
public class EssenceManager : MonoBehaviour
{
    public static EssenceManager Instance;

    [Header("Essence")]
    public float essence     = 0f;
    public float maxEssence  = 100f;

    [Header("Kazanma Oranları")]
    public float perMinionPerDay  = 1f;   // her minyon her gün
    public float deathBonus       = 20f;  // ölünce
    public float inventionBonus   = 10f;  // icat yapılınca

    void Awake() { Instance = this; }

    void Start()
    {
        // Her gün essence kazan
        InvokeRepeating(nameof(DailyGain), TimeManager.Instance.dayDuration,
                                           TimeManager.Instance.dayDuration);
    }

    void DailyGain()
    {
        int alive = GenerationManager.Instance.GetTotalAliveCount();
        AddEssence(alive * perMinionPerDay);
    }

    public void AddEssence(float amount)
    {
        essence = Mathf.Clamp(essence + amount, 0f, maxEssence);
    }

    // ── OYUNCU MÜDAHALELERİ ──────────────────────────────

    // Yağmur yağdır — 5 essence
    public bool TriggerRain()
    {
        if(!Spend(5f)) return false;
        TimeManager.Instance.currentSeason = 0; // ilkbahara al
        NotificationManager.Instance.Show("Yağmur geldi!", NotificationType.Nature);
        return true;
    }

    // Hayvan sürüsü gönder — 8 essence
    public bool TriggerAnimalHerd()
    {
        if(!Spend(8f)) return false;
        ResourceManager.Instance.Add("food", 20f);
        NotificationManager.Instance.Show("Hayvan sürüsü geçiyor! Avcılar için fırsat.", NotificationType.Nature);
        return true;
    }

    // Bir minyona fikir sok — 3 essence
    public bool InjectIdea(MinionAI target)
    {
        if(target == null || !target.stats.isAlive) return false;
        if(!Spend(3f)) return false;

        // Rastgele pozitif duygu ver
        target.GetComponent<EmotionSystem>()?.SetEmotion("Euphoric", 0.6f);
        target.GetComponent<NeedsSystem>()?.Change("boredom", -30f);
        NotificationManager.Instance.Show(
            target.stats.name + " aniden ilham aldı!", NotificationType.Surprising);
        return true;
    }

    // Kuraklık başlat — 10 essence (olumsuz müdahale, strateji için)
    public bool TriggerDrought()
    {
        if(!Spend(10f)) return false;
        ResourceManager.Instance.waterCount = 2f;
        NotificationManager.Instance.Show("Kuraklık başladı!", NotificationType.Major);

        // Şaman fark eder
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive) continue;
            if(m.GetComponent<CardSystem>()?.HasCard("Şaman") ?? false)
                NotificationManager.Instance.Show(
                    m.stats.name + " bu kuraklığın doğal olmadığını hissetti...", NotificationType.Religion);
        }
        return true;
    }

    // Mutasyonu güçlendir — 15 essence
    public bool BoostMutation(MinionAI target)
    {
        if(target == null || !target.stats.isAlive) return false;
        if(!Spend(15f)) return false;

        target.stats.workSpeed   += 0.2f;
        target.stats.learningRate+= 0.3f;
        NotificationManager.Instance.Show(
            target.stats.name + "'ın mutasyonu güçlendi!", NotificationType.Evolution);
        return true;
    }

    bool Spend(float amount)
    {
        if(essence < amount) return false;
        essence -= amount;
        return true;
    }
}
