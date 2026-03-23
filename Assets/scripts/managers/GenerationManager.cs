using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ============================================================
// GenerationManager.cs — Singleton
// Nüfus takibi, doğum, nesil yönetimi
// ============================================================
public class GenerationManager : MonoBehaviour
{
    public static GenerationManager Instance;

    [Header("Nüfus")]
    public List<MinionAI> allMinyons = new List<MinionAI>();
    public int            maxPopulation = 20;

    [Header("Doğum Ayarları")]
    public GameObject minionPrefab;         // Inspector'dan ata
    public Transform  spawnArea;            // Minyonların doğacağı alan

    [Header("İsim Havuzu")]
    public List<string> maleNames   = new List<string>
        {"Ahmet","Mehmet","Ali","Hasan","Arda","Berk","Omer","Yusuf","Emre","Can"};
    public List<string> femaleNames = new List<string>
        {"Ayşe","Fatma","Zeynep","Elif","Selin","Deniz","Merve","Büşra","İrem","Hira"};

    // Nesil istatistikleri
    Dictionary<int,int> generationDeaths = new Dictionary<int,int>();
    Dictionary<int,int> generationBirths = new Dictionary<int,int>();

    void Awake() { Instance = this; }

    void Start()
    {
        // Sahnedeki başlangıç minyonlarını bul
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
            if(!allMinyons.Contains(m)) allMinyons.Add(m);
    }

    // ── NÜFUS ─────────────────────────────────────────────

    public int GetTotalAliveCount()
        => allMinyons.Count(m => m != null && m.stats.isAlive);

    public List<MinionAI> GetAlive()
        => allMinyons.Where(m => m != null && m.stats.isAlive).ToList();

    public int GetCurrentGeneration()
    {
        var alive = GetAlive();
        return alive.Count > 0 ? alive.Max(m => m.stats.generation) : 1;
    }

    // ── DOĞUM ─────────────────────────────────────────────

    public void TryBirth(MinionAI parent1, MinionAI parent2)
    {
        if(GetTotalAliveCount() >= maxPopulation) return;
        if(!parent1.stats.isAlive || !parent2.stats.isAlive) return;

        // Doğum ihtimali: ebeveyn sağlığına göre
        float birthChance = (parent1.stats.health + parent2.stats.health) / 200f;
        if(Random.value > birthChance) return;

        // Ölü doğum ihtimali
        bool stillbirth = parent1.stats.health < 40f || parent2.stats.health < 40f;
        if(stillbirth && Random.value < 0.2f)
        {
            NotificationManager.Instance.Show(
                parent1.stats.name + "'ın bebeği hayatta kalamadı. Derin üzüntü.", NotificationType.Death);
            parent1.GetComponent<EmotionSystem>()?.SetEmotion("Sad", 0.9f);
            parent2.GetComponent<EmotionSystem>()?.SetEmotion("Sad", 0.9f);
            return;
        }

        SpawnMinyon(parent1, parent2);
    }

    void SpawnMinyon(MinionAI p1, MinionAI p2)
    {
        if(minionPrefab == null)
        {
            Debug.LogWarning("GenerationManager: minionPrefab atanmamış!");
            return;
        }

        bool   isFemale = Random.value > 0.5f;
        string newName  = isFemale
            ? femaleNames[Random.Range(0, femaleNames.Count)]
            : maleNames[Random.Range(0, maleNames.Count)];

        // Duplicate isim kontrolü
        int suffix = 1;
        string baseName = newName;
        while(allMinyons.Any(m => m != null && m.stats.name == newName))
            newName = baseName + suffix++;

        // Spawn pozisyonu
        Vector3 pos = spawnArea != null
            ? spawnArea.position + Random.insideUnitSphere * 2f
            : p1.transform.position + Vector3.right;
        pos.y = p1.transform.position.y;

        var go      = Instantiate(minionPrefab, pos, Quaternion.identity);
        var minyon  = go.GetComponent<MinionAI>();
        if(minyon == null) { Destroy(go); return; }

        // Stats
        int newGen = Mathf.Max(p1.stats.generation, p2.stats.generation) + 1;
        minyon.stats.Initialize(newGen, isFemale, newName);
        minyon.stats.parent1 = p1;
        minyon.stats.parent2 = p2;

        // Kart mirası — %30 ihtimalle ebeveyn kartı alır
        var childCards  = minyon.GetComponent<CardSystem>();
        var p1Cards     = p1.GetComponent<CardSystem>();
        var p2Cards     = p2.GetComponent<CardSystem>();

        if(childCards != null)
        {
            if(p1Cards != null)
                foreach(var card in p1Cards.activeCards)
                    if(Random.value < 0.3f) childCards.AddCard(card);

            if(p2Cards != null)
                foreach(var card in p2Cards.activeCards)
                    if(Random.value < 0.3f) childCards.AddCard(card);
        }

        // Travma mirası
        var childMem = minyon.GetComponent<MemorySystem>();
        var p1Mem    = p1.GetComponent<MemorySystem>();
        var p2Mem    = p2.GetComponent<MemorySystem>();
        if(childMem != null && p1Mem != null && p2Mem != null)
            childMem.InheritTraumas(p1Mem, p2Mem);

        // Ebeveyn listelerine ekle
        if(!p1.stats.children.Contains(minyon)) p1.stats.children.Add(minyon);
        if(!p2.stats.children.Contains(minyon)) p2.stats.children.Add(minyon);

        // Sisteme kaydet
        allMinyons.Add(minyon);

        // Nesil sayacı
        if(!generationBirths.ContainsKey(newGen)) generationBirths[newGen] = 0;
        generationBirths[newGen]++;

        NotificationManager.Instance.Show(
            newName + " dünyaya geldi! (" + newGen + ". nesil)",
            NotificationType.Social);
    }

    // ── ÖLÜM ──────────────────────────────────────────────

    public void OnMinionDied(MinionAI m)
    {
        if(!generationDeaths.ContainsKey(m.stats.generation))
            generationDeaths[m.stats.generation] = 0;
        generationDeaths[m.stats.generation]++;

        // Ebeveynlerin çocuk listesinden çıkar
        if(m.stats.parent1 != null) m.stats.parent1.stats.children.Remove(m);
        if(m.stats.parent2 != null) m.stats.parent2.stats.children.Remove(m);

        // Cascade: ölüm
        m.GetComponent<CascadeSystem>()?.TriggerCascade("Death", m.stats.name, 0.6f);

        // Nesil tükeniyor mu?
        int alive = GetTotalAliveCount();
        if(alive <= 1)
            NotificationManager.Instance.Show(
                "Köyde sadece bir kişi kaldı...", NotificationType.Major);
    }

    // ── İSTATİSTİK ────────────────────────────────────────

    public string GetGenerationSummary()
    {
        int maxGen = GetCurrentGeneration();
        return $"Mevcut nesil: {maxGen} | Toplam nüfus: {GetTotalAliveCount()}";
    }
}
