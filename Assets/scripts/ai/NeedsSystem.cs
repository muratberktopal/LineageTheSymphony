using System.Collections.Generic;
using UnityEngine;

// ============================================================
// NeedsSystem.cs
// 10 ihtiyacı takip eder, eşik geçilince duyguyu tetikler
// ============================================================
public class NeedsSystem : MonoBehaviour
{
    MinionAI        ai;
    EmotionSystem   emotions;
    CardSystem      cards;

    [Header("Fiziksel")]
    public float hunger    = 0f;   // 0=tok  100=ölüm eşiği
    public float fatigue   = 0f;   // 0=dinç 100=çöküş
    public float coldness  = 0f;
    public float pain      = 0f;

    [Header("Psikolojik")]
    public float loneliness = 0f;
    public float fear       = 0f;
    public float boredom    = 0f;
    public float curiosity  = 0f;

    [Header("Sosyal")]
    public float disrespect = 0f;
    public float injustice  = 0f;
    public float stress     = 0f;  // genel stres — birden çok ihtiyaçtan beslenir

    // Eşikler
    readonly Dictionary<string,float> warn = new Dictionary<string,float>
    {{"hunger",50},{"fatigue",60},{"coldness",40},{"pain",30},
     {"loneliness",70},{"fear",50},{"boredom",80},{"curiosity",75},
     {"disrespect",60},{"injustice",50}};

    readonly Dictionary<string,float> crit = new Dictionary<string,float>
    {{"hunger",80},{"fatigue",90},{"coldness",70},{"pain",70},
     {"loneliness",90},{"fear",80},{"boredom",95},{"curiosity",95},
     {"disrespect",85},{"injustice",80}};

    void Start()
    {
        ai       = GetComponent<MinionAI>();
        emotions = GetComponent<EmotionSystem>();
        cards    = GetComponent<CardSystem>();
        InvokeRepeating(nameof(Tick),    0f, 1f);
        InvokeRepeating(nameof(CheckThresholds), 0f, 2f);
    }

    void Tick()
    {
        if (!ai.stats.isAlive) return;

        // --- AÇLIK ---
        hunger += 0.8f;
        if (ai.currentState == "Eating")    hunger  = Mathf.Max(0, hunger  - 15f);
        if (cards.HasCard("Bencil"))        hunger += 0.1f; // biriktirdiğinden hesaplıyor, daha çok yer

        // --- YORGUNLUK ---
        if (ai.currentState == "Sleeping") fatigue  = Mathf.Max(0, fatigue - 10f);
        else                               fatigue += 0.5f;
        if (cards.HasCard("Hızlı"))        fatigue += 0.3f;
        if (cards.HasCard("ErkenKalkan") && TimeManager.Instance.isNight)  fatigue -= 0.1f; // gece aktif, gündüz uyar
        if (cards.HasCard("AğırUyuyan")  && !TimeManager.Instance.isNight) fatigue += 0.2f; // gündüz daha çabuk yorulur

        // --- SOĞUK ---
        if (TimeManager.Instance.isWinter && !ai.stats.isAlive) coldness += 2f;
        else if (ai.currentState == "AtCampfire") coldness = Mathf.Max(0, coldness - 5f);
        else coldness = Mathf.Max(0, coldness - 0.3f);

        // --- ACI ---
        if (ai.stats.isInjured) pain = Mathf.Min(100, pain + 0.3f);
        else                    pain = Mathf.Max(0,   pain - 0.8f);

        // --- YALNIZLIK ---
        int nearby = NearbyCount(8f);
        if (nearby == 0) loneliness += cards.HasCard("Sosyal") || cards.HasCard("AşırıSosyal") ? 2f : 1.5f;
        else             loneliness  = Mathf.Max(0, loneliness - nearby * 0.5f);
        if (cards.HasCard("Yansıtıcı"))
        {
            bool sameCardNearby = HasSameCardNearby();
            loneliness = sameCardNearby ? Mathf.Max(0, loneliness - 2f) : loneliness + 0.5f;
        }

        // --- KORKU ---
        if (DangerNearby()) fear += cards.HasCard("Korkak") ? 5f : 3f;
        else                fear  = Mathf.Max(0, fear - 1f);
        if (cards.HasCard("Önsezili") && DangerNearby()) fear += 1.5f; // tehlikeyi önceden hisseder

        // --- SIKINTI ---
        if (ai.currentState == ai.lastState) boredom += cards.HasCard("Maceracı") ? 0.5f : 0.3f;
        else                                 boredom  = Mathf.Max(0, boredom - 2f);
        if (cards.HasCard("Rekabetçi") && NoRivalNearby()) boredom += 0.4f;
        if (cards.HasCard("AşırıHevesli")) boredom = Mathf.Max(0, boredom - 0.2f); // kolay sıkılmaz

        // Sıkılma merakı tetikler
        if (boredom > 60f) curiosity = Mathf.Min(100, curiosity + 0.5f);

        // --- SAYGI / ADALETSIZLIK ---
        if (ai.stats.reputation < 30) disrespect += 0.8f;
        else                          disrespect  = Mathf.Max(0, disrespect - 0.3f);
        if (ai.stats.wasWronged) injustice = Mathf.Min(100, injustice + 1f);
        else                     injustice = Mathf.Max(0,   injustice - 0.3f);

        // --- GENEL STRES ---
        stress = (hunger * 0.3f + fatigue * 0.2f + fear * 0.3f + injustice * 0.2f) / 1f;
        stress = Mathf.Clamp(stress, 0, 100);

        // Tümünü sınırla
        Clamp();
    }

    void CheckThresholds()
    {
        Check("hunger",    hunger,    "Anxious",      "Desperate");
        Check("fatigue",   fatigue,   "Tired",        "Exhausted");
        Check("coldness",  coldness,  "Uncomfortable","Freezing");
        Check("pain",      pain,      "Suffering",    "Agony");
        Check("loneliness",loneliness,"Sad",          "Depressed");
        Check("fear",      fear,      "Nervous",      "Terrified");
        Check("boredom",   boredom,   "Restless",     "Desperate_Boredom");
        Check("disrespect",disrespect,"Resentful",    "Humiliated");
        Check("injustice", injustice, "Bitter",       "Vengeful");
    }

    void Check(string key, float val, string warnEmo, string critEmo)
    {
        if (val >= crit[key]) emotions.SetEmotion(critEmo, 1f);
        else if (val >= warn[key]) emotions.SetEmotion(warnEmo, 0.6f);
    }

    public float TotalSuffering()
        => (hunger+fatigue+coldness+pain+loneliness+fear+disrespect+injustice) / 8f;

    public string MostUrgent()
    {
        var d = new Dictionary<string,float>
        {{"hunger",hunger},{"fatigue",fatigue},{"coldness",coldness},{"pain",pain},
         {"loneliness",loneliness},{"fear",fear},{"boredom",boredom},{"disrespect",disrespect},{"injustice",injustice}};
        string best = "hunger"; float max = 0;
        foreach(var kv in d) if(kv.Value > max){max = kv.Value; best = kv.Key;}
        return best;
    }

    // Dışarıdan erişim (encounter sistemi için)
    public float Get(string need)
    {
        switch(need)
        {
            case "hunger":     return hunger;
            case "fatigue":    return fatigue;
            case "coldness":   return coldness;
            case "pain":       return pain;
            case "loneliness": return loneliness;
            case "fear":       return fear;
            case "boredom":    return boredom;
            case "curiosity":  return curiosity;
            case "disrespect": return disrespect;
            case "injustice":  return injustice;
            case "stress":     return stress;
            default: return 0f;
        }
    }

    public void Change(string need, float delta)
    {
        switch(need)
        {
            case "hunger":     hunger     = Clamp0(hunger     + delta); break;
            case "fatigue":    fatigue    = Clamp0(fatigue    + delta); break;
            case "coldness":   coldness   = Clamp0(coldness   + delta); break;
            case "pain":       pain       = Clamp0(pain       + delta); break;
            case "loneliness": loneliness = Clamp0(loneliness + delta); break;
            case "fear":       fear       = Clamp0(fear       + delta); break;
            case "boredom":    boredom    = Clamp0(boredom    + delta); break;
            case "disrespect": disrespect = Clamp0(disrespect + delta); break;
            case "injustice":  injustice  = Clamp0(injustice  + delta); break;
            case "health":     ai.stats.health = Mathf.Clamp(ai.stats.health + delta, 0, 100); break;
        }
    }

    float Clamp0(float v) => Mathf.Clamp(v, 0, 100);

    void Clamp()
    {
        hunger=Clamp0(hunger); fatigue=Clamp0(fatigue); coldness=Clamp0(coldness);
        pain=Clamp0(pain); loneliness=Clamp0(loneliness); fear=Clamp0(fear);
        boredom=Clamp0(boredom); curiosity=Clamp0(curiosity);
        disrespect=Clamp0(disrespect); injustice=Clamp0(injustice);
    }

    int NearbyCount(float r)
    {
        int c = 0;
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
            if(m != ai && m.stats.isAlive && Vector3.Distance(transform.position, m.transform.position) < r) c++;
        return c;
    }

    bool DangerNearby() =>
        Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Danger")).Length > 0;

    bool HasSameCardNearby()
    {
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(m == ai || !m.stats.isAlive) continue;
            if(Vector3.Distance(transform.position, m.transform.position) > 8f) continue;
            foreach(var card in cards.activeCards)
                if(m.GetComponent<CardSystem>().HasCard(card)) return true;
        }
        return false;
    }

    bool NoRivalNearby()
    {
        foreach(var m in FindObjectsByType<MinionAI>(FindObjectsSortMode.None))
        {
            if(m == ai || !m.stats.isAlive) continue;
            if(Vector3.Distance(transform.position, m.transform.position) < 10f) return false;
        }
        return true;
    }
}
