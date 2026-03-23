using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ============================================================
// CascadeSystem.cs
// Zincirleme olaylar — bir olay tetiklenince köyde domino etkisi
// Her minyona eklenir.
// ============================================================
public class CascadeSystem : MonoBehaviour
{
    MinionAI      ai;
    SocialNetwork social;
    EmotionSystem emotions;
    MemorySystem  memory;
    CardSystem    cards;

    // Aktif cascade'lerin tekrar tetiklenmesini önlemek için cooldown
    Dictionary<string,float> cascadeCooldowns = new Dictionary<string,float>();
    const float CASCADE_COOLDOWN = 30f;

    void Start()
    {
        ai       = GetComponent<MinionAI>();
        social   = GetComponent<SocialNetwork>();
        emotions = GetComponent<EmotionSystem>();
        memory   = GetComponent<MemorySystem>();
        cards    = GetComponent<CardSystem>();
    }

    // ── ANA NOKTA — burası çağrılır ───────────────────────
    public void TriggerCascade(string type, string originName, float intensity)
    {
        if(!ai.stats.isAlive) return;

        // Cooldown kontrolü
        if(cascadeCooldowns.ContainsKey(type))
            if(Time.time - cascadeCooldowns[type] < CASCADE_COOLDOWN) return;
        cascadeCooldowns[type] = Time.time;

        switch(type)
        {
            case "Fight":              OnFight(originName, intensity);         break;
            case "Death":              OnDeath(originName, intensity);         break;
            case "Scandal":            OnScandal(originName, intensity);       break;
            case "Murder":             OnMurder(originName, intensity);        break;
            case "Betrayal":           OnBetrayal(originName, intensity);      break;
            case "PublicHumiliation":  OnHumiliation(originName, intensity);   break;
            case "PowerChallenge":     OnPowerChallenge(originName, intensity);break;
            case "LeaderDeath":        OnLeaderDeath(intensity);               break;
            case "Rebellion":          OnRebellion(originName, intensity);     break;
            case "War":                OnWar(intensity);                       break;
            case "Plague":             OnPlague(originName, intensity);        break;
            case "WolfAttack":         OnWolfAttack(intensity);                break;
            case "VillagePanic":       OnVillagePanic(intensity);              break;
            case "MadLaw":             OnMadLaw(originName, intensity);        break;
            case "WeakLeader":         OnWeakLeader(originName, intensity);    break;
            case "Divorce":            OnDivorce(originName, intensity);       break;
            case "Marriage":           OnMarriage(originName, intensity);      break;
            case "Election":           OnElection(intensity);                  break;
            case "Exile":              OnExile(originName, intensity);         break;
            case "BloodFeud":          OnBloodFeud(originName, intensity);     break;
            case "HolyWar":            OnHolyWar(originName, intensity);       break;
            case "DarkAge":            OnDarkAge(intensity);                   break;
            case "TotalChaos":         OnTotalChaos(intensity);                break;
            case "PowerVacuum":        OnPowerVacuum(intensity);               break;
            case "ClumsyDomino":       OnClumsyDomino(originName, intensity);  break;
            case "EnemyFormed":        OnEnemyFormed(originName, intensity);   break;
            case "PublicScandal":      OnPublicScandal(originName, intensity); break;
            case "PeaceMade":          OnPeaceMade(originName, intensity);     break;
            case "PublicFight":        OnPublicFight(originName, intensity);   break;
        }
    }

    // ── KAVGA ─────────────────────────────────────────────
    void OnFight(string originName, float intensity)
    {
        float dist = DistToOrigin(originName);
        if(dist > 12f) return;

        float myRel = social.GetRelationship(originName);

        if(myRel > 65f)
        {
            // Arkadaşı kavgada → yardıma koş
            emotions.SetEmotion("Angry", 0.4f);
            JoinFightFor(originName);
        }
        else if(myRel < 30f)
        {
            // Düşmanı kavgada → karşı tarafa katıl
            JoinFightAgainst(originName);
        }
        else if(cards.HasCard("Korkak"))
        {
            // Korkak → kaç
            emotions.SetEmotion("Terrified", 0.6f);
        }
        else
        {
            // İzle, hafif endişelen
            emotions.SetEmotion("Anxious", 0.2f);
        }
    }

    // ── ÖLÜM ──────────────────────────────────────────────
    void OnDeath(string originName, float intensity)
    {
        float myRel = social.GetRelationship(originName);

        if(myRel > 70f)
        {
            emotions.SetEmotion("Sad", 0.8f);
            memory.RecordEvent("FamilyDeath", originName, 0.9f, true);
            ai.stats.isInMourning = true;
            Invoke(nameof(EndMourning), 120f);
        }
        else if(myRel > 50f)
        {
            emotions.SetEmotion("Sad", 0.4f);
        }
        else if(myRel < 30f)
        {
            // Düşman öldü → gizli rahatlama, ama suçluluk
            emotions.SetEmotion("Guilty", 0.3f);
        }

        // Lider öldüyse güç boşluğu
        var dead = FindByName(originName);
        if(dead != null && dead.stats.isLeader)
            TriggerCascade("LeaderDeath", originName, 0.8f);
    }

    void EndMourning() { ai.stats.isInMourning = false; }

    // ── SKANDAL ───────────────────────────────────────────
    void OnScandal(string originName, float intensity)
    {
        // Dedikodu dalgasal yayılır
        var informed = new HashSet<string>();
        var queue    = new Queue<MinionAI>();

        foreach(var m in FindNearby(8f)) queue.Enqueue(m);

        int maxSpread = Mathf.RoundToInt(intensity * 10f);
        int spread    = 0;

        while(queue.Count > 0 && spread < maxSpread)
        {
            var cur = queue.Dequeue();
            if(informed.Contains(cur.stats.name)) continue;
            informed.Add(cur.stats.name);
            spread++;

            cur.GetComponent<SocialNetwork>()?.LearnInfo("Scandal_" + originName, true);

            float rel = cur.GetComponent<SocialNetwork>()?.GetRelationship(originName) ?? 50f;
            if(rel > 60f)
                cur.GetComponent<EmotionSystem>()?.SetEmotion("Anxious", 0.3f);
            else
            {
                cur.GetComponent<EmotionSystem>()?.SetEmotion("Surprised", 0.3f);
                // Arkadaşlarına da anlat
                foreach(var f in cur.GetComponent<SocialNetwork>()?.GetFriends() ?? new List<MinionAI>())
                    if(!informed.Contains(f.stats.name)) queue.Enqueue(f);
            }
        }

        // Çok yayıldıysa köy ikiye bölünür
        if(spread >= 6)
        {
            NotificationManager.Instance.Show(
                "Skandal köy geneline yayıldı! " + originName + " yüzünden.",
                NotificationType.Major);
            VillageSplit(originName);
        }
    }

    // ── CİNAYET ───────────────────────────────────────────
    void OnMurder(string originName, float intensity)
    {
        // Cinayet herkes için korku ve panik
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive || m == ai) continue;
            m.GetComponent<EmotionSystem>()?.SetEmotion("Terrified", 0.5f);
            m.GetComponent<NeedsSystem>()?.Change("fear", 25f);
        }

        NotificationManager.Instance.Show(
            "Köyde cinayet! Herkes korkuyor.", NotificationType.Major);

        // Katil kendi köyünde itibar kaybeder
        var killer = FindByName(originName);
        if(killer != null)
        {
            killer.stats.reputation -= 30f;
            killer.GetComponent<SocialNetwork>()?.LearnInfo("Murder_" + originName, true);
        }
    }

    // ── İHANET ────────────────────────────────────────────
    void OnBetrayal(string originName, float intensity)
    {
        // Herkes taraf seçmek zorunda
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive || m == ai) continue;
            var mSocial = m.GetComponent<SocialNetwork>();
            if(mSocial == null) continue;

            float betrayerRel = mSocial.GetRelationship(originName);
            float victimRel   = mSocial.GetRelationship(ai.stats.name);

            if(betrayerRel > victimRel)
            {
                mSocial.UpdateRelationship(ai.stats.name, -20f, "SidedWithBetrayer");
                m.GetComponent<EmotionSystem>()?.SetEmotion("Angry", 0.3f);
            }
            else
            {
                mSocial.UpdateRelationship(originName, -25f, "Betrayal");
                m.GetComponent<EmotionSystem>()?.SetEmotion("Angry", 0.4f);
            }
        }

        if(intensity > 0.5f) VillageSplit(originName);
    }

    // ── AŞAĞI LAMA ────────────────────────────────────────
    void OnHumiliation(string originName, float intensity)
    {
        // Aşağılayanın itibarı düşer
        var humiliator = FindByName(originName);
        if(humiliator != null)
            humiliator.stats.reputation -= 10f;

        // Şahitler tepki verir
        foreach(var m in FindNearby(10f))
        {
            float witnessRel = m.GetComponent<SocialNetwork>()?.GetRelationship(ai.stats.name) ?? 50f;
            if(witnessRel > 60f)
            {
                m.GetComponent<EmotionSystem>()?.SetEmotion("Angry", 0.3f);
                m.GetComponent<SocialNetwork>()?.UpdateRelationship(originName, -15f, "WitnessedHumiliation");
            }
            else
            {
                m.GetComponent<EmotionSystem>()?.SetEmotion("Guilty", 0.1f);
            }
        }
    }

    // ── GÜÇ MÜCADELESİ ────────────────────────────────────
    void OnPowerChallenge(string challengerName, float intensity)
    {
        NotificationManager.Instance.Show(
            challengerName + " liderliğe meydan okuyor! Köy taraf seçiyor.", NotificationType.Major);

        // Herkes taraf seç
        var challenger = FindByName(challengerName);
        var leader     = PowerSystem.Instance.currentLeader;
        if(challenger == null || leader == null) return;

        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive || m == ai) continue;
            var mSocial = m.GetComponent<SocialNetwork>();
            if(mSocial == null) continue;

            float cRel = mSocial.GetRelationship(challengerName);
            float lRel = mSocial.GetRelationship(leader.stats.name);

            if(cRel > lRel)
                m.GetComponent<SocialNetwork>()?.JoinFaction(challengerName + "'in Taraftarları",
                    challenger.GetComponent<SocialNetwork>());
            else
                m.GetComponent<SocialNetwork>()?.JoinFaction(leader.stats.name + "'in Taraftarları",
                    leader.GetComponent<SocialNetwork>());
        }

        // 60 sn sonra kazananı belirle
        Invoke(nameof(ResolvePowerChallenge), 60f);
    }

    void ResolvePowerChallenge()
    {
        var leader     = PowerSystem.Instance.currentLeader;
        if(leader == null) return;

        // En çok taraftarı olan kazanır
        MinionAI winner = null;
        int      maxFollowers = 0;

        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive) continue;
            var mSocial = m.GetComponent<SocialNetwork>();
            if(mSocial == null || !mSocial.isFactionLeader) continue;

            int followers = mSocial.factionMembers.Count;
            if(followers > maxFollowers) { maxFollowers = followers; winner = m; }
        }

        if(winner != null && winner != leader)
        {
            PowerSystem.Instance.SetLeader(winner);
            NotificationManager.Instance.Show(winner.stats.name + " yeni lider oldu!", NotificationType.Major);
        }
        else
        {
            NotificationManager.Instance.Show(leader.stats.name + " liderliğini korudu!", NotificationType.Social);
        }
    }

    // ── LİDER ÖLÜMÜ ───────────────────────────────────────
    void OnLeaderDeath(float intensity)
    {
        NotificationManager.Instance.Show("Lider öldü! Güç boşluğu var.", NotificationType.Major);

        // Adaylar
        var candidates = FindObjectsByType<MinionAI>(FindObjectsSortMode.None)
            .Where(m => m.stats.isAlive && (m.GetComponent<CardSystem>()?.HasCard("Lider") ?? false))
            .ToList();

        if(candidates.Count == 0)
        {
            // Aday yok, kaos
            TriggerCascade("PowerVacuum", "", 0.8f);
        }
        else if(candidates.Count == 1)
        {
            PowerSystem.Instance.SetLeader(candidates[0]);
            NotificationManager.Instance.Show(candidates[0].stats.name + " lider oldu.", NotificationType.Major);
        }
        else
        {
            // Birden çok aday, güç mücadelesi
            var strongest = candidates.OrderByDescending(m => m.stats.reputation).First();
            TriggerCascade("PowerChallenge", strongest.stats.name, 0.7f);
        }
    }

    // ── İSYAN ─────────────────────────────────────────────
    void OnRebellion(string originName, float intensity)
    {
        NotificationManager.Instance.Show(
            "Köyde isyan! " + originName + "'a karşı halk ayaklandı.", NotificationType.Major);

        // Liderin itibarı çöker
        var leader = PowerSystem.Instance.currentLeader;
        if(leader != null) leader.stats.reputation -= 25f;

        // Tüm köyde öfke
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive) continue;
            m.GetComponent<EmotionSystem>()?.SetEmotion("Angry", 0.5f);
            m.GetComponent<NeedsSystem>()?.Change("injustice", -20f); // isyan çözüm hissi verir
        }

        Invoke(nameof(ResolveRebellion), 30f);
    }

    void ResolveRebellion()
    {
        var leader = PowerSystem.Instance.currentLeader;
        if(leader == null) return;

        if(leader.stats.reputation < 20f)
        {
            TriggerCascade("LeaderDeath", leader.stats.name, 0.9f);
            leader.stats.isLeader = false;
        }
    }

    // ── SAVAŞ ─────────────────────────────────────────────
    void OnWar(float intensity)
    {
        NotificationManager.Instance.Show("Köyde savaş başladı!", NotificationType.Major);

        // Tüm köyde korku ve öfke
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive) continue;
            m.GetComponent<EmotionSystem>()?.SetEmotion("Angry",    0.6f);
            m.GetComponent<EmotionSystem>()?.SetEmotion("Terrified",0.3f);
            m.GetComponent<NeedsSystem>()?.Change("fear", 30f);
        }
    }

    // ── SALGON ────────────────────────────────────────────
    void OnPlague(string originName, float intensity)
    {
        NotificationManager.Instance.Show("Salgın yayılıyor!", NotificationType.Major);

        // Yakındaki minyonlara bulaş
        foreach(var m in FindNearby(15f))
        {
            if(m.stats.isSick) continue;
            if(Random.value < intensity * 0.4f)
            {
                m.stats.isSick = true;
                m.GetComponent<EmotionSystem>()?.SetEmotion("Anxious", 0.4f);
            }
        }
    }

    // ── KURT SALDIRISI ────────────────────────────────────
    void OnWolfAttack(float intensity)
    {
        // Korkak olanlar panikler
        foreach(var m in FindNearby(12f))
        {
            if(m.GetComponent<CardSystem>()?.HasCard("Korkak") ?? false)
                m.GetComponent<EmotionSystem>()?.SetEmotion("Terrified", 0.8f);
            else
                m.GetComponent<EmotionSystem>()?.SetEmotion("Anxious", 0.4f);

            m.GetComponent<NeedsSystem>()?.Change("fear", 25f);
        }
    }

    // ── KÖY PANİĞİ ────────────────────────────────────────
    void OnVillagePanic(float intensity)
    {
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive) continue;
            m.GetComponent<EmotionSystem>()?.SetEmotion("Terrified", 0.4f);
            m.GetComponent<NeedsSystem>()?.Change("fear", 20f);
        }
        NotificationManager.Instance.Show("Köyde panik!", NotificationType.Major);
    }

    // ── DELİ YASA ─────────────────────────────────────────
    void OnMadLaw(string originName, float intensity)
    {
        // Köy garip bir yasa uygular
        string[] laws = {
            "Artık salı günleri ağaç kesmek yasak!",
            "Herkes şapka takmak zorunda.",
            "Balık yemek suç sayılacak.",
            "Güneşe selam vermek zorunlu.",
            "Gece dışarı çıkmak yasak, ama neden bilinmiyor."
        };
        string law = laws[Random.Range(0, laws.Length)];
        NotificationManager.Instance.Show(originName + " yeni yasa: " + law, NotificationType.Surprising);

        // Köyde garip bir itaat — itibar beklenmedik yönde değişir
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive) continue;
            m.GetComponent<EmotionSystem>()?.SetEmotion("Surprised", 0.3f);
        }
    }

    // ── ZAYIF LİDER ───────────────────────────────────────
    void OnWeakLeader(string originName, float intensity)
    {
        // Rakip güç toplamaya başlar
        var rivals = FindObjectsByType<MinionAI>(FindObjectsSortMode.None)
            .Where(m => m.stats.isAlive && m.stats.reputation > 50f && !m.stats.isLeader)
            .OrderByDescending(m => m.stats.reputation)
            .FirstOrDefault();

        if(rivals != null)
            NotificationManager.Instance.Show(
                rivals.stats.name + " liderin zayıflığından güç topluyor.", NotificationType.Drama);
    }

    // ── BOŞANMA ───────────────────────────────────────────
    void OnDivorce(string originName, float intensity)
    {
        // Tanıklar taraf seç
        foreach(var m in FindNearby(10f))
        {
            var mSocial = m.GetComponent<SocialNetwork>();
            if(mSocial == null) continue;

            float myRel    = mSocial.GetRelationship(ai.stats.name);
            float otherRel = mSocial.GetRelationship(originName);

            if(myRel > otherRel)
                mSocial.UpdateRelationship(ai.stats.name, 5f, "DivorceSupport");
            else
                mSocial.UpdateRelationship(originName, 5f, "DivorceSupport");
        }

        NotificationManager.Instance.Show(
            ai.stats.name + " ve " + originName + " boşandı!", NotificationType.Drama);
    }

    // ── EVLİLİK ───────────────────────────────────────────
    void OnMarriage(string originName, float intensity)
    {
        // Çevredekiler mutlu olur
        foreach(var m in FindNearby(8f))
            m.GetComponent<EmotionSystem>()?.SetEmotion("Happy", 0.2f);

        NotificationManager.Instance.Show(
            ai.stats.name + " ve " + originName + " evlendi!", NotificationType.Social);
    }

    // ── SEÇİM ─────────────────────────────────────────────
    void OnElection(float intensity)
    {
        NotificationManager.Instance.Show("Köyde lider seçimi başladı!", NotificationType.Major);

        // En yüksek itibar kazanır
        var winner = FindObjectsByType<MinionAI>(FindObjectsSortMode.None)
            .Where(m => m.stats.isAlive && m.GetComponent<CardSystem>()?.HasCard("Lider") == true)
            .OrderByDescending(m => m.stats.reputation)
            .FirstOrDefault();

        if(winner != null)
        {
            PowerSystem.Instance.SetLeader(winner);
            winner.GetComponent<EmotionSystem>()?.SetEmotion("Proud", 0.8f);
            NotificationManager.Instance.Show(winner.stats.name + " seçimi kazandı!", NotificationType.Major);
        }
    }

    // ── SÜRGÜN ────────────────────────────────────────────
    void OnExile(string originName, float intensity)
    {
        NotificationManager.Instance.Show(originName + " köyden sürüldü!", NotificationType.Major);

        // Köyde hafif rahatlama ya da üzüntü — ilişkiye göre
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive || m == ai) continue;
            float rel = m.GetComponent<SocialNetwork>()?.GetRelationship(originName) ?? 50f;
            if(rel > 60f) m.GetComponent<EmotionSystem>()?.SetEmotion("Sad", 0.3f);
            else          m.GetComponent<EmotionSystem>()?.SetEmotion("Peaceful", 0.2f);
        }
    }

    // ── KAN DAVASI ────────────────────────────────────────
    void OnBloodFeud(string originName, float intensity)
    {
        NotificationManager.Instance.Show(
            originName + " ile kan davası başladı! Nesiller boyu sürecek.", NotificationType.Major);

        // Aile üyeleri de düşman ilan eder
        var origin = FindByName(originName);
        if(origin == null) return;

        foreach(var child in ai.stats.children)
        {
            if(child == null || !child.stats.isAlive) continue;
            child.GetComponent<SocialNetwork>()?.UpdateRelationship(originName, -40f, "BloodFeud");
            if(!child.stats.enemies.Contains(origin)) child.stats.enemies.Add(origin);
        }
    }

    // ── KUTSAL SAVAŞ ──────────────────────────────────────
    void OnHolyWar(string originName, float intensity)
    {
        NotificationManager.Instance.Show(
            "İnanç savaşı! " + originName + " kutsal savaş ilan etti.", NotificationType.Major);
        OnWar(intensity); // Savaş etkilerini uygula
    }

    // ── KARANLIK ÇAĞ ─────────────────────────────────────
    void OnDarkAge(float intensity)
    {
        NotificationManager.Instance.Show(
            "Köy karanlık çağa giriyor...", NotificationType.Major);

        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive) continue;
            m.GetComponent<EmotionSystem>()?.SetEmotion("Sad",      0.6f);
            m.GetComponent<EmotionSystem>()?.SetEmotion("Hopeless", 0.4f);
            m.GetComponent<NeedsSystem>()?.Change("loneliness",  20f);
        }
    }

    // ── TAM KAOS ──────────────────────────────────────────
    void OnTotalChaos(float intensity)
    {
        NotificationManager.Instance.Show("TAM KAOS! Her şey aynı anda oluyor!", NotificationType.Major);
        OnWolfAttack(0.5f);
        OnVillagePanic(0.8f);
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive) continue;
            m.GetComponent<EmotionSystem>()?.SetEmotion("Terrified", 0.7f);
        }
    }

    // ── GÜÇ BOŞLUĞU ───────────────────────────────────────
    void OnPowerVacuum(float intensity)
    {
        NotificationManager.Instance.Show("Güç boşluğu! Köyde otorite yok.", NotificationType.Major);

        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive) continue;
            m.GetComponent<EmotionSystem>()?.SetEmotion("Anxious", 0.5f);
        }

        Invoke(nameof(FillPowerVacuum), 20f);
    }

    void FillPowerVacuum()
    {
        var candidate = FindObjectsByType<MinionAI>(FindObjectsSortMode.None)
            .Where(m => m.stats.isAlive)
            .OrderByDescending(m => m.stats.reputation)
            .FirstOrDefault();

        if(candidate != null)
        {
            PowerSystem.Instance.SetLeader(candidate);
            NotificationManager.Instance.Show(
                candidate.stats.name + " güç boşluğunu doldurdu!", NotificationType.Major);
        }
    }

    // ── KLUMSy DOMINO ─────────────────────────────────────
    void OnClumsyDomino(string originName, float intensity)
    {
        // Yakındakilere küçük hasar, komik sonuç
        int affected = 0;
        foreach(var m in FindNearby(6f))
        {
            m.GetComponent<NeedsSystem>()?.Change("health", -3f);
            affected++;
        }
        if(affected > 0)
            NotificationManager.Instance.Show(
                originName + "'ın klumsy anı " + affected + " kişiyi etkiledi!", NotificationType.Comedy);
    }

    // ── DÜŞMAN OLUŞUMU ────────────────────────────────────
    void OnEnemyFormed(string originName, float intensity)
    {
        // Yakındakiler gerilimi hisseder
        foreach(var m in FindNearby(8f))
            m.GetComponent<EmotionSystem>()?.SetEmotion("Anxious", 0.2f);
    }

    // ── HALK SKANDALI ─────────────────────────────────────
    void OnPublicScandal(string originName, float intensity)
    {
        foreach(var m in FindNearby(10f))
        {
            m.GetComponent<SocialNetwork>()?.LearnInfo("Scandal_" + originName, true);
            m.GetComponent<EmotionSystem>()?.SetEmotion("Surprised", 0.3f);
        }
        NotificationManager.Instance.Show(
            originName + " herkese rezil oldu!", NotificationType.Drama);
    }

    // ── BARIŞ YAPILDI ─────────────────────────────────────
    void OnPeaceMade(string originName, float intensity)
    {
        // Kavgacılar barışır, çevredekiler rahatlar
        foreach(var m in FindNearby(10f))
        {
            m.GetComponent<EmotionSystem>()?.SetEmotion("Peaceful", 0.3f);
            m.GetComponent<NeedsSystem>()?.Change("fear", -10f);
        }
        NotificationManager.Instance.Show(
            originName + " kavgası barışla bitti.", NotificationType.Social);
    }

    // ── HALK KAVGASI ──────────────────────────────────────
    void OnPublicFight(string originName, float intensity)
    {
        // Herkes izler, bazıları katılır
        foreach(var m in FindNearby(8f))
        {
            float rel = m.GetComponent<SocialNetwork>()?.GetRelationship(originName) ?? 50f;
            if(rel < 30f)
                m.GetComponent<EmotionSystem>()?.SetEmotion("Angry", 0.4f);
            else if(m.GetComponent<CardSystem>()?.HasCard("Korkak") ?? false)
                m.GetComponent<EmotionSystem>()?.SetEmotion("Terrified", 0.5f);
            else
                m.GetComponent<EmotionSystem>()?.SetEmotion("Surprised", 0.3f);
        }
    }

    // ── YARDIMCILAR ───────────────────────────────────────

    void VillageSplit(string originName)
    {
        NotificationManager.Instance.Show(
            "Köy ikiye bölündü! " + originName + " yüzünden.", NotificationType.Major);

        var sorted = FindObjectsByType<MinionAI>(FindObjectsSortMode.None)
            .Where(m => m.stats.isAlive)
            .OrderByDescending(m => m.stats.reputation)
            .ToList();

        if(sorted.Count >= 2)
        {
            sorted[0].GetComponent<SocialNetwork>()?.FormFaction("Birinci Grup");
            sorted[1].GetComponent<SocialNetwork>()?.FormFaction("İkinci Grup");
        }
    }

    void JoinFightFor(string friendName)
    {
        var friend = FindByName(friendName);
        if(friend?.currentAttacker == null) return;
        ai.currentState = "Fighting";
    }

    void JoinFightAgainst(string originName)
    {
        var origin = FindByName(originName);
        if(origin == null) return;
        ai.currentState = "Fighting";
    }

    float DistToOrigin(string originName)
    {
        var origin = FindByName(originName);
        return origin == null ? Mathf.Infinity
            : Vector3.Distance(transform.position, origin.transform.position);
    }

    MinionAI[] FindNearby(float radius)
        => FindObjectsByType<MinionAI>(FindObjectsSortMode.None)
            .Where(m => m != ai && m.stats.isAlive &&
                Vector3.Distance(transform.position, m.transform.position) < radius)
            .ToArray();

    MinionAI FindByName(string name)
    {
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
            if(m.stats.name == name) return m;
        return null;
    }
}
