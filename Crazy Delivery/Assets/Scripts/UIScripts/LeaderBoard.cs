using System.Collections.Generic;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{
    private List<User> users;
    void Start()
    {
        DatabaseManager.Instance.OnLeaderboardLoaded += HandleLeaderboardLoaded;
    }
    
    private void HandleLeaderboardLoaded(List<User> loadedUsers)
    {
        users = loadedUsers;
        
    }
}
