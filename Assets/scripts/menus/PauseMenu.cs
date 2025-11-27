using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem; // 👈 NUEVO

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public Slider volumeSlider;
    public TMP_Dropdown graphicsDropdown;

    private bool isPaused = false;
    private InputSystem_Actions controls; // 👈 NUEVO

    void Awake()
    {
        controls = new InputSystem_Actions(); // 👈 NUEVO
        controls.UI.Pause.performed += ctx => TogglePause(); // 👈 NUEVO
    }

    void OnEnable()
    {
        controls.UI.Enable(); // 👈 NUEVO
    }

    void OnDisable()
    {
        controls.UI.Disable(); // 👈 NUEVO
    }

    void Start()
    {
        pausePanel.SetActive(false);

        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        graphicsDropdown.value = PlayerPrefs.GetInt("Graphics", QualitySettings.GetQualityLevel());
        graphicsDropdown.RefreshShownValue();
    }

    private void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void SaveGame()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedScene", sceneName);
        PlayerPrefs.Save();
        Debug.Log("Partida guardada en escena: " + sceneName);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }

    public void SetGraphics(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("Graphics", index);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

