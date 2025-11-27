using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public string sceneToLoad = "escena1"; // Cambia esto por el nombre de tu escena jugable

    public void PlayGame()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoadGame()
    {
        // Cargar escena y datos guardados
        if (PlayerPrefs.HasKey("SavedScene"))
        {
            string savedScene = PlayerPrefs.GetString("SavedScene");
            SceneManager.LoadScene(savedScene);
        }
        else
        {
            Debug.LogWarning("No hay partida guardada.");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
