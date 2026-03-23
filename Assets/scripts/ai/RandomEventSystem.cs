using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ============================================================
// RandomEventSystem.cs
// Her minyona eklenir. Encounter'ları kontrol eder ve tetikler.
// ============================================================
public class RandomEventSystem : MonoBehaviour
{
    MinionAI       ai;
    NeedsSystem    needs;
    EmotionSystem  emotions;
    MemorySystem   memory;
    SocialNetwork  social;
    CardSystem     cards;
    CascadeSystem  cascade;

    static List<RandomEventData> allEvents;

    Dictionary<string,float> lastTriggered = new Dictionary<string,float>();
    HashSet<string>          triggeredUnique = new HashSet<string>();

    // Öncelik sistemi: nesil → izin verilen max priority
    int AllowedMaxPriority()
    {
        int gen  = ai.stats.generation;
        int pop  = GenerationManager.Instance.GetTotalAliveCount();
        if(gen >= 4 || pop >= 10) return 5;
        if(gen >= 3) return 4;
        if(gen >= 2) return 3;
        if(gen >= 1) return 2;
        return 1;
    }

    void Start()
    {
        ai       = GetComponent<MinionAI>();
        needs    = GetComponent<NeedsSystem>();
        emotions = GetComponent<EmotionSystem>();
        memory   = GetComponent<MemorySystem>();
        social   = GetComponent<SocialNetwork>();
        cards    = GetComponent<CardSystem>();
        cascade  = GetComponent<CascadeSystem>();

        // Singleton yükleme — sadece bir kez
        if(allEvents == null)
            allEvents = RandomEncounterDatabase.GetAllEncounters();

        InvokeRepeating(nameof(CheckEvents), 2f, 2f);
    }

    void CheckEvents()
    {
        if(!ai.stats.isAlive) return;

        // Depresyon varsa iş reddi
        if(emotions.CanRefuseWork() && Random.value < 0.3f) return;

        int maxPri = AllowedMaxPriority();

        foreach(var evt in allEvents)
        {
            // Öncelik filtresi
            if(evt.priority > maxPri) continue;

            // Unique kontrolü
            if(evt.isUnique && triggeredUnique.Contains(evt.eventId)) continue;

            // Cooldown kontrolü
            if(lastTriggered.TryGetValue(evt.eventId, out float last))
                if(Time.time - last < evt.cooldown) continue;

            // Koşul kontrolü
            if(!CheckAll(evt.conditions)) continue;

            // Şans kontrolü
            if(Random.value > evt.triggerChance) continue;

            Trigger(evt);
        }
    }

    bool CheckAll(List<EventCondition> conds)
    {
        foreach(var c in conds)
            if(!Check(c)) return false;
        return true;
    }

    bool Check(EventCondition c)
    {
        switch(c.conditionType)
        {
            case EventCondition.ConditionType.HasCard:           return cards.HasCard(c.parameter);
            case EventCondition.ConditionType.NeedAbove:         return needs.Get(c.parameter) > c.value;
            case EventCondition.ConditionType.NeedBelow:         return needs.Get(c.parameter) < c.value;
            case EventCondition.ConditionType.EmotionAbove:      return emotions.GetI(c.parameter) > c.value;
            case EventCondition.ConditionType.EmotionBelow:      return emotions.GetI(c.parameter) < c.value;
            case EventCondition.ConditionType.RelationshipBelow:
                string rTarget = c.parameter == "spouse" && ai.stats.spouse != null
                    ? ai.stats.spouse.stats.name : c.parameter;
                return social.GetRelationship(rTarget) < c.value;
            case EventCondition.ConditionType.RelationshipAbove:
                string rTarget2 = c.parameter == "spouse" && ai.stats.spouse != null
                    ? ai.stats.spouse.stats.name : c.parameter;
                return social.GetRelationship(rTarget2) > c.value;
            case EventCondition.ConditionType.ThresholdMet:      return memory.GetThreshold(c.parameter) > c.value;
            case EventCondition.ConditionType.IsMarried:         return ai.stats.isMarried;
            case EventCondition.ConditionType.IsLeader:          return ai.stats.isLeader;
            case EventCondition.ConditionType.HasInvention:      return InventionManager.Instance.IsDiscovered(c.parameter);
            case EventCondition.ConditionType.ReputationBelow:   return ai.stats.reputation < c.value;
            case EventCondition.ConditionType.ReputationAbove:   return ai.stats.reputation > c.value;
            case EventCondition.ConditionType.GenerationAbove:   return ai.stats.generation > c.value;
            case EventCondition.ConditionType.FriendCountAbove:  return ai.stats.friends.Count > c.value;
            case EventCondition.ConditionType.EnemyCountAbove:   return ai.stats.enemies.Count > c.value;
            case EventCondition.ConditionType.IsNight:           return TimeManager.Instance.isNight;
            case EventCondition.ConditionType.IsWinter:          return TimeManager.Instance.isWinter;
            case EventCondition.ConditionType.HasShelter:        return ai.stats.isAlive; // placeholder
            case EventCondition.ConditionType.IsInjured:         return ai.stats.isInjured;
            case EventCondition.ConditionType.IsSick:            return ai.stats.isSick;
            case EventCondition.ConditionType.SpouseCheating:    return ai.stats.spouse != null && ai.stats.spouse.stats.hasSecretRelationship;
            case EventCondition.ConditionType.NearbyFightExists: return IsFightNearby();
            case EventCondition.ConditionType.ResourceAbove:     return GetResource(c.parameter) > c.value;
            case EventCondition.ConditionType.ResourceBelow:     return GetResource(c.parameter) < c.value;
            case EventCondition.ConditionType.AgeAbove:          return ai.stats.age > c.value;
            case EventCondition.ConditionType.HoardingAbove:     return ai.stats.hoardingTendency > c.value;
            case EventCondition.ConditionType.TrustBelow:        return ai.stats.trustLevel < c.value;
            case EventCondition.ConditionType.IsInMourning:      return ai.stats.isInMourning;
            case EventCondition.ConditionType.FactionExists:     return social.factionName != "None";
            case EventCondition.ConditionType.PopulationAbove:   return GenerationManager.Instance.GetTotalAliveCount() > c.value;
            case EventCondition.ConditionType.PopulationBelow:   return GenerationManager.Instance.GetTotalAliveCount() < c.value;
            default: return true;
        }
    }

    void Trigger(RandomEventData evt)
    {
        lastTriggered[evt.eventId] = Time.time;
        if(evt.isUnique) triggeredUnique.Add(evt.eventId);
        memory.RecordEvent(evt.eventId, "", 0.5f);

        foreach(var o in evt.outcomes) Apply(o);

        EventLogSystem.Instance.AddEntry(
            evt.eventId, ai.stats.name, NotificationType.Social, ai.stats.generation);
    }

    void Apply(EventOutcome o)
    {
        switch(o.outcomeType)
        {
            case EventOutcome.OutcomeType.SetEmotion:
                emotions.SetEmotion(o.parameter, o.value); break;

            case EventOutcome.OutcomeType.ChangeNeed:
                needs.Change(o.parameter, o.value); break;

            case EventOutcome.OutcomeType.ChangeRelationship:
                string rTarget = o.parameter == "spouse" && ai.stats.spouse != null
                    ? ai.stats.spouse.stats.name : o.parameter;
                social.UpdateRelationship(rTarget, o.value, "EventOutcome"); break;

            case EventOutcome.OutcomeType.ChangeReputation:
                ai.stats.reputation = Mathf.Clamp(ai.stats.reputation + o.value, 0, 100); break;

            case EventOutcome.OutcomeType.ChangeStat:
                ChangeStat(o.parameter, o.value); break;

            case EventOutcome.OutcomeType.AttackMinion:
                AttackTarget(o.parameter); break;

            case EventOutcome.OutcomeType.KillMinion:
                KillTarget(o.parameter); break;

            case EventOutcome.OutcomeType.InventItem:
                InventionManager.Instance.RegisterInvention(o.parameter, ai.stats.name); break;

            case EventOutcome.OutcomeType.TriggerCascade:
                cascade.TriggerCascade(o.parameter, ai.stats.name, o.value); break;

            case EventOutcome.OutcomeType.ShowNotification:
                string msg = o.notificationMessage
                    .Replace("{name}",ai.stats.name)
                    .Replace("{target}",FindNearest()?.stats.name ?? "biri")
                    .Replace("{spouse}",ai.stats.spouse?.stats.name ?? "")
                    .Replace("{rival}",FindNearestEnemy()?.stats.name ?? "rakip");
                NotificationManager.Instance.Show(msg, o.notificationType); break;

            case EventOutcome.OutcomeType.StealFrom:
                StealFrom(o.parameter); break;

            case EventOutcome.OutcomeType.Marry:
                MarryTarget(o.parameter); break;

            case EventOutcome.OutcomeType.Divorce:
                DivorceSpouse(); break;

            case EventOutcome.OutcomeType.HealMinion:
                HealTarget(o.parameter); break;

            case EventOutcome.OutcomeType.MakeLeader:
                PowerSystem.Instance.SetLeader(ai); break;

            case EventOutcome.OutcomeType.BecomeFriends:
                BecomeFriendsWithNearest(); break;

            case EventOutcome.OutcomeType.BecomeEnemies:
                BecomeEnemiesWithTarget(o.parameter); break;

            case EventOutcome.OutcomeType.FormFaction:
                social.FormFaction(o.parameter); break;
        }
    }

    // ── Yardımcı metodlar ────────────────────────────────

    void ChangeStat(string stat, float val)
    {
        switch(stat)
        {
            case "health":           ai.stats.health             = Mathf.Clamp(ai.stats.health + val, 0, 100); break;
            case "workSpeed":        ai.stats.workSpeed          = Mathf.Clamp(ai.stats.workSpeed + val, 0.1f, 3f); break;
            case "sociability":      ai.stats.sociability        = Mathf.Clamp(ai.stats.sociability + val, 0, 100); break;
            case "trustLevel":       ai.stats.trustLevel         = Mathf.Clamp(ai.stats.trustLevel + val, 0, 100); break;
            case "hoardingTendency": ai.stats.hoardingTendency  += val; break;
            case "fearThreshold":    ai.stats.fearThreshold       = Mathf.Clamp(ai.stats.fearThreshold + val, 0, 100); break;
            case "learningRate":     ai.stats.learningRate       += val; break;
        }
    }

    void AttackTarget(string targetType)
    {
        var t = GetTarget(targetType);
        if(t == null) return;
        t.stats.health -= 20f;
        memory.RecordEvent("Attacked", t.stats.name, 0.7f, true);
        cascade.TriggerCascade("Fight", t.stats.name, 0.5f);
    }

    void KillTarget(string targetType)
    {
        var t = GetTarget(targetType);
        if(t == null) return;
        t.stats.health = 0f;
        memory.RecordEvent("Murdered", t.stats.name, 1f, true);
        cascade.TriggerCascade("Murder", t.stats.name, 1f);
    }

    void StealFrom(string targetType)
    {
        var t = GetTarget(targetType);
        if(t == null) return;
        bool caught = Random.value < 0.3f;
        if(caught)
        {
            ai.stats.caughtStealing = true;
            social.UpdateRelationship(t.stats.name, -20f, "CaughtStealing");
            ai.stats.reputation -= 15f;
            memory.RecordEvent("CaughtStealing", t.stats.name, 0.8f, true);
        }
        else
        {
            ResourceManager.Instance.foodCount += 5;
        }
    }

    void MarryTarget(string targetType)
    {
        var t = GetTarget(targetType);
        if(t == null || t.stats.isMarried) return;
        ai.stats.isMarried = true; ai.stats.spouse = t;
        t.stats.isMarried  = true; t.stats.spouse  = ai;
        social.UpdateRelationship(t.stats.name, 30f, "Marriage");
        cascade.TriggerCascade("Marriage", t.stats.name, 0.3f);
    }

    void DivorceSpouse()
    {
        if(ai.stats.spouse == null) return;
        var sp = ai.stats.spouse;
        ai.stats.isMarried = false; ai.stats.spouse = null;
        sp.stats.isMarried = false; sp.stats.spouse = null;
        social.UpdateRelationship(sp.stats.name, -30f, "Divorce");
        cascade.TriggerCascade("Divorce", sp.stats.name, 0.5f);
    }

    void HealTarget(string targetType)
    {
        var t = targetType == "self" ? ai : GetTarget(targetType);
        if(t == null) return;
        bool smart = cards.HasCard("Zeka");
        bool mad   = cards.HasCard("Deli");
        if(mad && Random.value < 0.5f) { t.stats.health -= 10f; return; }
        float chance = smart ? 0.8f : 0.5f;
        if(Random.value < chance) { t.stats.health = Mathf.Min(100, t.stats.health + 30); t.stats.isSick = false; }
    }

    void BecomeFriendsWithNearest()
    {
        var t = FindNearest();
        if(t == null) return;
        social.UpdateRelationship(t.stats.name, 25f, "EventFriendship");
        t.GetComponent<SocialNetwork>().UpdateRelationship(ai.stats.name, 25f, "EventFriendship");
    }

    void BecomeEnemiesWithTarget(string targetType)
    {
        var t = GetTarget(targetType);
        if(t == null) return;
        social.UpdateRelationship(t.stats.name, -30f, "EventEnemy");
    }

    MinionAI GetTarget(string type)
    {
        switch(type)
        {
            case "spouse":        return ai.stats.spouse;
            case "spouse_cheater":return ai.stats.spouse?.stats.secretLover;
            case "nearest":       return FindNearest();
            case "nearest_enemy": return FindNearestEnemy();
            case "nearest_friend":return FindNearestFriend();
            case "nearest_sick":  return FindNearestSick();
            case "leader":        return PowerSystem.Instance.currentLeader;
            case "self":          return ai;
            default:              return FindByName(type);
        }
    }

    float GetResource(string r)
    {
        switch(r)
        {
            case "wood":  return ResourceManager.Instance.woodCount;
            case "food":  return ResourceManager.Instance.foodCount;
            case "stone": return ResourceManager.Instance.stoneCount;
            case "water": return ResourceManager.Instance.waterCount;
            default: return 0f;
        }
    }

    bool IsFightNearby()
    {
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(m == ai || !m.stats.isAlive) continue;
            if(m.currentState == "Fighting" && Vector3.Distance(transform.position, m.transform.position) < 10f)
                return true;
        }
        return false;
    }

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

    MinionAI FindNearestEnemy()
    {
        foreach(var e in ai.stats.enemies) if(e != null && e.stats.isAlive) return e;
        return null;
    }

    MinionAI FindNearestFriend()
    {
        foreach(var f in ai.stats.friends) if(f != null && f.stats.isAlive) return f;
        return null;
    }

    MinionAI FindNearestSick()
    {
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None)) if(m != ai && m.stats.isAlive && m.stats.isSick) return m;
        return null;
    }

    MinionAI FindByName(string name)
    {
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None)) if(m.stats.name == name) return m;
        return null;
    }
}
