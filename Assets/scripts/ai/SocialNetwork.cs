using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ============================================================
// SocialNetwork.cs
// İlişki skoru, dedikodu, fraksiyon, itibar yönetimi
// Her minyona eklenir.
// ============================================================
public class SocialNetwork : MonoBehaviour
{
    MinionAI      ai;
    EmotionSystem emotions;
    MemorySystem  memory;
    CardSystem    cards;

    // İlişki skorları: minyon ismi → 0-100
    // 0-20  = Düşman
    // 20-40 = Soğuk
    // 40-60 = Nötr
    // 60-80 = Dost
    // 80-100= Yakın dost
    public Dictionary<string,float> relationships = new Dictionary<string,float>();

    // Fraksiyon
    public string      factionName    = "None";
    public bool        isFactionLeader = false;
    public List<string> factionMembers = new List<string>();

    // Bilgi ağı: hangi bilgileri biliyor
    public Dictionary<string,bool> knownInfo = new Dictionary<string,bool>();

    // Köy geneli itibar
    public float villageReputation = 50f;

    void Start()
    {
        ai       = GetComponent<MinionAI>();
        emotions = GetComponent<EmotionSystem>();
        memory   = GetComponent<MemorySystem>();
        cards    = GetComponent<CardSystem>();

        InvokeRepeating(nameof(TickRelationships), 0f, 5f);
        InvokeRepeating(nameof(TickGossip),        0f, 10f);
        InvokeRepeating(nameof(TickReputation),    0f, 15f);
    }

    // ── İLİŞKİ ────────────────────────────────────────────

    public float GetRelationship(string name)
        => relationships.ContainsKey(name) ? relationships[name] : 50f;

    public void UpdateRelationship(string name, float delta, string reason)
    {
        if(!relationships.ContainsKey(name)) relationships[name] = 50f;

        float old = relationships[name];
        relationships[name] = Mathf.Clamp(relationships[name] + delta, 0f, 100f);
        float neo = relationships[name];

        // Hafızaya büyük değişimleri yaz
        if(Mathf.Abs(delta) > 10f)
            memory.RecordEvent(delta > 0 ? "RelationshipImproved" : "RelationshipDamaged",
                name, Mathf.Abs(delta) / 100f);

        // Eşik geçişleri
        if(old >= 20f && neo < 20f)  BecomeEnemy(name);
        if(old < 60f  && neo >= 60f) BecomeFriend(name);
    }

    void BecomeEnemy(string name)
    {
        var enemy = FindByName(name);
        if(enemy == null) return;
        if(!ai.stats.enemies.Contains(enemy)) ai.stats.enemies.Add(enemy);
        emotions.SetEmotion("Angry", 0.4f);
        NotificationManager.Instance.Show(
            ai.stats.name + " ile " + name + " düşman oldu.", NotificationType.Drama);
        GetComponent<CascadeSystem>()?.TriggerCascade("EnemyFormed", name, 0.3f);
    }

    void BecomeFriend(string name)
    {
        var friend = FindByName(name);
        if(friend == null) return;
        if(!ai.stats.friends.Contains(friend)) ai.stats.friends.Add(friend);
        emotions.SetEmotion("Happy", 0.3f);
        NotificationManager.Instance.Show(
            ai.stats.name + " ile " + name + " arkadaş oldu.", NotificationType.Social);
    }

    void TickRelationships()
    {
        if(!ai.stats.isAlive) return;

        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(m == ai || !m.stats.isAlive) continue;
            float dist = Vector3.Distance(transform.position, m.transform.position);
            if(dist > 5f) continue;

            // Yakın çalışma ilişkiyi güçlendirir
            float delta = 0.1f;
            if(cards.HasCard("Sosyal"))      delta *= 1.5f;
            if(cards.HasCard("Sinir"))       delta *= 0.5f;
            if(cards.HasCard("Şüpheci"))     delta *= 0.3f; // yavaş ısınır
            if(cards.HasCard("AşırıSosyal")) delta *= 2f;   // çabuk kaynaşır
            if(cards.HasCard("Bencil"))      delta *= 0.2f; // ilişki kurmak umurunda değil

            UpdateRelationship(m.stats.name, delta, "Proximity");
        }
    }

    // ── DEDİKODU ──────────────────────────────────────────

    void TickGossip()
    {
        if(!ai.stats.isAlive) return;
        if(!cards.HasCard("Dedikoduncu") && !cards.HasCard("AşırıSosyal")) return;

        var target = FindNearest();
        if(target == null) return;

        var targetNet = target.GetComponent<SocialNetwork>();
        if(targetNet == null) return;

        foreach(var info in knownInfo.ToList())
        {
            if(targetNet.knownInfo.ContainsKey(info.Key)) continue;

            // Dedikoduncu bazen bilgiyi bozar
            bool distorted = Random.value < 0.2f;
            targetNet.knownInfo[info.Key] = !distorted;

            if(distorted)
            {
                NotificationManager.Instance.Show(
                    ai.stats.name + " dedikodu yaydı ama yanlış anlattı!",
                    NotificationType.Drama);
                // Yanlış bilgi ilişkiyi bozar
                string subject = ExtractSubject(info.Key);
                if(!string.IsNullOrEmpty(subject))
                    targetNet.UpdateRelationship(subject, -15f, "FalseGossip");
            }
        }
    }

    public void LearnInfo(string key, bool isTrue)
    {
        knownInfo[key] = isTrue;
        if(key.Contains("Betrayal"))   emotions.SetEmotion("Angry",   0.6f);
        else if(key.Contains("Death")) emotions.SetEmotion("Sad",     0.5f);
        else if(key.Contains("Scandal"))emotions.SetEmotion("Surprised",0.3f);
    }

    // ── FRAKSIYON ─────────────────────────────────────────

    public void FormFaction(string name)
    {
        factionName     = name;
        isFactionLeader = true;
        factionMembers.Clear();

        // En yakın dostları davet et (max 3)
        var topFriends = relationships
            .Where(r => r.Value > 60f)
            .OrderByDescending(r => r.Value)
            .Take(3)
            .Select(r => r.Key);

        foreach(var fname in topFriends)
        {
            var friend = FindByName(fname);
            if(friend == null) continue;
            friend.GetComponent<SocialNetwork>()?.JoinFaction(name, this);
            factionMembers.Add(fname);
        }

        NotificationManager.Instance.Show(
            ai.stats.name + " '" + name + "' fraksiyonunu kurdu!", NotificationType.Major);
    }

    public void JoinFaction(string name, SocialNetwork leader)
    {
        factionName     = name;
        isFactionLeader = false;
        UpdateRelationship(leader.ai.stats.name, 10f, "FactionBond");
    }

    public void LeaveFaction()
    {
        factionName     = "None";
        isFactionLeader = false;
        factionMembers.Clear();
    }

    // ── İTİBAR ────────────────────────────────────────────

    void TickReputation()
    {
        if(!ai.stats.isAlive) return;

        float delta = 0f;
        // Son 10 hafıza olayına göre itibar hesapla
        foreach(var m in memory.memories.OrderByDescending(x => x.timestamp).Take(10))
        {
            switch(m.eventType)
            {
                case "GaveFood":         delta += 2f;  break;
                case "HelpedBuild":      delta += 1f;  break;
                case "SavedSomeone":     delta += 5f;  break;
                case "Attacked":         delta -= 3f;  break;
                case "CaughtStealing":   delta -= 5f;  break;
                case "PublicHumiliation":delta -= 8f;  break;
                case "Betrayal":         delta -= 10f; break;
                case "Murdered":         delta -= 15f; break;
                case "HealedSomeone":    delta += 4f;  break;
            }
        }

        villageReputation = Mathf.Clamp(villageReputation + delta * 0.1f, 0f, 100f);
        ai.stats.reputation = villageReputation;

        // Çok düşük itibar → dışlanma
        if(villageReputation < 20f)
        {
            GetComponent<NeedsSystem>()?.Change("disrespect", 5f);
            emotions.SetEmotion("Humiliated", 0.2f);
        }
    }

    // ── YARDIMCI ──────────────────────────────────────────

    MinionAI FindNearest()
    {
        MinionAI best = null; float min = Mathf.Infinity;
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(m == ai || !m.stats.isAlive) continue;
            float d = Vector3.Distance(transform.position, m.transform.position);
            if(d < min) { min = d; best = m; }
        }
        return best;
    }

    MinionAI FindByName(string name)
    {
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
            if(m.stats.name == name) return m;
        return null;
    }

    string ExtractSubject(string key)
    {
        // "Betrayal_Ahmet" → "Ahmet"
        int i = key.IndexOf('_');
        return i >= 0 ? key.Substring(i + 1) : "";
    }

    public List<MinionAI> GetFriends() => ai.stats.friends;
    public List<MinionAI> GetEnemies() => ai.stats.enemies;
}
