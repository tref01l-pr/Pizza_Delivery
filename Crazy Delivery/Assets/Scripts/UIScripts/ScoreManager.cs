using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int score { get; set; }

    [SerializeField] private Text scoreText;
    
    public void AddPoint()
    {
        score += 1;
        scoreText.text = score.ToString();
    }
    
    private void Start()
    {
        score = 0;
        scoreText.text = score.ToString();
    }
}
