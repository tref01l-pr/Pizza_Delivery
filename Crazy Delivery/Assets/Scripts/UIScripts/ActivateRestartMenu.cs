using Gadd420;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ActivateRestartMenu : MonoBehaviour
{
    [SerializeField] private RB_Controller _rbController;
    [SerializeField] private ScoreManager _scoreManager;
    
    [SerializeField] private Canvas _playerHood;
    [SerializeField] private Canvas _restartMenu;

    [SerializeField] private Text _scoreOnRestartMenu;
    [SerializeField] private Text _highScore;

    private void Start()
    {
        _playerHood.enabled = true;
        _restartMenu.enabled = false;
        _highScore.text = PlayerPrefs.GetInt(PlayerManager.PREF_USER_SCORE, 0).ToString();
    }

    private void Update()
    {
        //Reset();
        if (_rbController.isCrashed)
        {
            _playerHood.enabled = false;
            _restartMenu.enabled = true;
            _scoreOnRestartMenu.text = _scoreManager.Score.ToString();
            SavingHighScore();
        }
    }
    
    private void SavingHighScore()
    {
        if (PlayerManager.Instance != null)
        {
            int currentBestScore = PlayerManager.Instance.GetCurrentScore();
            
            if (_scoreManager.Score > currentBestScore)
            {
                PlayerManager.Instance.UpdatePlayerScore(_scoreManager.Score);
                Debug.Log($"New high score: {_scoreManager.Score}!");
                _highScore.text = _scoreManager.Score.ToString();
            }
        }
        else
        {
            Debug.LogWarning("Player not logged in - score not saved");
            ShowLoginPrompt();
        }
    }

    private void ShowLoginPrompt()
    {
        SceneManager.LoadScene(0);
    }


    private void Reset()
    {
        PlayerPrefs.DeleteKey("HighScore");
    }
}
