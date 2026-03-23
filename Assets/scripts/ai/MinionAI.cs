using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// ============================================================
// MinionAI.cs (DÜZELTİLMİŞ, TAM ENTEGRE VERSİYON)
// Ana minyon sınıfı — behaviour tree, hareket, iş sistemi
// Duygular, hafıza ve sosyal ağ tamamen korunmuştur.
// ============================================================
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NeedsSystem))]
[RequireComponent(typeof(EmotionSystem))]
[RequireComponent(typeof(MemorySystem))]
[RequireComponent(typeof(CardSystem))]
[RequireComponent(typeof(SocialNetwork))]
[RequireComponent(typeof(CascadeSystem))]
[RequireComponent(typeof(RandomEventSystem))]
public class MinionAI : MonoBehaviour
{
    [Header("Veri")]
    public MinionStats stats = new MinionStats();
    private Vector3 lastMoveTarget = Vector3.zero;

    [Header("Durum")]
    public string currentState = "Idle";
    public string lastState = "Idle";
    public MinionAI currentAttacker = null;
    public GameObject shelterPrefab;
    // Bileşenler
    NavMeshAgent agent;
    NeedsSystem needs;
    EmotionSystem emotions;
    MemorySystem memory;
    CardSystem cards;
    SocialNetwork social;
    CascadeSystem cascade;

    // Behaviour tree tick hızı
    float btTimer = 0f;
    const float BT_TICK = 2.0f; // 2 saniyede bir karar verir (Performans ve kararlılık için ideal)
    private static Dictionary<GameObject, int> houseOccupancy = new Dictionary<GameObject, int>();
    // Hedefler
    GameObject currentTarget;
    Vector3 wanderTarget;
    float stateTimer = 0f;

    // Renk sistemi
    Renderer minionRenderer;
    Color baseColor;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        needs = GetComponent<NeedsSystem>();
        emotions = GetComponent<EmotionSystem>();
        memory = GetComponent<MemorySystem>();
        cards = GetComponent<CardSystem>();
        social = GetComponent<SocialNetwork>();
        cascade = GetComponent<CascadeSystem>();

        minionRenderer = GetComponentInChildren<Renderer>();
        if (minionRenderer != null)
            baseColor = new Color(0.85f, 0.75f, 0.65f);

        if (cards.HasCard("Hızlı")) agent.speed *= 1.5f;

        InvokeRepeating(nameof(UpdateColor), 0f, 0.5f);
        InvokeRepeating(nameof(AgeTick), 0f, 1f);
    }

    void Update()
    {
        if (!TimeManager.Instance.isNight && minionRenderer != null && !minionRenderer.enabled)
        {
            minionRenderer.enabled = true; // Geri görünür ol

            // Eğer bir evin içindeysek o evi boşaltalım (Kapasiteyi düşür)
            var myHome = FindNearest("Shelter");
            if (myHome != null && houseOccupancy.ContainsKey(myHome) && houseOccupancy[myHome] > 0)
            {
                houseOccupancy[myHome]--;
            }
        }
        if (!stats.isAlive) return;

        lastState = currentState;
        stateTimer += Time.deltaTime;

        btTimer += Time.deltaTime;
        if (btTimer >= BT_TICK)
        {
            btTimer = 0f;
            RunBehaviourTree();
        }
    }

    // ── 0. BEHAVIOUR TREE (KARAR AĞACI HİYERARŞİSİ) ─────────
    void RunBehaviourTree()
    {
        // 1. ÖLÜM VE KRİTİK İHTİYAÇLAR (Her zaman en üstte)
        if (CheckDeath()) return;
        if (RunEmergency()) { ClearCurrentTask(); return; }

        // 2. SOSYAL VE DUYGUSAL DURUM (Eğer çok mutsuzsa veya yalnızsa işi reddetsin)
        if (emotions.CanRefuseWork() && Random.value < 0.3f) { ClearCurrentTask(); SetState("DepressedIdle"); return; }

        // 3. ÜREME VE SOSYALLEŞME (Eğer üreme vakti geldiyse veya çok yalnızsa işi bırakıp birine gitsin)
        if (CanMate()) { ClearCurrentTask(); if (TryMate()) return; }
        if (needs.loneliness > 70f || needs.boredom > 75f) { ClearCurrentTask(); if (RunSocialNeeds()) return; }

        // 4. GÖREV ODAKLANMASI (Eğer bir işe başladıysan ve yukarıdaki krizler yoksa işi bitir)
        if (currentTarget != null && IsWorkingState(currentState))
        {
            ContinueCurrentTask();
            return;
        }

        // 5. TEMEL HAYATTA KALMA VE İŞLER
        if (RunBasicSurvival()) return;
        if (RunCardBehaviors()) return;
        if (RunNightRoutine()) return;

        // 6. HİÇBİR ŞEY YOKSA
        RunDefault();
    }

    // ── YENİ: GÖREV KİLİDİ ──────────────────────────────────
    void ContinueCurrentTask()
    {
        if (currentTarget == null || !currentTarget.activeInHierarchy)
        {
            ClearCurrentTask();
            SetState("Idle");
            return;
        }

        // İNŞAAT ÖZEL KONTROLÜ: Odun bittiyse inşaatı bırakıp oduna gitmesi lazım
        if (currentState == "Building" && ResourceManager.Instance.woodCount < 10f)
        {
            ClearCurrentTask();
            return;
        }

        float dist = GetDistance2D(transform.position, currentTarget.transform.position);

        if (dist > 3f)
        {
            MoveTo(currentTarget.transform.position);
        }
        else
        {
            agent.isStopped = true;
            ExecuteWorkLogic(); // Mevcut işi yap (Odun kes, balık tut veya ev yap)
        }
    }
    bool IsWorkingState(string state)
    {
        return state == "ChoppingWood" || state == "Fishing" || state == "Building";
    }

    // Hata CS0103: 'ExecuteWorkLogic' çözümüdür
    void ExecuteWorkLogic()
    {
        MinionFeedback fb = GetComponentInChildren<MinionFeedback>();
        if (currentState == "ChoppingWood")
        {
            float amount = stats.workSpeed;
            if (cards.HasCard("Sinir") && emotions.GetI("Angry") > 0.5f) amount *= 1.5f;
            ResourceManager.Instance.Add("wood", amount * Time.deltaTime * 2f);
            if (fb != null) fb.TriggerEmoji("Wood"); // Balta emojisi tetikle
        }
        else if (currentState == "Fishing")
        {
            float chance = InventionManager.Instance.IsDiscovered("FishingNet") ? 0.8f : 0.5f;
            if (Random.value < (chance * Time.deltaTime))
            {
                ResourceManager.Instance.Add("food", 2f);
                needs.Change("hunger", -5f);
                if (fb != null) fb.TriggerEmoji("Fish");
            }
        }
        else if (currentState == "Building")
        {
            FinishBuilding(); // Bu metodu aşağıda tanımladık
        }
    }
    void FinishBuilding()
    {
        var site = currentTarget; // Mevcut hedefimiz inşaat alanı
        if (site == null) return;

        stats.buildingsConstructed++;
        ResourceManager.Instance.Spend("wood", 10f);

        // İnşaat küpünün yerine gerçek ev prefabını koy
        if (shelterPrefab != null)
        {
            GameObject realHome = Instantiate(shelterPrefab, site.transform.position, site.transform.rotation);
            realHome.tag = "Shelter";
            realHome.name = "Shelter_" + stats.name;
        }

        // Küpü (BuildSite) yok et
        Destroy(site);

        NotificationManager.Instance.Show(stats.name + " bir ev inşa etti!", NotificationType.Social);

        // İş bitti, hedefi temizle
        ClearCurrentTask();
        SetState("Idle");
    }
    // ── YENİ: TEMEL HAYATTA KALMA MANTIĞI ───────────────────
    bool RunBasicSurvival()
    {
        if (needs.hunger > 50f)
        {
            if (GoFish()) return true;
        }

        if (!HasShelter())
        {
            if (ResourceManager.Instance.woodCount >= 10f)
            {
                if (Build()) return true;
            }
            else
            {
                if (ChopWood()) return true;
            }
        }

        if (ResourceManager.Instance.foodCount < 10f)
        {
            if (GoFish()) return true;
        }

        if (ResourceManager.Instance.woodCount < 5f)
        {
            if (ChopWood()) return true;
        }

        return false;
    }

    // ── 1. ÖLÜM KONTROLÜ ────────────────────────────────────
    bool CheckDeath()
    {
        if (needs.hunger >= 100f || stats.health <= 0f || stats.age >= stats.maxAge)
        {
            Die();
            return true;
        }
        return false;
    }

    void Die()
    {
        stats.isAlive = false;
        SetState("Dead");
        agent.enabled = false;

        EssenceManager.Instance.AddEssence(20f);
        GenerationManager.Instance.OnMinionDied(this);
        NotificationManager.Instance.Show(stats.name + " hayatını kaybetti.", NotificationType.Death);
        EventLogSystem.Instance.AddEntry("Death", stats.name, NotificationType.Death, stats.generation);

        Destroy(gameObject, 3f);
    }

    // ── 2. ACİL DURUMLAR ────────────────────────────────────
    bool RunEmergency()
    {
        if (emotions.ShouldFlee()) { FleeFromDanger(); return true; }
        if (needs.hunger > 85f) { EmergencyFoodSearch(); return true; }
        if (needs.coldness > 80f) { FindHeatSource(); return true; }
        if (stats.health < 20f && stats.isSick) { FindHealer(); return true; }
        return false;
    }

    void FleeFromDanger()
    {
        SetState("Fleeing");
        Vector3 fleeDir = (transform.position - GetDangerCenter()).normalized;
        MoveTo(transform.position + fleeDir * 8f);
    }

    void EmergencyFoodSearch()
    {
        SetState("EmergencyEat");
        var food = FindNearest("Food");
        if (food != null) MoveTo(food.transform.position);
        else GoFish();
    }

    void FindHeatSource()
    {
        SetState("WarmingUp");
        var fire = FindNearest("Campfire");
        if (fire != null) MoveTo(fire.transform.position);
    }

    void FindHealer()
    {
        SetState("SeekingHealer");
        foreach (var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if (m == this || !m.stats.isAlive) continue;
            if (m.GetComponent<CardSystem>()?.HasCard("Şaman") ?? false)
            {
                MoveTo(m.transform.position);
                return;
            }
        }
    }

    // ── 3. İŞ VE KAYNAK FONKSİYONLARI (FİZİKSEL DÜZELTMELER) ─
    bool ChopWood()
    {
        var tree = FindNearest("Tree");
        if (tree == null) return false;

        SetState("ChoppingWood");
        MoveTo(tree.transform.position);

        // DÜZELTME 3: Y-Ekseni Bug'ı Çözümü (Sadece yatay mesafe ölçülür)
        if (GetDistance2D(transform.position, tree.transform.position) < 4.5f)
        {
            float amount = stats.workSpeed;
            if (cards.HasCard("Sinir") && emotions.GetI("Angry") > 0.5f) amount *= 1.5f;

            // DÜZELTME 4: Time.deltaTime yerine direkt BT_TICK kullanıldı
            ResourceManager.Instance.Add("wood", amount * BT_TICK);
        }
        return true;
    }

    bool GoFish()
    {
        var spot = FindNearest("FishingSpot");
        if (spot == null) return false;

        SetState("Fishing");
        MoveTo(spot.transform.position);

        if (GetDistance2D(transform.position, spot.transform.position) < 4.5f)
        {
            float chance = InventionManager.Instance.IsDiscovered("FishingNet") ? 0.8f : 0.4f;
            if (Random.value < chance)
            {
                ResourceManager.Instance.Add("food", 2f);
                needs.Change("hunger", -5f);
            }
        }
        return true;
    }

    bool Build()
    {
        var site = FindNearest("BuildSite");
        if (site == null) return false;

        SetState("Building");
        MoveTo(site.transform.position);

        // 2D mesafe kontrolü (Ağaç boyu/Ev boyu bug'ına girmemesi için)
        if (GetDistance2D(transform.position, site.transform.position) < 4.5f)
        {
            stats.buildingsConstructed++;
            ResourceManager.Instance.Spend("wood", 10f); // 10 Odun harca

            // --- PREFAB SPAWN ETME ---
            if (shelterPrefab != null)
            {
                // İnşaat küpünün tam yerine yeni evi dikiyoruz
                GameObject realHome = Instantiate(shelterPrefab, site.transform.position, site.transform.rotation);

                // Yeni objenin Tag'ini "Shelter" yapıyoruz ki HasShelter() onu bulabilsin
                realHome.tag = "Shelter";
                realHome.name = "Shelter_" + stats.name;
            }

            // İnşaat bittiğine göre o geçici küpü (BuildSite) yok et
            Destroy(site);

            NotificationManager.Instance.Show(stats.name + " bir ev inşa etti!", NotificationType.Social);

            // Görevi temizle ki minyon orada mal gibi dikilmesin
            ClearCurrentTask();
        }
        return true;
    }

    // ── 4. KART VE MESLEKLER ────────────────────────────────
    bool RunCardBehaviors()
    {
        if (cards.HasCard("Oduncu") && ResourceManager.Instance.woodCount < ResourceManager.Instance.maxWood)
        {
            if (ChopWood()) return true;
        }

        if (cards.HasCard("Avcı") && ResourceManager.Instance.foodCount < ResourceManager.Instance.maxFood)
        {
            if (GoFish()) return true;
        }

        if (cards.HasCard("İnşaatçı") && ResourceManager.Instance.woodCount >= 10f)
        {
            if (Build()) return true;
        }

        if (cards.HasCard("Şaman")) { if (HealNearby()) return true; }
        if (cards.HasCard("Tembel") && Random.value < 0.2f) { Loaf(); return true; }
        if (cards.HasCard("Deli") && Random.value < stats.randomBehaviorChance) { DoCrazy(); return true; }
        if (cards.HasCard("Mızmız") && Random.value < 0.1f) Complain();

        return false;
    }

    bool HealNearby()
    {
        foreach (var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if (m == this || !m.stats.isAlive) continue;
            if (!m.stats.isSick && !m.stats.isInjured) continue;

            SetState("Healing");
            MoveTo(m.transform.position);

            if (GetDistance2D(transform.position, m.transform.position) < 3f)
            {
                bool mad = cards.HasCard("Deli");
                bool smart = cards.HasCard("Zeka");
                float s = smart ? 0.8f : mad ? 0.3f : 0.5f;

                if (Random.value < s)
                {
                    m.stats.health += 30f;
                    m.stats.isSick = false;
                    m.stats.isInjured = false;
                    memory.RecordEvent("HealedSomeone", m.stats.name, 0.5f);
                    social.UpdateRelationship(m.stats.name, 10f, "Healed");
                }
                else if (mad) m.stats.health -= 10f;
            }
            return true;
        }
        return false;
    }

    void Loaf() { SetState("Loafing"); agent.isStopped = true; needs.Change("fatigue", -1f); }
    void DoCrazy() { SetState("Wandering"); MoveTo(transform.position + Random.insideUnitSphere * 4f); }
    void Complain() { NotificationManager.Instance.Show(stats.name + " söyleniyor...", NotificationType.Comedy); }

    // ── 5. DUYGU TEPKİLERİ ──────────────────────────────────
    bool RunEmotionalReactions()
    {
        if (emotions.IsPlottingRevenge())
        {
            var enemy = FindNearestEnemy();
            if (enemy != null) { ExecuteRevenge(enemy); return true; }
        }

        if (emotions.GetI("Humiliated") > 0.6f) { MoveToEdge(); return true; }
        if (emotions.GetI("Depressed") > 0.5f && HasShelter()) { StayHome(); return true; }

        if (emotions.GetI("Euphoric") > 0.7f && cards.HasCard("Sosyal")) { SpreadHappiness(); return true; }

        return false;
    }

    void ExecuteRevenge(MinionAI target)
    {
        SetState("Revenge");
        MoveTo(target.transform.position);
        if (GetDistance2D(transform.position, target.transform.position) < 2.5f)
        {
            target.stats.health -= 15f;
            memory.RecordEvent("RevengeExecuted", target.stats.name, 0.7f);
            emotions.SetEmotion("Proud", 0.5f);
            cascade.TriggerCascade("Fight", target.stats.name, 0.5f);
        }
    }

    void MoveToEdge() { SetState("Isolated"); MoveTo(transform.position + Random.insideUnitSphere.normalized * 15f); }
    void StayHome() { var home = FindNearest("Shelter"); if (home != null) { MoveTo(home.transform.position); agent.isStopped = true; } }
    void SpreadHappiness()
    {
        SetState("Socializing");
        var nearest = FindNearestAlive();
        if (nearest != null)
        {
            MoveTo(nearest.transform.position);
            if (GetDistance2D(transform.position, nearest.transform.position) < 3f)
                nearest.GetComponent<EmotionSystem>()?.SetEmotion("Happy", 0.2f);
        }
    }

    // ── 6. GECE RUTİNİ ──────────────────────────────────────
    bool RunNightRoutine()
    {
        if (!TimeManager.Instance.isNight) return false;
        if (cards.HasCard("ErkenKalkan")) return false;

        if (HasShelter()) GoHomeAndSleep();
        else SleepOutside();
        return true;
    }

    void GoHomeAndSleep()
    {
        SetState("Sleeping");
        var shelters = GameObject.FindGameObjectsWithTag("Shelter");
        GameObject targetHome = null;

        // En yakın ve müsait (en fazla 2 kişi) evi bul
        foreach (var s in shelters)
        {
            if (!houseOccupancy.ContainsKey(s)) houseOccupancy[s] = 0;

            if (houseOccupancy[s] < 2) // Kapasite kontrolü (Max 2)
            {
                targetHome = s;
                break;
            }
        }

        if (targetHome != null)
        {
            MoveTo(targetHome.transform.position);
            if (GetDistance2D(transform.position, targetHome.transform.position) < 1.5f)
            {
                agent.isStopped = true;
                if (minionRenderer != null && minionRenderer.enabled)
                {
                    minionRenderer.enabled = false; // İçeri girdi, gizle
                    houseOccupancy[targetHome]++; // Evdeki sayıyı artır
                }
            }
        }
    }

    void SleepOutside()
    {
        SetState("SleepingOutside");
        agent.isStopped = true;
        needs.Change("coldness", 2f);
    }

    // ── 7. SOSYAL İHTİYAÇLAR (Ve Çiftleşme) ─────────────────
    bool RunSocialNeeds()
    {
        if (needs.loneliness > 70f && stats.friends.Count > 0) { GoToFriend(); return true; }
        if (needs.boredom > 75f && cards.HasCard("Maceracı")) { Explore(); return true; }
        if (needs.boredom > 70f && cards.HasCard("Rekabetçi")) { FindRival(); return true; }

        if (CanMate()) { if (TryMate()) return true; }

        return false;
    }

    void GoToFriend()
    {
        SetState("GoingToFriend");
        foreach (var f in stats.friends)
        {
            if (f != null && f.stats.isAlive)
            {
                MoveTo(f.transform.position);
                if (GetDistance2D(transform.position, f.transform.position) < 3f)
                {
                    needs.Change("loneliness", -15f);
                    social.UpdateRelationship(f.stats.name, 2f, "Socializing");
                }
                return;
            }
        }
    }

    void Explore() { SetState("Exploring"); MoveTo(transform.position + Random.insideUnitSphere.normalized * 15f); }

    void FindRival()
    {
        SetState("FindingRival");
        var rival = FindNearestAlive();
        if (rival != null)
        {
            MoveTo(rival.transform.position);
            if (GetDistance2D(transform.position, rival.transform.position) < 3f) needs.Change("boredom", -20f);
        }
    }

    bool CanMate()
    {
        if (stats.isMarried || stats.health < 50f) return false;
        if (Time.time - stats.lastMatingTime < stats.matingCooldown) return false;
        if (needs.loneliness < 40f && needs.hunger > 60f) return false;
        return true;
    }

    bool TryMate()
    {
        foreach (var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if (m == this || !m.stats.isAlive || m.stats.isFemale == stats.isFemale || m.stats.isMarried) continue;
            if (social.GetRelationship(m.stats.name) < 55f) continue;

            SetState("LookingForMate");
            MoveTo(m.transform.position);

            if (GetDistance2D(transform.position, m.transform.position) < 3f)
            {
                stats.lastMatingTime = Time.time;
                m.stats.lastMatingTime = Time.time;
                if (Random.value < 0.4f) GenerationManager.Instance.TryBirth(this, m);
            }
            return true;
        }
        return false;
    }

    // ── 8. DEFAULT (Boşta Kalma) ────────────────────────────
    void RunDefault() { Idle(); }

    void Idle()
    {
        SetState("Idle");
        if (!agent.hasPath || agent.remainingDistance < 0.5f) agent.isStopped = true;
        if (cards.HasCard("Zeka") && memory.GetThreshold("idle_long") > 180f) SetState("Thinking");
    }

    // ── RENK VE YAŞLANMA ────────────────────────────────────
    void UpdateColor()
    {
        if (minionRenderer == null || !stats.isAlive) return;

        Color c = baseColor;

        // --- YENİ: SAĞLIK VE HASTALIK GERİ BİLDİRİMİ ---
        if (stats.isSick)
        {
            c = Color.green; // Zehirlenince yeşil olsun
        }
        else if (stats.health < 30f)
        {
            c = Color.black; // Ölmek üzereyken kararsın
        }
        // ----------------------------------------------

        // Mevcut duygu renkleri (Üzerine ekler)
        switch (emotions.Dominant())
        {
            case "Angry": c = Color.Lerp(c, Color.red, emotions.GetI("Angry") * 0.4f); break;
            case "Happy": c = Color.Lerp(c, Color.yellow, emotions.GetI("Happy") * 0.2f); break;
            case "Terrified": c = Color.white; break;
        }

        minionRenderer.material.color = c;
    }

    void AgeTick()
    {
        if (!stats.isAlive) return;
        stats.age += 1f / 60f;
        if (stats.age > stats.maxAge * 0.7f) agent.speed = Mathf.Max(0.5f, agent.speed - 0.001f);
    }

    // ── FİZİKSEL YARDIMCILAR (KUSURSUZ HAREKET SİSTEMİ) ─────
    public void SetState(string newState)
    {
        if (currentState != newState)
        {
            stateTimer = 0f;
            currentState = newState;

            // KAFADA DURUM YAZDIRMA
            MinionFeedback fb = GetComponentInChildren<MinionFeedback>();
            if (fb != null)
            {
                // Durum isimlerini daha "insancıl" yapalım
                string displayName = newState;
                if (newState == "ChoppingWood") displayName = "Odun Kesiyor";
                if (newState == "Fishing") displayName = "Balık Tutuyor";
                if (newState == "Building") displayName = "İnşaat Yapıyor";
                if (newState == "Sleeping") displayName = "Zzz...";

                fb.ShowText(displayName, Color.white);
            }
        }
        if (agent.isActiveAndEnabled) agent.isStopped = false;
    }

    void MoveTo(Vector3 pos)
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
        agent.isStopped = false;

        // DÜZELTME 6: NavMesh Kekemeliği (Stuttering) Çözümü
        if (Vector3.Distance(lastMoveTarget, pos) > 1.5f)
        {
            lastMoveTarget = pos;
            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
                agent.SetDestination(pos);
        }
    }

    // Hayat Kurtaran 2D Mesafe Ölçer (Yükseklikleri yok sayar)
    float GetDistance2D(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }

    bool HasShelter()
    {
        var shelter = FindNearest("Shelter");
        return shelter != null && GetDistance2D(transform.position, shelter.transform.position) < 25f;
    }

    Vector3 GetDangerCenter()
    {
        var dangers = Physics.OverlapSphere(transform.position, 15f, LayerMask.GetMask("Danger"));
        if (dangers.Length == 0) return transform.position;
        Vector3 center = Vector3.zero;
        foreach (var d in dangers) center += d.transform.position;
        return center / dangers.Length;
    }

    GameObject FindNearest(string tag)
    {
        GameObject[] objs;
        try { objs = GameObject.FindGameObjectsWithTag(tag); } catch { return null; }
        GameObject best = null; float min = Mathf.Infinity;
        foreach (var o in objs)
        {
            float d = GetDistance2D(transform.position, o.transform.position);
            if (d < min) { min = d; best = o; }
        }
        return best;
    }

    MinionAI FindNearestAlive()
    {
        MinionAI best = null; float min = Mathf.Infinity;
        foreach (var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if (m == this || !m.stats.isAlive) continue;
            float d = GetDistance2D(transform.position, m.transform.position);
            if (d < min) { min = d; best = m; }
        }
        return best;
    }
    void ClearCurrentTask()
    {
        currentTarget = null; // Mevcut hedefi sıfırlar, AI'nın kafasını boşaltır
    }
    MinionAI FindNearestEnemy()
    {
        foreach (var e in stats.enemies) if (e != null && e.stats.isAlive) return e;
        return null;
    }
}