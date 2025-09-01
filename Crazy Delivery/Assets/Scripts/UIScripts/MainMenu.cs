using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject loginMenuUI;
    public void ShowLoginMenu()
    {
        loginMenuUI.SetActive(true);
        mainMenuUI.SetActive(false);
    }
    
    public void ShowMainMenu()
    {
        loginMenuUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }
    
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    
    public void Logout()
    {
        PlayerManager.Instance.LogoutPlayer();
        ShowLoginMenu();
    }
}
