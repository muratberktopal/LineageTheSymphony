using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MinionFeedback : MonoBehaviour
{
    public Sprite heartEmoji;   // Aþk/Üreme için
    public Sprite skullEmoji;   // Ölüm/Tehlike için
    public Sprite sickEmoji;    // Zehirlenme için
    public Sprite woodEmoji;    // Odun keserken
    public Sprite fishEmoji;    // Balýk tutarken

    public void TriggerEmoji(string type)
    {
        Sprite toShow = null;
        switch (type)
        {
            case "Love": toShow = heartEmoji; break;
            case "Dead": toShow = skullEmoji; break;
            case "Sick": toShow = sickEmoji; break;
            case "Wood": toShow = woodEmoji; break;
            case "Fish": toShow = fishEmoji; break;
        }

        if (toShow != null) ShowEmoji(toShow);
    }
    public TextMeshProUGUI statusText;
    public Image emojiImage;
    public float displayDuration = 2.5f;

    private float timer = 0f;

    void Start()
    {
        // Baþlangýçta her þeyi gizle
        statusText.gameObject.SetActive(false);
        emojiImage.gameObject.SetActive(false);
    }

    public void ShowText(string text, Color color)
    {
        statusText.text = text;
        statusText.color = color;
        statusText.gameObject.SetActive(true);
        timer = displayDuration;
    }

    public void ShowEmoji(Sprite emoji)
    {
        emojiImage.sprite = emoji;
        emojiImage.gameObject.SetActive(true);
        timer = displayDuration;
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            // Hafifçe yukarý süzülme efekti
            transform.Translate(Vector3.up * Time.deltaTime * 0.2f);

            if (timer <= 0)
            {
                statusText.gameObject.SetActive(false);
                emojiImage.gameObject.SetActive(false);
                // Pozisyonu resetle
                transform.localPosition = new Vector3(0, 2.2f, 0);
            }
        }
    }
}