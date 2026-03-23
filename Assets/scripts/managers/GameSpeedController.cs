using UnityEngine;

// ============================================================
// GameSpeedController.cs
// Baþka hiçbir scriptten etkilenmeyen, kaba kuvvet hýz kontrolü.
// ============================================================
public class GameSpeedController : MonoBehaviour
{
    void Update()
    {
        // Klavyenin üstündeki rakam tuþlarý
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetGameSpeed(1f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetGameSpeed(2f);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetGameSpeed(4f);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetGameSpeed(8f);
        if (Input.GetKeyDown(KeyCode.Alpha0)) SetGameSpeed(0f); // Oyunu dondur
    }

    void SetGameSpeed(float speed)
    {
        Time.timeScale = speed;

        // Konsola yazdýrýyoruz ki tuþa basabildik mi, klavye mi bozuk anlayalým
        Debug.Log(">>> OYUN HIZI DEÐÝÞTÝ: " + speed + "x <<<");
    }
}
