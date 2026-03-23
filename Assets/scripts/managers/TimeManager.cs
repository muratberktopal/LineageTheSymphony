using UnityEngine;

// ============================================================
// TimeManager.cs — Singleton
// Gece/gündüz ve mevsim döngüsü
// Sahnede tek bir GameObject'e eklenir
// ============================================================
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("Zaman")]
    public float dayDuration    = 120f;  // bir gün kaç gerçek saniye
    public float currentTime    = 0f;    // 0-1 arası (0=şafak, 0.5=gece yarısı)

    [Header("Mevsim")]
    public int   currentSeason  = 0;     // 0=İlkbahar 1=Yaz 2=Sonbahar 3=Kış
    public int   dayOfSeason    = 0;
    public int   daysPerSeason  = 10;
    public int   totalDays      = 0;

    [Header("Durum")]
    public bool isNight  = false;
    public bool isWinter = false;

    [Header("Hız")]
    public float timeScale = 1f;         // oyuncu 1x/2x/4x/8x seçer

    void Awake() { Instance = this; }

    void Update()
    {
        currentTime += (Time.deltaTime * timeScale) / dayDuration;

        if(currentTime >= 1f)
        {
            currentTime -= 1f;
            totalDays++;
            dayOfSeason++;

            if(dayOfSeason >= daysPerSeason)
            {
                dayOfSeason = 0;
                currentSeason = (currentSeason + 1) % 4;
                NotificationManager.Instance.Show(
                    SeasonName(currentSeason) + " başladı.", NotificationType.Nature);
            }
        }

        // 0.25-0.75 arası gündüz, geri kalanı gece
        isNight  = currentTime < 0.25f || currentTime > 0.75f;
        isWinter = currentSeason == 3;
    }

    public string SeasonName(int s)
    {
        switch(s)
        {
            case 0: return "İlkbahar";
            case 1: return "Yaz";
            case 2: return "Sonbahar";
            case 3: return "Kış";
            default: return "?";
        }
    }

    public string CurrentSeasonName() => SeasonName(currentSeason);

    // Oyuncu hız kontrolü
    public void SetSpeed(int speed)
    {
        switch(speed)
        {
            case 0: Time.timeScale = 0f; break;
            case 1: timeScale = 1f; Time.timeScale = 1f; break;
            case 2: timeScale = 2f; Time.timeScale = 1f; break;
            case 4: timeScale = 4f; Time.timeScale = 1f; break;
            case 8: timeScale = 8f; Time.timeScale = 1f; break;
        }
    }

    // 0-1 arası zaman dilimini okunabilir saate çevir
    public string GetTimeString()
    {
        int hour = Mathf.FloorToInt(currentTime * 24f);
        return hour.ToString("00") + ":00";
    }
}
