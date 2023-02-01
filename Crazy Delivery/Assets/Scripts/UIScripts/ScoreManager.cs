using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int Score { get; private set; }

    [SerializeField] private Text scoreText;

    private void Start()
    {
        Score = 0;
        scoreText.text = Score.ToString();
    }
    
    public void AddPoint()
    {
        Score += 1;
        scoreText.text = Score.ToString();
    }
}
