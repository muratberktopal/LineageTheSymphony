using System.Collections.Generic;
using UnityEngine;

// ============================================================
// EmotionSystem.cs
// 16 duygu, her biri davranışa modifier uygular
// ============================================================
public class EmotionSystem : MonoBehaviour
{
    MinionAI      ai;
    MemorySystem  memory;

    // Aktif duygular: isim → yoğunluk (0-1)
    public Dictionary<string,float> activeEmotions = new Dictionary<string,float>();

    [System.Serializable]
    public class EmotionEffect
    {
        public float workSpeedMod    = 1f;
        public float socialMod       = 1f;
        public float aggressionMod   = 1f;
        public float fleeChance      = 0f;
        public float stealChance     = 0f;
        public float gossipMod       = 1f;
        public float explorationMod  = 1f;
        public float sabotageChance  = 0f;
        public float compensationMod = 1f;
        public float leadershipMod   = 1f;
        public float hoardingMod     = 1f;
        public bool  planRevenge     = false;
        public bool  isolationSeek   = false;
        public bool  canRefuseWork   = false;
        public float randomActChance = 0f;
    }

    static readonly Dictionary<string,EmotionEffect> FX = new Dictionary<string,EmotionEffect>
    {
        {"Happy",         new EmotionEffect{workSpeedMod=1.2f, socialMod=1.3f, aggressionMod=0.5f}},
        {"Sad",           new EmotionEffect{workSpeedMod=0.7f, socialMod=0.6f, aggressionMod=0.8f}},
        {"Angry",         new EmotionEffect{workSpeedMod=1.4f, socialMod=0.3f, aggressionMod=2f}},
        {"Terrified",     new EmotionEffect{workSpeedMod=0f,   socialMod=0f,   aggressionMod=0f, fleeChance=1f}},
        {"Desperate",     new EmotionEffect{workSpeedMod=1.5f, socialMod=0f,   aggressionMod=1.5f, stealChance=0.3f}},
        {"Vengeful",      new EmotionEffect{workSpeedMod=0.6f, socialMod=0f,   aggressionMod=3f, planRevenge=true}},
        {"Humiliated",    new EmotionEffect{workSpeedMod=0.5f, socialMod=0f,   aggressionMod=2f, isolationSeek=true}},
        {"Depressed",     new EmotionEffect{workSpeedMod=0.3f, socialMod=0f,   aggressionMod=0.2f, canRefuseWork=true}},
        {"Anxious",       new EmotionEffect{workSpeedMod=0.8f, socialMod=0.7f, aggressionMod=0.9f, hoardingMod=1.5f}},
        {"Euphoric",      new EmotionEffect{workSpeedMod=1.5f, socialMod=1.8f, aggressionMod=0.3f, randomActChance=0.2f}},
        {"Bitter",        new EmotionEffect{workSpeedMod=0.7f, socialMod=0.4f, aggressionMod=1.3f, gossipMod=2f}},
        {"Restless",      new EmotionEffect{workSpeedMod=0.6f, socialMod=1.1f, aggressionMod=1.1f, explorationMod=2f}},
        {"Jealous",       new EmotionEffect{workSpeedMod=0.8f, socialMod=0f,   aggressionMod=1.8f, sabotageChance=0.15f}},
        {"Guilty",        new EmotionEffect{workSpeedMod=1.1f, socialMod=0.5f, aggressionMod=0.3f, compensationMod=2f}},
        {"Proud",         new EmotionEffect{workSpeedMod=1.3f, socialMod=1.2f, aggressionMod=0.7f, leadershipMod=1.5f}},
        {"Hopeful",       new EmotionEffect{workSpeedMod=1.1f, socialMod=1.1f, aggressionMod=0.6f}},
        {"Brave",         new EmotionEffect{workSpeedMod=1.2f, socialMod=1f,   aggressionMod=1.2f, fleeChance=0f}},
        {"Peaceful",      new EmotionEffect{workSpeedMod=1f,   socialMod=1.2f, aggressionMod=0.2f}},
        {"Surprised",     new EmotionEffect{workSpeedMod=0.9f, socialMod=1f,   aggressionMod=0.8f}},
        {"Tired",         new EmotionEffect{workSpeedMod=0.6f, socialMod=0.7f, aggressionMod=0.7f}},
        {"Exhausted",     new EmotionEffect{workSpeedMod=0.2f, socialMod=0.3f, aggressionMod=0.4f}},
        {"Hopeless",      new EmotionEffect{workSpeedMod=0.2f, socialMod=0.2f, aggressionMod=0.5f, canRefuseWork=true}},
    };

    static readonly Dictionary<string,string> Opposites = new Dictionary<string,string>
    {
        {"Happy","Sad"},{"Sad","Happy"},{"Angry","Peaceful"},{"Proud","Humiliated"},
        {"Humiliated","Proud"},{"Guilty","Justified"},{"Terrified","Brave"},
        {"Desperate","Hopeful"},{"Vengeful","Peaceful"},{"Depressed","Euphoric"}
    };

    void Start()
    {
        ai     = GetComponent<MinionAI>();
        memory = GetComponent<MemorySystem>();
        SetEmotion("Happy", 0.5f);
        InvokeRepeating(nameof(Decay),       0f, 3f);
        InvokeRepeating(nameof(Interactions), 0f, 5f);
    }

    public void SetEmotion(string emo, float intensity)
    {
        if (!activeEmotions.ContainsKey(emo)) activeEmotions[emo] = 0f;
        activeEmotions[emo] = Mathf.Min(1f, activeEmotions[emo] + intensity);
        if (intensity > 0.5f) memory.RecordEmotionalEvent(emo, intensity);
        if (Opposites.ContainsKey(emo) && activeEmotions.ContainsKey(Opposites[emo]))
            activeEmotions[Opposites[emo]] *= 0.5f;
    }

    void Decay()
    {
        var keys = new List<string>(activeEmotions.Keys);
        foreach(var k in keys)
        {
            activeEmotions[k] -= 0.05f;
            if(activeEmotions[k] <= 0) activeEmotions.Remove(k);
        }
    }

    void Interactions()
    {
        // Uzun üzüntü → depresyon
        if(GetI("Sad") > 0.7f && memory.GetEmotionDuration("Sad") > 300f) SetEmotion("Depressed", 0.3f);
        // Öfke + acı → intikam
        if(GetI("Angry") > 0.6f && GetI("Bitter") > 0.5f) SetEmotion("Vengeful", 0.4f);
        // Kıskançlık → sabotaj isteği
        if(GetI("Jealous") > 0.7f) ai.stats.consideringSabotage = true;
        // Gurur kırılması → aşağılanma
        if(GetI("Proud") > 0.7f && ai.stats.reputation < 25f) SetEmotion("Humiliated", 0.4f);
        // Çok mutlu + tembel → rastgele davranış
        if(GetI("Euphoric") > 0.8f && GetComponent<CardSystem>().HasCard("Tembel"))
            SetEmotion("Restless", 0.2f);
    }

    public float GetI(string emo) => activeEmotions.ContainsKey(emo) ? activeEmotions[emo] : 0f;

    public string Dominant()
    {
        string d = "Neutral"; float h = 0f;
        foreach(var kv in activeEmotions) if(kv.Value > h){h = kv.Value; d = kv.Key;}
        return d;
    }

    public float WorkSpeed()   => Apply(e => e.workSpeedMod);
    public float Aggression()  => Apply(e => e.aggressionMod);
    public float SocialMod()   => Apply(e => e.socialMod);
    public bool  ShouldFlee()  => activeEmotions.ContainsKey("Terrified") && activeEmotions["Terrified"] > 0.6f;
    public bool  IsPlottingRevenge() => activeEmotions.ContainsKey("Vengeful") && activeEmotions["Vengeful"] > 0.5f && FX["Vengeful"].planRevenge;
    public bool  CanRefuseWork()     => GetI("Depressed") > 0.6f || GetI("Hopeless") > 0.5f;

    float Apply(System.Func<EmotionEffect,float> selector)
    {
        float r = 1f;
        foreach(var kv in activeEmotions)
        {
            if(!FX.ContainsKey(kv.Key)) continue;
            r *= Mathf.Lerp(1f, selector(FX[kv.Key]), kv.Value);
        }
        return r;
    }
}
