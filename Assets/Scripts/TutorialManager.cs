using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public Image tutorialImage;         // Tempat nampilin sprite tutorial
    public Button nextButton;           // Tombol ke kanan
    public Button prevButton;           // Tombol ke kiri
    public Text pageCounterText;        // (Opsional) Text "Page X / Y"

    [Header("Tutorial Pages")]
    public Sprite[] tutorialPages;      // Isi dengan sprite tutorial kamu

    private int currentPage = 0;

    private void Start()
    {
        if (tutorialPages == null || tutorialPages.Length == 0)
        {
            Debug.LogError("❌ Tidak ada sprite tutorial yang di-assign!");
            return;
        }

        UpdatePage();

        // Assign listener ke tombol
        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
    }

    public void NextPage()
    {
        if (tutorialPages.Length == 0) return;

        currentPage++;
        if (currentPage >= tutorialPages.Length)
            currentPage = tutorialPages.Length - 1; // Stop di terakhir

        UpdatePage();
    }

    public void PrevPage()
    {
        if (tutorialPages.Length == 0) return;

        currentPage--;
        if (currentPage < 0)
            currentPage = 0; // Stop di awal

        UpdatePage();
    }

    private void UpdatePage()
    {
        tutorialImage.sprite = tutorialPages[currentPage];

        // Update counter text kalau ada
        if (pageCounterText != null)
            pageCounterText.text = $"Page {currentPage + 1} / {tutorialPages.Length}";

        // Disable tombol kalau di ujung
        if (prevButton != null)
            prevButton.interactable = currentPage > 0;

        if (nextButton != null)
            nextButton.interactable = currentPage < tutorialPages.Length - 1;
    }
}
