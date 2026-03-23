using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// ============================================================
// DemoSceneSetup.cs
// Bu scripti sahnede boş bir GameObject'e ekle.
// Play'e basınca sahneyi otomatik kurar:
//   - Düz zemin (NavMesh)
//   - Ağaç ve Balık noktaları
//   - 6 Minyon (4 Erkek, 2 Kadın)
//   - Tüm Manager'lar
// ============================================================
public class DemoSceneSetup : MonoBehaviour
{
    [Header("Prefab'lar (Inspector'dan bağla)")]
    public GameObject minionPrefab;      // MinionAI + tüm componentler
    public GameObject treePrefab;        // Tag: "Tree"
    public GameObject fishingSpotPrefab; // Tag: "FishingSpot"
    public GameObject shelterPrefab;     // Tag: "Shelter"
    public GameObject campfirePrefab;    // Tag: "Campfire"

    [Header("Prefab yoksa primitiv kullan")]
    public bool usePrimitives = true;

    void Awake()
    {
        // Manager'ları oluştur
        EnsureManager<TimeManager>("TimeManager");
        EnsureManager<ResourceManager>("ResourceManager");
        EnsureManager<GenerationManager>("GenerationManager");
        EnsureManager<PowerSystem>("PowerSystem");
        EnsureManager<InventionManager>("InventionManager");
        EnsureManager<NotificationManager>("NotificationManager");
        EssenceManager em = EnsureManager<EssenceManager>("EssenceManager");
        EnsureManager<EventLogSystem>("EventLogSystem");
    }

    void Start()
    {
        if (usePrimitives)
            BuildPrimitiveScene();
        else
            BuildPrefabScene();

        // 1. KAYNAKLARI GÜÇLÜ BAŞLAT (6 kişi oldukları için biraz artırdık)
        ResourceManager.Instance.woodCount = 50f; // Ev inşaatını hızlıca tetikler
        ResourceManager.Instance.foodCount = 100f; // Açlıktan ölmelerini engeller
        ResourceManager.Instance.waterCount = 80f;

        // 2. AHMET VE AYŞE'NİN ÖZEL DURUMU & GENEL MUTLULUK
        MinionAI ahmet = GenerationManager.Instance.allMinyons.Find(m => m.stats.name == "Ahmet");
        MinionAI ayse = GenerationManager.Instance.allMinyons.Find(m => m.stats.name == "Ayşe");

        if (ahmet != null)
        {
            // Ahmet'i anında lider yap
            PowerSystem.Instance.SetLeader(ahmet);
        }

        // Tüm minyonları oyuna mutlu başlatalım
        foreach (var minion in GenerationManager.Instance.allMinyons)
        {
            minion.GetComponent<EmotionSystem>().SetEmotion("Happy", 0.8f);

            // Ahmet ve Ayşe'yi yine de çift yapalım ki hemen nesil üremeye başlasın
            if (minion.stats.name == "Ahmet" && ayse != null)
                minion.GetComponent<SocialNetwork>().relationships[ayse.stats.name] = 75f;
            else if (minion.stats.name == "Ayşe" && ahmet != null)
                minion.GetComponent<SocialNetwork>().relationships[ahmet.stats.name] = 75f;
        }
    }

    // ── PRİMİTİV SAHNE (prefab olmadan test) ─────────────
    void BuildPrimitiveScene()
    {
        // Zemin
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ground.GetComponent<Renderer>().material.color = new Color(0.4f, 0.6f, 0.3f);

        SpawnAllMinions();
    }

    void BuildPrefabScene()
    {
        SpawnAllMinions();
    }

    // ── 6 MİNYONU SPAWN ETME MERKEZİ ─────────────────────
    void SpawnAllMinions()
    {
        // ERKEKLER (4 Kişi)
        SpawnMinyon("Ahmet", false, new Vector3(-2f, 0f, -2f), new[] { "Sinir", "Oduncu" });
        SpawnMinyon("Mehmet", false, new Vector3(-4f, 0f, 0f), new[] { "Zeka", "İnşaatçı" });
        SpawnMinyon("Ali", false, new Vector3(-2f, 0f, 2f), new[] { "Tembel", "Maceracı" });
        SpawnMinyon("Veli", false, new Vector3(0f, 0f, -4f), new[] { "Hızlı", "Avcı" });

        // KADINLAR (2 Kişi)
        SpawnMinyon("Ayşe", true, new Vector3(2f, 0f, -2f), new[] { "Sosyal", "Avcı" });
        SpawnMinyon("Fatma", true, new Vector3(4f, 0f, 0f), new[] { "Şüpheci", "Şaman" });
    }

    // ── MİNYON SPAWN ─────────────────────────────────────
    void SpawnMinyon(string name, bool female, Vector3 pos, string[] cardList)
    {
        GameObject go;

        if (minionPrefab != null)
        {
            go = Instantiate(minionPrefab, pos, Quaternion.identity);
        }

        else
        {
            // Prefab yoksa primitiv + gerekli componentler
            go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.transform.position = pos;
            AddComponents(go);
        }
        

        

        var ai = go.GetComponent<MinionAI>();
        ai.stats.Initialize(1, female, name);

        // YENİ: DemoScene içindeki prefabı minyona aktar
        ai.shelterPrefab = this.shelterPrefab;
        go.name = name;

        var cs = go.GetComponent<CardSystem>();
        foreach (var card in cardList) cs.AddCard(card);

        // Renk — kadın/erkek farkı
        var r = go.GetComponentInChildren<Renderer>();
        if (r != null)
            r.material.color = female
                ? new Color(0.9f, 0.7f, 0.8f)
                : new Color(0.7f, 0.8f, 0.9f);

        // GenerationManager'a kaydet
        GenerationManager.Instance.allMinyons.Add(ai);

        Debug.Log($"[Demo] {name} oluşturuldu. Kartlar: {string.Join(", ", cardList)}");
    }

    void AddComponents(GameObject go)
    {
        // Sadece prefab yoksa elle ekle
        if (!go.GetComponent<NavMeshAgent>()) go.AddComponent<NavMeshAgent>();
        if (!go.GetComponent<MinionAI>()) go.AddComponent<MinionAI>();
        if (!go.GetComponent<NeedsSystem>()) go.AddComponent<NeedsSystem>();
        if (!go.GetComponent<EmotionSystem>()) go.AddComponent<EmotionSystem>();
        if (!go.GetComponent<MemorySystem>()) go.AddComponent<MemorySystem>();
        if (!go.GetComponent<CardSystem>()) go.AddComponent<CardSystem>();
        if (!go.GetComponent<SocialNetwork>()) go.AddComponent<SocialNetwork>();
        if (!go.GetComponent<CascadeSystem>()) go.AddComponent<CascadeSystem>();
        if (!go.GetComponent<RandomEventSystem>()) go.AddComponent<RandomEventSystem>();
    }

    T EnsureManager<T>(string goName) where T : MonoBehaviour
    {
        var existing = FindFirstObjectByType<T>();
        if (existing != null) return existing;

        var go = new GameObject(goName);
        return go.AddComponent<T>();
    }
}