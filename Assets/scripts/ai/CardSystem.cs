using System.Collections.Generic;
using UnityEngine;

// ============================================================
// CardSystem.cs
// Kart yönetimi — maksimum 3 kart/minyon
// Kart Listesi (Tam):
// KİŞİLİK: Gururlu, Şüpheci, Haset, Bencil, Sosyal, Romantik,
//           Çapkın, Kıskanç, Dedikoduncu, Lider, Koruyucu
// ZİHİNSEL: Yansıtıcı, Önsezili, RüyaGörür, Kör, Zeka, Deli, Sakin
// FİZİKSEL: Hızlı, ErkenKalkan, AğırUyuyan, KısaÖmürlü, UzunÖmürlü,
//            Oduncu, Avcı, İnşaatçı, Tembel, Maceracı, Korkak
// KOMİK:    Unutkan, Rekabetçi, AşırıSosyal, AşırıHevesli, Klumsy,
//            Mızmız, Sinir, Şaman, Hırsız
// ============================================================
public class CardSystem : MonoBehaviour
{
    public List<string> activeCards = new List<string>(); // maks 3
    public int maxCards = 3;

    // Doğuşta açık kartlar (oyun başı ilk 2'de kart yok, 3. minyondan itibaren)
    public static readonly List<string> StartCards = new List<string>
    {
        "Oduncu","Avcı","İnşaatçı","Sosyal","Zeka","Sinir","Koruyucu","Sakin"
    };

    // Keşfedilebilir kartlar (belirli olaylarla açılır)
    public static readonly List<string> DiscoverableCards = new List<string>
    {
        "Romantik","Çapkın","Kıskanç","Hırsız","Dedikoduncu","Lider","Korkak",
        "Maceracı","Tembel","Şaman","Deli","Gururlu","Şüpheci","Haset","Bencil",
        "Yansıtıcı","Önsezili","RüyaGörür","Kör","Hızlı","ErkenKalkan",
        "AğırUyuyan","KısaÖmürlü","UzunÖmürlü","Unutkan","Rekabetçi",
        "AşırıSosyal","AşırıHevesli","Klumsy","Mızmız"
    };

    public bool HasCard(string card) => activeCards.Contains(card);

    public bool AddCard(string card)
    {
        if(activeCards.Count >= maxCards) return false;
        if(activeCards.Contains(card)) return false;
        activeCards.Add(card);
        ApplyPassiveEffect(card);
        return true;
    }

    public void RemoveCard(string card) => activeCards.Remove(card);

    // Kart alındığında kalıcı stat değişimleri
    void ApplyPassiveEffect(string card)
    {
        var stats = GetComponent<MinionAI>().stats;
        switch(card)
        {
            case "Hızlı":        stats.workSpeed    += 0.3f;  break;
            case "Zeka":         stats.learningRate += 0.5f;  break;
            case "KısaÖmürlü":   stats.maxAge       *= 0.6f; stats.matingCooldown *= 0.4f; break;
            case "UzunÖmürlü":   stats.maxAge       *= 1.5f; stats.matingCooldown *= 2f;   break;
            case "Sosyal":       stats.sociability  += 20f;   break;
            case "Sinir":        stats.fearThreshold -= 10f;  break;
            case "Korkak":       stats.fearThreshold += 20f;  break;
            case "Tembel":       stats.workSpeed     -= 0.2f; break;
            case "Gururlu":      stats.trustLevel    -= 5f;   break;  // güvenmesi zor
            case "Bencil":       stats.sociability   -= 10f;  break;
            case "Klumsy":       stats.workSpeed     -= 0.1f; break;  // işi biraz yavaşlar
        }
    }

    // Kart kombinasyonu bonusu
    public float GetComboBonusWorkSpeed()
    {
        float bonus = 0f;
        if(HasCard("Zeka") && HasCard("İnşaatçı")) bonus += 0.2f;  // Zeka+İnşaatçı: daha iyi yapı
        if(HasCard("Avcı") && HasCard("Hızlı"))    bonus += 0.3f;  // Avcı+Hızlı: çok iyi avcı
        if(HasCard("Tembel") && HasCard("Zeka"))   bonus += 0.15f; // Tembel+Zeka: kısayol bulucu
        if(HasCard("AşırıHevesli") && HasCard("Sakin")) bonus += 0.1f; // beklenmedik uyum
        return bonus;
    }

    // Belirli olay tetiklenince kart keşfetme
    public void OnEventOccured(string eventType)
    {
        switch(eventType)
        {
            case "FirstMating":       TryDiscover("Romantik");    break;
            case "Cheating":          TryDiscover("Çapkın");      break;
            case "JealousReaction":   TryDiscover("Kıskanç");     break;
            case "Starvation":        TryDiscover("Hırsız");      break;
            case "Scandal":           TryDiscover("Dedikoduncu"); break;
            case "Population10":      TryDiscover("Lider");       break;
            case "AnimalAttack":      TryDiscover("Korkak");      break;
            case "MapExpands":        TryDiscover("Maceracı");    break;
            case "LongIdle":          TryDiscover("Tembel");      break;
            case "Disease":           TryDiscover("Şaman");       break;
            case "LongStarvationDeath": TryDiscover("Deli");      break;
            case "PublicHumiliation": TryDiscover("Gururlu");     break;
            case "BetrayalEvent":     TryDiscover("Şüpheci");     break;
            case "RivalSuccess":      TryDiscover("Haset");       break;
        }
    }

    void TryDiscover(string card)
    {
        if(!HasCard(card) && activeCards.Count < maxCards)
        {
            AddCard(card);
            NotificationManager.Instance.Show(
                GetComponent<MinionAI>().stats.name + " '" + card + "' kartını keşfetti!",
                NotificationType.Evolution);
        }
    }
}
