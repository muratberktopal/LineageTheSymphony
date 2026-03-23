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
        // --- DEBUG: KLAVYE KISAYOLLARI ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetSpeed(1); // Normal Hız
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetSpeed(2); // 2x Hız
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetSpeed(4); // 4x Hız
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetSpeed(8); // 8x Hız
        if (Input.GetKeyDown(KeyCode.Space)) SetSpeed(Time.timeScale == 0 ? 1 : 0); // Durdur / Devam et

        // --- ORİJİNAL ZAMAN AKIŞI ---
        // timeScale yerine Time.timeScale kullanmıyoruz çünkü Update zaten Time.timeScale'den etkilenen Time.deltaTime ile çalışıyor.
        // O yüzden formülü normal deltaTime ile sadeleştirdik.
        currentTime += Time.deltaTime / dayDuration;

        if (currentTime >= 1f)
        {
            currentTime -= 1f;
            totalDays++;
            dayOfSeason++;

            if (dayOfSeason >= daysPerSeason)
            {
                dayOfSeason = 0;
                currentSeason = (currentSeason + 1) % 4;
                NotificationManager.Instance.Show(
                    SeasonName(currentSeason) + " başladı.", NotificationType.Nature);
            }
        }

        // 0.25-0.75 arası gündüz, geri kalanı gece
        isNight = currentTime < 0.25f || currentTime > 0.75f;
        isWinter = currentSeason == 3;
    }

    // Oyuncu veya Debugger hız kontrolü
    public void SetSpeed(int speed)
    {
        // Time.timeScale Unity'nin fizik, NavMeshAgent ve Invoke sürelerini otomatik hızlandırır.
        switch (speed)
        {
            case 0: Time.timeScale = 0f; timeScale = 0f; break;
            case 1: Time.timeScale = 1f; timeScale = 1f; break;
            case 2: Time.timeScale = 2f; timeScale = 2f; break; // Düzeltildi!
            case 4: Time.timeScale = 4f; timeScale = 4f; break; // Düzeltildi!
            case 8: Time.timeScale = 8f; timeScale = 8f; break; // Düzeltildi!
        }

        Debug.Log("Oyun Hızı Değiştirildi: " + speed + "x");
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


    // 0-1 arası zaman dilimini okunabilir saate çevir
    public string GetTimeString()
    {
        int hour = Mathf.FloorToInt(currentTime * 24f);
        return hour.ToString("00") + ":00";
    }
}
