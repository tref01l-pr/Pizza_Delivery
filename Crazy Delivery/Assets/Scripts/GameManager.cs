using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadLeaderboardWithDelay());
    }

    IEnumerator LoadLeaderboardWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        PlayerManager.Instance.LoadLeaderboard();
    }
}
