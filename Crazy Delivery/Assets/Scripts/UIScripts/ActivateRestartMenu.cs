using System;
using System.Collections;
using System.Collections.Generic;
using Gadd420;
using UnityEngine;

using UnityEngine.UI;

public class ActivateRestartMenu : MonoBehaviour
{
    [SerializeField] private RB_Controller _rbController;
    [SerializeField] private ScoreManager _scoreManager;
    
    [SerializeField] private Canvas PlayerHood;
    [SerializeField] private Canvas RestartMenu;

    [SerializeField] private Text scoreOnRestartMenu;
    [SerializeField] private Text highScore;

    private void SavingHighScore()
    {
        if (_scoreManager.score > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", _scoreManager.score);
            highScore.text = _scoreManager.score.ToString();
        }
    }
    
    private void Start()
    {
        PlayerHood.enabled = true;
        RestartMenu.enabled = false;
        highScore.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
    }

    private void Update()
    {
        //Reset();
        if (_rbController.isCrashed)
        {
            PlayerHood.enabled = false;
            RestartMenu.enabled = true;
            scoreOnRestartMenu.text = _scoreManager.score.ToString();
            SavingHighScore();
        }
    }

    private void Reset()
    {
        PlayerPrefs.DeleteKey("HighScore");
    }
}
