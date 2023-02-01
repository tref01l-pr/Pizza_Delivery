using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private Text scoreText;

    public int Score { get; private set; }
    
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
