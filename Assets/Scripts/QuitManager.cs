using UnityEngine;

public class QuitManager : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("🚪 Quit button pressed.");

#if UNITY_EDITOR
        // Kalau lagi di Editor, cuma tampilkan pesan (biar gak nutup editor)
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Kalau di build (Windows, Android, dll), keluar beneran
        Application.Quit();
#endif
    }
}
