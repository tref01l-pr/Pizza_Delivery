using TMPro;
using UnityEngine;

public class LeaderInfo : MonoBehaviour
{
    [SerializeField]
    private TMP_Text userName;
    [SerializeField]
    private TMP_Text userScore;
    [SerializeField]
    private TMP_Text userRank;
    
    public void SetLeaderInfo(string name, int score, int rank)
    {
        userName.text = name;
        userScore.text = score.ToString();
        userRank.text = rank.ToString();
    }
}
