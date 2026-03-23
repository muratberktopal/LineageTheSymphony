using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
// NotificationManager.cs — Singleton
// Ekrana bildirim kuyruğu gösterir
// Canvas/UI yoksa sadece Debug.Log atar (demo uyumlu)
// ============================================================
public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("UI (Opsiyonel — demo'da boş bırakılabilir)")]
    public Transform notificationParent;   // bildirimlerin spawn noktası
    public GameObject notificationPrefab;  // Text + arka plan prefab

    [Header("Ayarlar")]
    public int   maxVisible   = 5;
    public float displayTime  = 4f;
    public float fadeTime     = 0.5f;

    Queue<(string msg, NotificationType type)> pending = new Queue<(string,NotificationType)>();
    List<GameObject>  active  = new List<GameObject>();
    float             timer   = 0f;

    // Renk eşlemeleri
    static readonly Dictionary<NotificationType,Color> Colors = new Dictionary<NotificationType,Color>
    {
        {NotificationType.Social,    new Color(0.8f, 0.9f, 1f)},
        {NotificationType.Drama,     new Color(1f,   0.6f, 0.2f)},
        {NotificationType.Conflict,  new Color(1f,   0.2f, 0.2f)},
        {NotificationType.Surprising,new Color(0.9f, 1f,   0.4f)},
        {NotificationType.Major,     new Color(1f,   0.8f, 0f)},
        {NotificationType.Death,     new Color(0.5f, 0.5f, 0.5f)},
        {NotificationType.Evolution, new Color(0.4f, 1f,   0.8f)},
        {NotificationType.Religion,  new Color(0.9f, 0.7f, 1f)},
        {NotificationType.Nature,    new Color(0.5f, 1f,   0.5f)},
        {NotificationType.Comedy,    new Color(1f,   1f,   0.6f)},
    };

    void Awake() { Instance = this; }

    void Update()
    {
        // Kuyruktan al
        if(pending.Count > 0 && active.Count < maxVisible)
        {
            timer -= Time.deltaTime;
            if(timer <= 0f)
            {
                var (msg, type) = pending.Dequeue();
                SpawnNotification(msg, type);
                timer = 0.3f; // arka arkaya çıkma süresi
            }
        }
    }

    public void Show(string message, NotificationType type)
    {
        // Her zaman log at (debug için)
        Debug.Log($"[{type}] {message}");

        // UI varsa kuyruğa ekle
        if(notificationPrefab != null && notificationParent != null)
            pending.Enqueue((message, type));
    }

    void SpawnNotification(string message, NotificationType type)
    {
        if(notificationPrefab == null || notificationParent == null) return;

        var go  = Instantiate(notificationPrefab, notificationParent);
        active.Add(go);

        // Metin
        var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if(tmp != null) tmp.text = message;

        // Renk
        var img = go.GetComponent<Image>();
        if(img != null && Colors.ContainsKey(type))
            img.color = Colors[type];

        // Otomatik sil
        Destroy(go, displayTime + fadeTime);
        active.RemoveAll(x => x == null);
    }
}
