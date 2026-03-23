using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ============================================================
// MemorySystem.cs
// Olayları kaydeder, travma aktarımı yapar, eşik sürelerini tutar
// ============================================================
public class MemorySystem : MonoBehaviour
{
    MinionAI ai;

    [System.Serializable]
    public class Memory
    {
        public string eventType;
        public string involvedMinion;
        public float  timestamp;
        public float  emotionalWeight; // 0-1
        public bool   isTrauma;
        public string details;
    }

    public List<Memory>   memories         = new List<Memory>();
    public List<string>   inheritedTraumas  = new List<string>();
    public int            maxMemories       = 100;

    // Duygu başlangıç zamanları
    Dictionary<string,float> emoStart = new Dictionary<string,float>();

    // İcat/encounter sistemi için eşik süre sayaçları
    public Dictionary<string,float> thresholds = new Dictionary<string,float>();

    void Start()
    {
        ai = GetComponent<MinionAI>();
        InvokeRepeating(nameof(UpdateThresholds), 0f, 5f);
        InvokeRepeating(nameof(DecayTraumas),     0f, 30f);
    }

    public void RecordEvent(string type, string minion, float weight, bool trauma = false, string details = "")
    {
        memories.Add(new Memory
        {
            eventType = type, involvedMinion = minion,
            timestamp = Time.time, emotionalWeight = weight,
            isTrauma  = trauma, details = details
        });

        // Eski hafızaları temizle (travmalar kalır)
        if(memories.Count > maxMemories)
        {
            var oldest = memories.Where(m => !m.isTrauma).OrderBy(m => m.timestamp).FirstOrDefault();
            if(oldest != null) memories.Remove(oldest);
        }

        if(trauma && weight > 0.8f) ProcessHeavyTrauma(type);
    }

    public void RecordEmotionalEvent(string emo, float intensity)
    {
        if(!emoStart.ContainsKey(emo)) emoStart[emo] = Time.time;
        RecordEvent("EmotionalState", "", intensity, false, emo);
    }

    public float GetEmotionDuration(string emo)
        => emoStart.ContainsKey(emo) ? Time.time - emoStart[emo] : 0f;

    void UpdateThresholds()
    {
        var needs  = GetComponent<NeedsSystem>();
        var emos   = GetComponent<EmotionSystem>();
        if(needs == null) return;

        Set("hunger_high",   needs.hunger     > 70f);
        Set("fatigue_high",  needs.fatigue    > 70f);
        Set("cold_long",     needs.coldness   > 60f);
        Set("lonely_long",   needs.loneliness > 70f);
        Set("stress_high",   needs.stress     > 60f);
        Set("idle_long",     ai.currentState  == "Idle");
        Set("vengeful_state",emos.IsPlottingRevenge());
        Set("humiliated_state", emos.GetI("Humiliated") > 0.5f);
        Set("wood_excess",   ResourceManager.Instance.woodCount  > 50);
        Set("food_excess",   ResourceManager.Instance.foodCount  > 30);
    }

    void Set(string key, bool condition)
    {
        if(!thresholds.ContainsKey(key)) thresholds[key] = 0f;
        thresholds[key] = condition ? thresholds[key] + 5f : 0f;
    }

    public float GetThreshold(string key)
        => thresholds.ContainsKey(key) ? thresholds[key] : 0f;

    void ProcessHeavyTrauma(string type)
    {
        switch(type)
        {
            case "FamilyDeath":     ai.stats.baseGriefLevel  += 10; ai.stats.hasLostFamily    = true; break;
            case "Starvation":      ai.stats.hoardingTendency+= 20; break;
            case "Attacked":        ai.stats.fearThreshold   -= 15; ai.stats.hasBeenAttacked  = true; break;
            case "PublicHumiliation": ai.stats.sociability   -= 15; ai.stats.hasBeenHumiliated= true; break;
            case "Betrayal":        ai.stats.trustLevel      -= 25; break;
        }
    }

    void DecayTraumas()
    {
        foreach(var m in memories.Where(x => x.isTrauma))
            m.emotionalWeight = Mathf.Max(0.1f, m.emotionalWeight - 0.01f);
    }

    // Doğumda çağrılır
    public void InheritTraumas(MemorySystem p1, MemorySystem p2)
    {
        foreach(var src in new[]{p1, p2})
        {
            foreach(var m in src.memories.Where(x => x.isTrauma && x.emotionalWeight > 0.7f))
            {
                if(inheritedTraumas.Contains(m.eventType)) continue;
                inheritedTraumas.Add(m.eventType);
                ApplyInherited(m.eventType, m.emotionalWeight * 0.4f);
            }
        }
    }

    void ApplyInherited(string type, float w)
    {
        switch(type)
        {
            case "Starvation":        ai.stats.hoardingTendency += w * 15; break;
            case "Attacked":          ai.stats.fearThreshold    -= w * 10; break;
            case "PublicHumiliation": ai.stats.sociability      -= w * 10; break;
            case "Betrayal":          ai.stats.trustLevel       -= w * 15; break;
        }
        NotificationManager.Instance.Show(
            ai.stats.name + " geçmiş travma izleri taşıyor.", NotificationType.Evolution);
    }

    public bool HasExperienced(string type) => memories.Any(m => m.eventType == type);

    public List<Memory> About(string minion)
        => memories.Where(m => m.involvedMinion == minion).ToList();
}
