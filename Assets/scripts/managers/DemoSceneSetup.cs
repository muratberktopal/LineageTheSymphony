using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// ============================================================
// DemoSceneSetup.cs
// Bu scripti sahnede boş bir GameObject'e ekle.
// Play'e basınca sahneyi otomatik kurar:
//   - Düz zemin (NavMesh)
//   - Ağaç ve Balık noktaları
//   - 2 Minyon (Ahmet + Ayşe)
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
        if(usePrimitives)
            BuildPrimitiveScene();
        else
            BuildPrefabScene();

        // Kaynakları başlat
        ResourceManager.Instance.woodCount  = 15f;
        ResourceManager.Instance.foodCount  = 10f;
        ResourceManager.Instance.waterCount = 20f;
    }

    // ── PRİMİTİV SAHNE (prefab olmadan test) ─────────────
    void BuildPrimitiveScene()
    {
        // Zemin
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ground.GetComponent<Renderer>().material.color = new Color(0.4f, 0.6f, 0.3f);

        // NavMesh Surface — Runtime NavMesh gerekiyor
        // Eğer Unity NavMesh Surface bileşeni yüklüyse ekle
        // var surface = ground.AddComponent<NavMeshSurface>();
        // surface.BuildNavMesh();

        // Ağaçlar
        for(int i = 0; i < 6; i++)
        {
            var tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tree.name = "Tree_" + i;
            tree.tag  = "Tree";
            tree.transform.position = new Vector3(
                Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
            tree.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
            tree.GetComponent<Renderer>().material.color = new Color(0.3f, 0.5f, 0.2f);
        }

        // Balık noktaları
        for(int i = 0; i < 2; i++)
        {
            var spot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spot.name = "FishingSpot_" + i;
            spot.tag  = "FishingSpot";
            spot.transform.position = new Vector3(Random.Range(-8f, 8f), 0.1f, Random.Range(-8f, 8f));
            spot.transform.localScale = Vector3.one * 0.5f;
            spot.GetComponent<Renderer>().material.color = new Color(0.2f, 0.5f, 0.9f);
        }

        // İnşaat alanı
        var buildSite = GameObject.CreatePrimitive(PrimitiveType.Cube);
        buildSite.name = "BuildSite";
        buildSite.tag  = "BuildSite";
        buildSite.transform.position = new Vector3(3f, 0.25f, 3f);
        buildSite.transform.localScale = new Vector3(1f, 0.5f, 1f);
        buildSite.GetComponent<Renderer>().material.color = new Color(0.8f, 0.7f, 0.5f);

        // Minyonları oluştur
        SpawnMinyon("Ahmet", false, new Vector3(-2f, 0f, -2f), new[]{"Sinir","Oduncu"});
        SpawnMinyon("Ayşe",  true,  new Vector3(2f,  0f, -2f), new[]{"Sosyal","Avcı"});
    }

    void BuildPrefabScene()
    {
        if(treePrefab != null)
            for(int i = 0; i < 6; i++)
                Instantiate(treePrefab, new Vector3(
                    Random.Range(-12f,12f), 0f, Random.Range(-12f,12f)), Quaternion.identity);

        if(fishingSpotPrefab != null)
            for(int i = 0; i < 2; i++)
                Instantiate(fishingSpotPrefab, new Vector3(
                    Random.Range(-8f,8f), 0f, Random.Range(-8f,8f)), Quaternion.identity);

        SpawnMinyon("Ahmet", false, new Vector3(-2f, 0f, -2f), new[]{"Sinir","Oduncu"});
        SpawnMinyon("Ayşe",  true,  new Vector3(2f,  0f, -2f), new[]{"Sosyal","Avcı"});
    }

    // ── MİNYON SPAWN ─────────────────────────────────────
    void SpawnMinyon(string name, bool female, Vector3 pos, string[] cardList)
    {
        GameObject go;

        if(minionPrefab != null)
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

        go.name = name;

        var ai = go.GetComponent<MinionAI>();
        ai.stats.Initialize(1, female, name);

        var cs = go.GetComponent<CardSystem>();
        foreach(var card in cardList) cs.AddCard(card);

        // Renk — kadın/erkek farkı
        var r = go.GetComponentInChildren<Renderer>();
        if(r != null)
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
        if(!go.GetComponent<NavMeshAgent>())          go.AddComponent<NavMeshAgent>();
        if(!go.GetComponent<MinionAI>())              go.AddComponent<MinionAI>();
        if(!go.GetComponent<NeedsSystem>())           go.AddComponent<NeedsSystem>();
        if(!go.GetComponent<EmotionSystem>())         go.AddComponent<EmotionSystem>();
        if(!go.GetComponent<MemorySystem>())          go.AddComponent<MemorySystem>();
        if(!go.GetComponent<CardSystem>())            go.AddComponent<CardSystem>();
        if(!go.GetComponent<SocialNetwork>())         go.AddComponent<SocialNetwork>();
        if(!go.GetComponent<CascadeSystem>())         go.AddComponent<CascadeSystem>();
        if(!go.GetComponent<RandomEventSystem>())     go.AddComponent<RandomEventSystem>();
    }

    T EnsureManager<T>(string goName) where T : MonoBehaviour
    {
        var existing = FindFirstObjectByType<T>();
        if(existing != null) return existing;

        var go = new GameObject(goName);
        return go.AddComponent<T>();
    }
}
