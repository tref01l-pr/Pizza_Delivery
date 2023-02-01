using Gadd420;
using UnityEngine;

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
        _highScore.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
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
        if (_scoreManager.Score > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", _scoreManager.Score);
            _highScore.text = _scoreManager.Score.ToString();
        }
    }

    private void Reset()
    {
        PlayerPrefs.DeleteKey("HighScore");
    }
}
