using System.Collections.Generic;
using UnityEngine;

// ============================================================
// InventionManager.cs — Singleton
// Hangi icatların keşfedildiğini takip eder
// ============================================================
public class InventionManager : MonoBehaviour
{
    public static InventionManager Instance;

    // Keşfedilen icatlar: isim → keşfeden minyon
    public Dictionary<string,string> discovered = new Dictionary<string,string>();

    void Awake() { Instance = this; }

    public bool IsDiscovered(string item) => discovered.ContainsKey(item);

    public void RegisterInvention(string item, string discovererName)
    {
        if(discovered.ContainsKey(item)) return; // zaten var

        discovered[item] = discovererName;

        // Kaynak yöneticisini bilgilendir
        ResourceManager.Instance?.OnInvention(item);

        // Bildirim
        NotificationManager.Instance?.Show(
            discovererName + " '" + FriendlyName(item) + "' icat etti!",
            NotificationType.Surprising);

        // Log
        EventLogSystem.Instance?.AddEntry(
            "Invention_" + item, discovererName, NotificationType.Surprising, 0);
    }

    // İnsan okunabilir isimler
    string FriendlyName(string item)
    {
        switch(item)
        {
            case "FishingRod":   return "Olta";
            case "Trap":         return "Hayvan Tuzağı";
            case "Campfire":     return "Ateş";
            case "Farming":      return "Tarım";
            case "Wheel":        return "Tekerlek";
            case "Fermentation": return "Fermentasyon";
            case "Pottery":      return "Çömlek";
            case "BowAndArrow":  return "Yay ve Ok";
            case "Music":        return "Müzik";
            case "Medicine":     return "Tıp";
            case "Boat":         return "Sal";
            case "TradeSystem":  return "Takas Sistemi";
            case "Storage":      return "Depolama";
            case "EnergyDrink":  return "Enerji İçeceği";
            case "Writing":      return "Yazı";
            case "Irrigation":   return "Sulama";
            case "Metallurgy":   return "Metalürji";
            case "Calendar":     return "Takvim";
            case "LegalCode":    return "Hukuk Kanunları";
            case "Granary":      return "Tahıl Ambarı";
            case "Art":          return "Sanat";
            case "Monument":     return "Anıt";
            case "Rope":         return "İp";
            case "StoneKnife":   return "Taş Bıçak";
            case "Bread":        return "Ekmek";
            case "FishingNet":   return "Balık Ağı";
            case "Door":         return "Kapı";
            case "Smoking":      return "Tütsüleme";
            case "DryingRack":   return "Kurutma Rafı";
            case "Livestock":    return "Evcil Hayvan";
            case "Map":          return "Harita";
            case "Astronomy":    return "Astronomi";
            case "CropRotation": return "Tarla Rotasyonu";
            case "SoundSystem":  return "Ses Sistemi";
            case "CreationMyth": return "Yaratılış Miti";
            default: return item;
        }
    }

    // Keşif ağacı — hangi icat hangisini açar
    public List<string> GetUnlocked(string item)
    {
        switch(item)
        {
            case "Campfire":     return new List<string>{"Bread","Smoking","Metallurgy"};
            case "FishingRod":   return new List<string>{"FishingNet"};
            case "Farming":      return new List<string>{"CropRotation","Granary"};
            case "Writing":      return new List<string>{"LegalCode","CreationMyth","Calendar"};
            case "TradeSystem":  return new List<string>{"Map"};
            default:             return new List<string>();
        }
    }
}
