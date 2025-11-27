using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public void SaveGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedScene", currentScene);
        PlayerPrefs.Save();

        Debug.Log("Partida guardada en: " + currentScene);
    }
}
