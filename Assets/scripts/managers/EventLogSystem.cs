using System.Collections.Generic;
using UnityEngine;

// ============================================================
// EventLogSystem.cs — Singleton
// Tüm köy olaylarını kaydeder
// UI'da gösterilebilir, oyun bittikten sonra köy tarihini çıkarır
// ============================================================
public class EventLogSystem : MonoBehaviour
{
    public static EventLogSystem Instance;

    [System.Serializable]
    public class LogEntry
    {
        public string          eventId;
        public string          minionName;
        public NotificationType type;
        public int             generation;
        public float           timestamp;
        public int             day;
    }

    public List<LogEntry> log     = new List<LogEntry>();
    public int            maxLog  = 500;

    void Awake() { Instance = this; }

    public void AddEntry(string eventId, string minionName, NotificationType type, int generation)
    {
        log.Insert(0, new LogEntry
        {
            eventId    = eventId,
            minionName = minionName,
            type       = type,
            generation = generation,
            timestamp  = Time.time,
            day        = TimeManager.Instance?.totalDays ?? 0
        });

        if(log.Count > maxLog)
            log.RemoveAt(log.Count - 1);
    }

    // Nesle göre özet
    public string GetGenerationSummary(int gen)
    {
        int count = 0;
        foreach(var e in log) if(e.generation == gen) count++;
        return $"{gen}. nesil: {count} önemli olay";
    }

    // Son N olayı getir
    public List<LogEntry> GetRecent(int count)
    {
        var result = new List<LogEntry>();
        for(int i = 0; i < Mathf.Min(count, log.Count); i++)
            result.Add(log[i]);
        return result;
    }

    // Tüm logu string olarak çıkar (debug için)
    public string DumpLog()
    {
        var sb = new System.Text.StringBuilder();
        foreach(var e in log)
            sb.AppendLine($"[Gün {e.day}][{e.type}] {e.minionName}: {e.eventId}");
        return sb.ToString();
    }
}
