using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject difficultyPanel;
    
    [Header("Buttons")]
    public Button singlePlayerButton;
    public Button twoPlayerButton;
    public Button quitButton;
    
    [Header("Difficulty")]
    public TMP_Dropdown difficultyDropdown;
    public Toggle playAsBlackToggle; // NOVO
    public Button startGameButton;
    public Button backButton;
    
    private bool isTwoPlayerMode = false;
    
    private void Start()
    {
        singlePlayerButton.onClick.AddListener(() => SelectMode(false));
        twoPlayerButton.onClick.AddListener(() => SelectMode(true));
        quitButton.onClick.AddListener(QuitGame);
        
        startGameButton.onClick.AddListener(StartGame);
        backButton.onClick.AddListener(BackToMainMenu);
        
        mainMenuPanel.SetActive(true);
        difficultyPanel.SetActive(false);
    }
    
    private void SelectMode(bool twoPlayer)
    {
        isTwoPlayerMode = twoPlayer;
        
        if (twoPlayer)
        {
            PlayerPrefs.SetInt("TwoPlayerMode", 1);
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            mainMenuPanel.SetActive(false);
            difficultyPanel.SetActive(true);
        }
    }
    
    private void StartGame()
    {
        PlayerPrefs.SetInt("TwoPlayerMode", 0);
        PlayerPrefs.SetInt("AIDifficulty", difficultyDropdown.value);
        
        // NOVO - Sačuvaj izbor boje
        int playerIsBlack = (playAsBlackToggle != null && playAsBlackToggle.isOn) ? 1 : 0;
        PlayerPrefs.SetInt("PlayerIsBlack", playerIsBlack);
        
        SceneManager.LoadScene("SampleScene");
    }
    
    private void BackToMainMenu()
    {
        mainMenuPanel.SetActive(true);
        difficultyPanel.SetActive(false);
    }
    
    private void QuitGame()
    {
        Application.Quit();
    }
}