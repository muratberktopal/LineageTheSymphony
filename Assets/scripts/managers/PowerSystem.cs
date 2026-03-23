using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ============================================================
// PowerSystem.cs — Singleton
// Güç dengesi, lider seçimi, darbe
// ============================================================
public class PowerSystem : MonoBehaviour
{
    public static PowerSystem Instance;

    public MinionAI currentLeader;
    public bool     isPowerStruggle = false;

    // Her minyonun güç puanı
    Dictionary<string,float> powerScores = new Dictionary<string,float>();

    void Awake() { Instance = this; }

    void Start()
    {
        InvokeRepeating(nameof(UpdateScores),  0f, 10f);
        InvokeRepeating(nameof(CheckBalance),  0f, 30f);
    }

    void UpdateScores()
    {
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive) continue;

            float score = 0f;
            score += m.stats.reputation * 0.4f;
            score += m.stats.friends.Count * 5f;

            var cards = m.GetComponent<CardSystem>();
            if(cards != null)
            {
                if(cards.HasCard("Lider"))   score += 20f;
                if(cards.HasCard("Sosyal"))  score += 10f;
                if(cards.HasCard("Zeka"))    score += 8f;
                if(cards.HasCard("Koruyucu"))score += 5f;
                if(cards.HasCard("Sinir"))   score -= 5f;
                if(cards.HasCard("Bencil"))  score -= 8f;
            }

            var social = m.GetComponent<SocialNetwork>();
            if(social != null)
                score += social.factionMembers.Count * 8f;

            // Eski soydan gelenler daha saygın
            score += Mathf.Max(0, (5 - m.stats.generation)) * 3f;

            powerScores[m.stats.name] = score;
        }
    }

    void CheckBalance()
    {
        if(isPowerStruggle) return;

        // Lider öldü mü
        if(currentLeader == null || !currentLeader.stats.isAlive)
        {
            TriggerVacuum();
            return;
        }

        float leaderScore = GetScore(currentLeader.stats.name);

        // Rakip var mı
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive || m == currentLeader) continue;
            if(GetScore(m.stats.name) > leaderScore * 1.3f)
            {
                TriggerChallenge(m);
                break;
            }
        }
    }

    void TriggerVacuum()
    {
        isPowerStruggle = true;
        NotificationManager.Instance.Show("Güç boşluğu! Kim lider olacak?", NotificationType.Major);
        Invoke(nameof(ResolveVacuum), 20f);
    }

    void ResolveVacuum()
    {
        var candidate = powerScores
            .OrderByDescending(p => p.Value)
            .Select(p => FindByName(p.Key))
            .FirstOrDefault(m => m != null && m.stats.isAlive);

        if(candidate != null) SetLeader(candidate);
        isPowerStruggle = false;
    }

    void TriggerChallenge(MinionAI challenger)
    {
        isPowerStruggle = true;
        NotificationManager.Instance.Show(
            challenger.stats.name + " liderliğe meydan okuyor!", NotificationType.Major);

        // Köy taraf seçer
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive || m == challenger || m == currentLeader) continue;
            var social = m.GetComponent<SocialNetwork>();
            if(social == null) continue;

            float cRel = social.GetRelationship(challenger.stats.name);
            float lRel = social.GetRelationship(currentLeader.stats.name);

            if(cRel > lRel)
                social.JoinFaction(challenger.stats.name + "'in Taraftarları",
                    challenger.GetComponent<SocialNetwork>());
            else
                social.JoinFaction(currentLeader.stats.name + "'in Taraftarları",
                    currentLeader.GetComponent<SocialNetwork>());
        }

        Invoke(nameof(ResolveChallenge), 60f);
    }

    void ResolveChallenge()
    {
        if(currentLeader == null) { ResolveVacuum(); return; }

        float leaderPow    = GetScore(currentLeader.stats.name);
        float challengerPow = 0f;
        MinionAI challenger = null;

        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(!m.stats.isAlive || m == currentLeader) continue;
            var social = m.GetComponent<SocialNetwork>();
            if(social == null || !social.isFactionLeader) continue;

            float p = GetScore(m.stats.name) + social.factionMembers.Count * 10f;
            if(p > challengerPow) { challengerPow = p; challenger = m; }
        }

        if(challenger != null && challengerPow > leaderPow)
        {
            SetLeader(challenger);
            NotificationManager.Instance.Show(
                challenger.stats.name + " yeni lider oldu!", NotificationType.Major);
        }
        else
        {
            NotificationManager.Instance.Show(
                currentLeader.stats.name + " liderliğini korudu!", NotificationType.Social);
        }

        isPowerStruggle = false;
    }

    public void SetLeader(MinionAI newLeader)
    {
        if(currentLeader != null)
        {
            currentLeader.stats.isLeader = false;
            currentLeader.GetComponent<EmotionSystem>()?.SetEmotion("Humiliated", 0.5f);
        }

        currentLeader = newLeader;
        newLeader.stats.isLeader = true;
        newLeader.GetComponent<EmotionSystem>()?.SetEmotion("Proud", 0.8f);
        newLeader.stats.reputation = Mathf.Min(100, newLeader.stats.reputation + 20f);
    }

    public float GetScore(string name)
        => powerScores.ContainsKey(name) ? powerScores[name] : 0f;

    MinionAI FindByName(string name)
    {
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
            if(m.stats.name == name) return m;
        return null;
    }
}
