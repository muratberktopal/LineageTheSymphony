using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
// ============================================================
// MinionAI.cs
// Ana minyon sınıfı — behaviour tree, hareket, iş sistemi
// Her minyona eklenir. NavMeshAgent gerektirir.
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
    public string lastState    = "Idle";
    public MinionAI currentAttacker = null;

    // Bileşenler
    NavMeshAgent    agent;
    NeedsSystem     needs;
    EmotionSystem   emotions;
    MemorySystem    memory;
    CardSystem      cards;
    SocialNetwork   social;
    CascadeSystem   cascade;

    // Behaviour tree tick hızı
    float btTimer    = 0f;
    const float BT_TICK = 2.0f;

    // Hedefler
    GameObject currentTarget;
    Vector3    wanderTarget;
    float      stateTimer = 0f;

    // Renk sistemi
    Renderer minionRenderer;
    Color    baseColor;

    void Start()
    {

        agent    = GetComponent<NavMeshAgent>();
        needs    = GetComponent<NeedsSystem>();
        emotions = GetComponent<EmotionSystem>();
        memory   = GetComponent<MemorySystem>();
        cards    = GetComponent<CardSystem>();
        social   = GetComponent<SocialNetwork>();
        cascade  = GetComponent<CascadeSystem>();

        minionRenderer = GetComponentInChildren<Renderer>();
        if(minionRenderer != null)
            baseColor = new Color(0.85f, 0.75f, 0.65f);

        // Hız kartı
        if(cards.HasCard("Hızlı"))
            agent.speed *= 1.5f;

        Debug.Log($"[{stats.name}] Agent isOnNavMesh: {agent.isOnNavMesh}, enabled: {agent.enabled}");
        Invoke(nameof(TestMove), 1f);


        InvokeRepeating(nameof(UpdateColor), 0f, 0.5f);
        InvokeRepeating(nameof(AgeTick),     0f, 1f);
    
    }

    void TestMove()
    {
        Debug.Log($"[{stats.name}] TestMove çağrıldı");
        MoveTo(transform.position + Vector3.forward * 3f);
    }



    void Update()
    {
        if(!stats.isAlive) return;

        lastState = currentState;
        stateTimer += Time.deltaTime;

        btTimer += Time.deltaTime;
        if(btTimer >= BT_TICK)
        {
            btTimer = 0f;
            RunBehaviourTree();
        }
    }

    // ── BEHAVIOUR TREE ────────────────────────────────────
    // ── BEHAVIOUR TREE ────────────────────────────────────
    void RunBehaviourTree()
    {
        if (emotions.CanRefuseWork() && Random.value < 0.3f)
        {
            ClearCurrentTask();
            SetState("DepressedIdle");
            return;
        }

        // 1. ÖLÜM VE ACİL DURUM
        if (CheckDeath()) return;
        if (RunEmergency())
        {
            ClearCurrentTask();
            return;
        }

        // 2. TEMEL HAYATTA KALMA (Yemek Yeme Mekaniği Eklendi)
        if (RunBasicSurvival()) return;

        // 3. ÜREME ÖNCELİĞİ (Oyun başı çoğalmayı garanti eder!)
        // Eğer üremeye hazırsa, her şeyi bırakıp eşine koşar.
        if (CanMate())
        {
            ClearCurrentTask();
            if (TryMate()) return;
        }

        // 4. GÖREV ODAKLANMASI (İş yapıyorsa bozma)
        if (currentTarget != null && IsWorkingState(currentState))
        {
            ContinueCurrentTask();
            return;
        }

        // 5. YENİ İŞ BULMA VEYA KART DAVRANIŞLARI
        if (RunCardBehaviors()) return;
        if (RunEmotionalReactions()) return;
        if (RunNightRoutine()) return;
        if (RunSocialNeeds()) return;

        // 6. DEFAULT (Hiçbir şey yoksa yapılacaklar)
        RunDefault();
    }


    // chop Yardımcı Fonksiyonlar (MinionAI.cs içine ekle)
    void ClearCurrentTask()
    {
        currentTarget = null;
    }

    bool IsWorkingState(string state)
    {
        return state == "ChoppingWood" || state == "Fishing" || state == "Building";
    }






    bool RunBasicSurvival()
    {
        // 1. Kendi karnı açsa
        if (needs.hunger > 40f)
        {
            // Köyde yemek varsa hemen ye ve rahatla (Sürekli balık tutmayı engeller)
            if (ResourceManager.Instance.foodCount >= 2f)
            {
                ResourceManager.Instance.Spend("food", 2f);
                needs.Change("hunger", -40f); // Karnı doydu
                // İşe devam etmesi için return true DEMİYORUZ.
            }
            // Köyde yemek yoksa ve zaten balık tutmuyorsa, acilen balığa git
            else if (currentState != "Fishing")
            {
                ClearCurrentTask();
                if (GoFish()) return true;
            }
        }

        // 2. Köyün yiyecek stoğu alarm veriyorsa (Dayanışma)
        if (ResourceManager.Instance.foodCount < 10f && currentState != "Fishing")
        {
            ClearCurrentTask();
            if (GoFish()) return true;
        }

        return false;
    }

    // ── 1. ÖLÜM ───────────────────────────────────────────
    bool CheckDeath()
    {
        if(needs.hunger >= 100f || stats.health <= 0f || stats.age >= stats.maxAge)
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

    // ── 2. ACİL DURUM ─────────────────────────────────────
    bool RunEmergency()
    {
        // Kaç
        if(emotions.ShouldFlee())
        {
            FleeFromDanger();
            return true;
        }

        // Kritik açlık
        if(needs.hunger > 85f)
        {
            EmergencyFoodSearch();
            return true;
        }

        // Kritik soğuk
        if(needs.coldness > 80f)
        {
            FindHeatSource();
            return true;
        }

        // Çok hasta
        if(stats.health < 20f && stats.isSick)
        {
            FindHealer();
            return true;
        }

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
        if(food != null) MoveTo(food.transform.position);
        else FindNearest("FishingSpot");
    }

    void FindHeatSource()
    {
        SetState("WarmingUp");
        var fire = FindNearest("Campfire");
        if(fire != null) MoveTo(fire.transform.position);
    }

    void FindHealer()
    {
        SetState("SeekingHealer");
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(m == this || !m.stats.isAlive) continue;
            if(m.GetComponent<CardSystem>()?.HasCard("Şaman") ?? false)
            {
                MoveTo(m.transform.position);
                return;
            }
        }
    }

    // ── 3. DUYGU TEPKİLERİ ────────────────────────────────
    bool RunEmotionalReactions()
    {
        // İntikam planı aktif ve düşman bulunabilir
        if(emotions.IsPlottingRevenge())
        {
            var enemy = FindNearestEnemy();
            if(enemy != null)
            {
                ExecuteRevenge(enemy);
                return true;
            }
        }

        // Aşırı aşağılanma — kenara çekil
        if(emotions.GetI("Humiliated") > 0.6f)
        {
            MoveToEdge();
            return true;
        }

        // Depresyon — evde kal
        if(emotions.GetI("Depressed") > 0.5f && HasShelter())
        {
            SetState("DepressedIdle");
            StayHome();
            return true;
        }

        // Çok mutlu — çevreyi etkile
        if(emotions.GetI("Euphoric") > 0.7f && cards.HasCard("Sosyal"))
        {
            SpreadHappiness();
            return true;
        }

        return false;
    }

    void ExecuteRevenge(MinionAI target)
    {
        SetState("Revenge");
        MoveTo(target.transform.position);
        if(Vector3.Distance(transform.position, target.transform.position) < 1.5f)
        {
            target.stats.health -= 15f;
            memory.RecordEvent("RevengeExecuted", target.stats.name, 0.7f);
            emotions.SetEmotion("Proud",   0.5f);
            emotions.SetEmotion("Guilty",  0.3f);
            cascade.TriggerCascade("Fight", target.stats.name, 0.5f);
        }
    }
    //runbehaviour
    void MoveToEdge()
    {
        SetState("Isolated");
        // Haritanın uzak köşesine git
        Vector3 edge = transform.position + Random.insideUnitSphere.normalized * 15f;
        edge.y = transform.position.y;
        MoveTo(edge);
    }

    void SpreadHappiness()
    {
        SetState("Socializing");
        var nearest = FindNearestAlive();
        if(nearest != null)
        {
            MoveTo(nearest.transform.position);
            if(Vector3.Distance(transform.position, nearest.transform.position) < 2f)
                nearest.GetComponent<EmotionSystem>()?.SetEmotion("Happy", 0.2f);
        }
    }

    // ── 4. GECE RUTINI ─────────────────────────────────────
    bool RunNightRoutine()
    {
        if(!TimeManager.Instance.isNight) return false;

        // Erken Kalkan gece çalışır
        if(cards.HasCard("ErkenKalkan"))
        {
            RunDefault(); // Gece çalışmaya devam
            return false;
        }

        // Ağır Uyuyan geç saate kadar çalışır
        if(cards.HasCard("AğırUyuyan") && TimeManager.Instance.currentTime < 0.85f)
        {
            RunDefault();
            return false;
        }

        // Rüya görür — uyurgezerlik ihtimali
        if(cards.HasCard("RüyaGörür") && Random.value < 0.05f)
        {
            Sleepwalk();
            return true;
        }

        if(HasShelter()) GoHomeAndSleep();
        else             SleepOutside();
        return true;
    }

    void Sleepwalk()
    {
        SetState("Sleepwalking");
        Vector3 random = transform.position + Random.insideUnitSphere * 5f;
        random.y = transform.position.y;
        MoveTo(random);
    }

    void GoHomeAndSleep()
    {
        SetState("Sleeping");
        var home = FindNearest("Shelter");
        if(home != null) MoveTo(home.transform.position);
    }

    void SleepOutside()
    {
        SetState("SleepingOutside");
        agent.isStopped = true;
        needs.Change("coldness", 2f); // dışarıda soğur
    }

    void StayHome()
    {
        var home = FindNearest("Shelter");
        if(home != null) MoveTo(home.transform.position);
        agent.isStopped = true;
    }

    // ── 5. SOSYAL İHTİYAÇLAR ──────────────────────────────
    bool RunSocialNeeds()
    {
        // Yalnızlık — arkadaş bul
        if(needs.loneliness > 70f && stats.friends.Count > 0)
        {
            GoToFriend();
            return true;
        }

        // Sıkılma + Maceracı — keşfet
        if(needs.boredom > 75f && cards.HasCard("Maceracı"))
        {
            Explore();
            return true;
        }

        // Sıkılma + Rekabetçi — rakip bul
        if(needs.boredom > 70f && cards.HasCard("Rekabetçi"))
        {
            FindRival();
            return true;
        }


        return false;
    }

    void GoToFriend()
    {
        SetState("GoingToFriend");
        foreach(var f in stats.friends)
        {
            if(f != null && f.stats.isAlive)
            {
                MoveTo(f.transform.position);
                if(Vector3.Distance(transform.position, f.transform.position) < 2f)
                {
                    needs.Change("loneliness", -15f);
                    social.UpdateRelationship(f.stats.name, 2f, "Socializing");
                }
                return;
            }
        }
    }

    void Explore()
    {
        SetState("Exploring");
        if(wanderTarget == Vector3.zero || Vector3.Distance(transform.position, wanderTarget) < 1f)
            wanderTarget = transform.position + Random.insideUnitSphere.normalized * 10f;
        wanderTarget.y = transform.position.y;
        MoveTo(wanderTarget);
    }

    void FindRival()
    {
        SetState("FindingRival");
        var rival = FindNearestAlive();
        if(rival != null)
        {
            MoveTo(rival.transform.position);
            if(Vector3.Distance(transform.position, rival.transform.position) < 2f)
                needs.Change("boredom", -20f);
        }
    }

    bool CanMate()
    {
        // Sağlığı çok düşükse veya çok açsa üremesin, onun dışında serbest
        if (stats.health < 40f) return false;
        if (needs.hunger > 60f) return false;
        if (Time.time - stats.lastMatingTime < stats.matingCooldown) return false;
        return true;
    }

    bool TryMate()
    {
        foreach (var m in GenerationManager.Instance.GetAlive())
        {
            if (m == this || m.stats.isFemale == stats.isFemale) continue;

            // İlişki 55'ten büyükse (DemoScene'de 75 yaptık, hemen buraya girecekler)
            if (social.GetRelationship(m.stats.name) >= 55f)
            {
                SetState("LookingForMate");
                MoveTo(m.transform.position);

                // Yan yana geldiklerinde
                if (Vector3.Distance(transform.position, m.transform.position) < 2.5f)
                {
                    stats.lastMatingTime = Time.time;
                    m.stats.lastMatingTime = Time.time;

                    // Oyun başı düzeni için ilk çiftleşmede otomatik evlensinler
                    if (!stats.isMarried && !m.stats.isMarried)
                    {
                        stats.isMarried = true; stats.spouse = m;
                        m.stats.isMarried = true; m.stats.spouse = this;
                        NotificationManager.Instance.Show($"{stats.name} ve {m.stats.name} evlendi!", NotificationType.Social);
                    }

                    // %70 gibi yüksek bir ihtimalle çocuk doğsun (Oyun başı ivmesi için)
                    if (Random.value < 0.7f)
                    {
                        GenerationManager.Instance.TryBirth(this, m);
                    }
                }
                return true; // Eş buldu ve peşinden gidiyor
            }
        }
        return false;
    }

    // ── 6. KART DAVRANIŞLARI ──────────────────────────────
    bool RunCardBehaviors()
    {
        // ODUNCU
        if(cards.HasCard("Oduncu") && ResourceManager.Instance.woodCount < 30f)
        {
            ChopWood();
            return true;
        }

        // AVCI
        if(cards.HasCard("Avcı") && needs.hunger > 30f)
        {
            GoFish();
            return true;
        }

        // İNŞAATÇI
        if(cards.HasCard("İnşaatçı") && ResourceManager.Instance.woodCount >= 10f)
        {
            Build();
            return true;
        }

        // ŞAMAN
        if(cards.HasCard("Şaman"))
        {
            HealNearby();
            return true;
        }

        // TEMBELLİK — düşük ihtimalle çalışır
        if(cards.HasCard("Tembel") && Random.value < 0.5f)
        {
            Loaf();
            return true;
        }

        // DELİ — rastgele davranış
        if(cards.HasCard("Deli") && Random.value < stats.randomBehaviorChance)
        {
            DoCrazy();
            return true;
        }

        // MIZMIZ — şikayet et
        if(cards.HasCard("Mızmız") && Random.value < 0.1f)
        {
            Complain();
            return false; // şikayet sonra çalışmaya devam
        }

        // UNUTKAN — bazen işi bırakır
        if(cards.HasCard("Unutkan") && currentState != "Idle" && Random.value < 0.05f)
        {
            SetState("Forgot");
            agent.isStopped = true;
            Invoke(nameof(RememberWork), Random.Range(3f, 8f));
            return true;
        }

        return false;
    }

    bool ChopWood()
    {
        // Hedef yoksa yeni hedef bul ve ayarla
        if (currentTarget == null)
        {
            currentTarget = FindNearest("Tree");
            if (currentTarget == null) return false;
            SetState("ChoppingWood");
        }

        return true; // Hedef atandı, BT döngüsünde ContinueCurrentTask ile işlenecek
    }

    bool GoFish()
    {
        if (currentTarget == null)
        {
            currentTarget = FindNearest("FishingSpot");
            if (currentTarget == null) return false;
            SetState("Fishing");
        }
        return true;
    }

    bool Build()
    {
        if (currentTarget == null)
        {
            currentTarget = FindNearest("BuildSite");
            if (currentTarget == null) return false;
            SetState("Building");
        }
        return true;
    }

    // --- MEVCUT İŞİ SÜRDÜRME MOTORU ---
    void ContinueCurrentTask()
    {
        // Hedef bir şekilde yok olduysa (ağaç kesilip bittiyse vb.) işi bırak
        if (currentTarget == null || !currentTarget.activeInHierarchy)
        {
            ClearCurrentTask();
            SetState("Idle");
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

        // AŞAMA 1: YÜRÜME
        if (dist > 2.5f)
        {
            MoveTo(currentTarget.transform.position);
        }
        // AŞAMA 2: HEDEFE ULAŞTI, ÇALIŞMA
        else
        {
            agent.isStopped = true; // Hedefe varınca fren yap

            if (currentState == "ChoppingWood")
            {
                float amount = stats.workSpeed;
                if (cards.HasCard("Sinir") && emotions.GetI("Angry") > 0.5f) amount *= 1.5f;
                ResourceManager.Instance.Add("wood", amount * Time.deltaTime * 2f); // BT_TICK (2s) kadar odun ver
            }
            else if (currentState == "Fishing")
            {
                float chance = InventionManager.Instance.IsDiscovered("FishingNet") ? 0.8f : 0.5f;
                if (Random.value < chance) // Saniyeye bölmeye gerek yok, 2 saniyede bir zar atıyoruz
                {
                    ResourceManager.Instance.Add("food", 2f);
                    needs.Change("hunger", -5f);
                }
            }
            else if (currentState == "Building")
            {
                stats.buildingsConstructed++;
                ResourceManager.Instance.Spend("wood", 10f);
                NotificationManager.Instance.Show(stats.name + " bir yapı tamamladı!", NotificationType.Social);
                ClearCurrentTask(); // İnşaat bitince hedefi sıfırla ki başka işe geçsin
            }
        }
    }


    void HealNearby()
    {
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(m == this || !m.stats.isAlive) continue;
            if(!m.stats.isSick && !m.stats.isInjured) continue;
            if(Vector3.Distance(transform.position, m.transform.position) > 8f) continue;

            SetState("Healing");
            MoveTo(m.transform.position);
            if(Vector3.Distance(transform.position, m.transform.position) < 1.5f)
            {
                bool mad   = cards.HasCard("Deli");
                bool smart = cards.HasCard("Zeka");
                float s    = smart ? 0.8f : mad ? 0.3f : 0.5f;

                if(Random.value < s)
                {
                    m.stats.health  += 30f;
                    m.stats.isSick   = false;
                    m.stats.isInjured= false;
                    memory.RecordEvent("HealedSomeone", m.stats.name, 0.5f);
                    social.UpdateRelationship(m.stats.name, 10f, "Healed");
                }
                else if(mad)
                {
                    m.stats.health -= 10f; // yanlış tedavi
                }
            }
            return;
        }

        // Kimse yoksa ritüel yap
        SetState("DoingRitual");
        agent.isStopped = true;
    }

    void Loaf()
    {
        SetState("Loafing");
        agent.isStopped = true;
        needs.Change("fatigue", -1f);
    }

    void DoCrazy()
    {
        string[] crazyActions = {"Spinning","TalkingToTree","Wandering","DancingAlone"};
        SetState(crazyActions[Random.Range(0, crazyActions.Length)]);
        Vector3 rand = transform.position + Random.insideUnitSphere * 3f;
        rand.y = transform.position.y;
        MoveTo(rand);
    }

    void Complain()
    {
        NotificationManager.Instance.Show(
            stats.name + " bir şeylerden şikayet ediyor ama kim dinliyor ki?",
            NotificationType.Comedy);
    }

    void RememberWork() { SetState("Idle"); }

    // ── 7. DEFAULT ────────────────────────────────────────
    void RunDefault()
    {
        // 1. Barınak yok ve yeterli odun var -> İnşaat (Öncelik)
        if (!HasShelter() && ResourceManager.Instance.woodCount >= 10f)
        {
            var site = FindNearest("BuildSite");
            if (site != null) { Build(); return; }
        }

        // 2. Yiyecek stoğu çok iyi değilse (Örn: 20'nin altıysa)
        if (ResourceManager.Instance.foodCount < 20f)
        {
            var spot = FindNearest("FishingSpot");
            if (spot != null) { GoFish(); return; }
        }

        // 3. Hiçbiri yoksa -> SÜREKLİ ODUN KES (Karakterlerin boş kalmasını tamamen çözer)
        var tree = FindNearest("Tree");
        if (tree != null) { ChopWood(); return; }

        Idle();
    }

    void Idle()
    {
        SetState("Idle");
        // Sadece durması gerektiğinde durdur, her frame fren yapma
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            agent.isStopped = true;
        }

        if (cards.HasCard("Zeka") && memory.GetThreshold("idle_long") > 180f)
        {
            SetState("Thinking");
        }
    }

    // ── RENK SİSTEMİ ──────────────────────────────────────
    void UpdateColor()
    {
        if(minionRenderer == null || !stats.isAlive) return;

        Color c = baseColor;

        // Mutasyon rengi
        switch(stats.mutationType)
        {
            case "WaterMan":  c = Color.Lerp(c, new Color(0.5f,0.7f,1f),   0.4f); break;
            case "TreeArm":   c = Color.Lerp(c, new Color(0.4f,0.25f,0.1f),0.4f); break;
            case "AngryBlood":c = Color.Lerp(c, new Color(0.8f,0.3f,0.3f), 0.3f); break;
            case "BigBrain":  c = Color.Lerp(c, new Color(0.8f,0.8f,1f),   0.3f); break;
            case "NightEye":  c = Color.Lerp(c, new Color(0.2f,0.1f,0.3f), 0.4f); break;
        }

        // Baskın duygu rengi
        switch(emotions.Dominant())
        {
            case "Angry":      c = Color.Lerp(c, Color.red,     emotions.GetI("Angry")     * 0.4f); break;
            case "Sad":
            case "Depressed":  c = Color.Lerp(c, Color.blue,    emotions.GetI("Sad")       * 0.3f); break;
            case "Happy":
            case "Euphoric":   c = Color.Lerp(c, Color.yellow,  emotions.GetI("Happy")     * 0.2f); break;
            case "Terrified":  c = Color.Lerp(c, Color.white,   emotions.GetI("Terrified") * 0.5f); break;
            case "Vengeful":   c = Color.Lerp(c, new Color(0.5f,0,0.5f), emotions.GetI("Vengeful") * 0.4f); break;
        }

        // Kart renk izleri
        if(cards.HasCard("Sinir"))       c = Color.Lerp(c, new Color(1f,0.6f,0.6f), 0.1f);
        if(cards.HasCard("Zeka"))        c = Color.Lerp(c, new Color(0.8f,0.8f,1f), 0.1f);
        if(cards.HasCard("Deli"))        c = Color.Lerp(c, new Color(0.8f,0.4f,0.8f),0.15f);
        if(cards.HasCard("Gururlu"))     c = Color.Lerp(c, new Color(1f,0.9f,0.5f), 0.1f);
        if(cards.HasCard("KısaÖmürlü"))  c = Color.Lerp(c, new Color(1f,0.5f,0.5f), 0.1f);
        if(cards.HasCard("UzunÖmürlü"))  c = Color.Lerp(c, new Color(0.5f,1f,0.7f), 0.1f);

        minionRenderer.material.color = c;
    }

    // ── YAŞLANMA ──────────────────────────────────────────
    void AgeTick()
    {
        if(!stats.isAlive) return;
        stats.age += 1f / 60f; // saniyeyi dakikaya çevir

        // Yaşlılık yavaşlaması
        if(stats.age > stats.maxAge * 0.7f)
            agent.speed = Mathf.Max(0.5f, agent.speed - 0.001f);
    }

    // ── YARDIMCILAR ───────────────────────────────────────
    void SetState(string newState)
    {
        if(currentState != newState) stateTimer = 0f;
        currentState = newState;
        if(agent.isActiveAndEnabled) agent.isStopped = false;
    }

    void MoveTo(Vector3 pos)
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        agent.isStopped = false; // Freni mutlaka bırak runsocial

        // Hedef nokta 1 birimden fazla değişmediyse, ROTA HESAPLAMASINI TEKRARLAMA!
        if (Vector3.Distance(lastMoveTarget, pos) > 1f)
        {
            lastMoveTarget = pos;

            // 10 birimlik çapta güvenli zemin bul
            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
                agent.SetDestination(pos); // Bulamazsa zorla
        }
    }

    bool HasShelter()
        => FindNearest("Shelter") != null &&
           Vector3.Distance(transform.position, FindNearest("Shelter").transform.position) < 20f;

    Vector3 GetDangerCenter()
    {
        var dangers = Physics.OverlapSphere(transform.position, 15f, LayerMask.GetMask("Danger"));
        if(dangers.Length == 0) return transform.position;
        Vector3 center = Vector3.zero;
        foreach(var d in dangers) center += d.transform.position;
        return center / dangers.Length;
    }

    GameObject FindNearest(string tag)
    {
        GameObject[] objs;
        try { objs = GameObject.FindGameObjectsWithTag(tag); }
        catch { return null; }

        GameObject best = null; float min = Mathf.Infinity;
        foreach(var o in objs)
        {
            float d = Vector3.Distance(transform.position, o.transform.position);
            if(d < min) { min = d; best = o; }
        }
        return best;
    }

    MinionAI FindNearestAlive()
    {
        MinionAI best = null; float min = Mathf.Infinity;
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(m == this || !m.stats.isAlive) continue;
            float d = Vector3.Distance(transform.position, m.transform.position);
            if(d < min) { min = d; best = m; }
        }
        return best;
    }

    MinionAI FindNearestEnemy()
    {
        foreach(var e in stats.enemies)
            if(e != null && e.stats.isAlive) return e;
        return null;
    }
}
