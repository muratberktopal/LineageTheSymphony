using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MinionFeedback : MonoBehaviour
{
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