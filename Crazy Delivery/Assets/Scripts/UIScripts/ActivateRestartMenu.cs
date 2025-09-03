using System.Collections;
using Gadd420;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ActivateRestartMenu : MonoBehaviour
{
    [SerializeField] private RB_Controller _rbController;
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private LeaderBoard _leaderBoard;
    
    [SerializeField] private Canvas _playerHood;
    [SerializeField] private Canvas _restartMenu;

    [SerializeField] private Text _scoreOnRestartMenu;
    [SerializeField] private Text _highScore;

    private bool _crashHandled = false;
    private void Start()
    {
        _playerHood.enabled = true;
        _restartMenu.enabled = false;
        _highScore.text = PlayerPrefs.GetInt(PlayerManager.PREF_USER_SCORE, 0).ToString();
    }

    private void Update()
    {
        //Reset();
        if (_rbController.isCrashed && !_crashHandled)
        {
            _crashHandled = true;
            StartCoroutine(HandleCrashWithDelay());
        }
    }

    private IEnumerator HandleCrashWithDelay()
    {
        _playerHood.enabled = false;
        _scoreOnRestartMenu.text = _scoreManager.Score.ToString();
        SavingHighScore();
        
        yield return new WaitForSeconds(0.3f);
    
        _restartMenu.enabled = true;
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
            _leaderBoard.InitializeLeaderboard(PlayerManager.Instance.GetCurrentScore());
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
