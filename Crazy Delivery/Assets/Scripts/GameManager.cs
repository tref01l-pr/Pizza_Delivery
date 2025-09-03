using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        PlayerManager.Instance.LoadLeaderboard();
    }
}
