using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{
    [SerializeField]
    private LeaderInfo _leaderInfoPrefab;
    [SerializeField]
    private TMP_Text _infoMessage;
    [SerializeField]
    private GameObject _contentPanel;
    
    private List<User> _users;
    private List<LeaderInfo> _leadersInfo;
    private bool _isInitialized = false;
    
    void Start()
    {
        DatabaseManager.Instance.OnLeaderboardLoaded += HandleLeaderboardLoaded;
    }

    public void InitializeLeaderboard(int currentBestScore)
    {
        if (_isInitialized)
        {
            return;
        }
        _isInitialized = true;
        _infoMessage.gameObject.SetActive(false);
        if (!PlayerManager.Instance.IsOnlineMode)
        {
            _infoMessage.text = "Leaderboard is unavailable in offline mode.";
            _infoMessage.gameObject.SetActive(true);
            return;
        }
        
        _users = DatabaseManager.Instance.LeaderboardCache.Where(u => u.score > 0).OrderByDescending(u => u.score).ToList();
        _leadersInfo = new List<LeaderInfo>();
        
        if (_users == null || _users.Count == 0 || currentBestScore <= 0)
        {
            _infoMessage.text = "No leaderboard data available.";
            _infoMessage.gameObject.SetActive(true);
            return;
        }

        var currentUserInLeaderBoard = _users.FirstOrDefault(u => PlayerManager.Instance.CurrentUser != null && 
            u.googleAuthenticationId == PlayerManager.Instance.CurrentUser.googleAuthenticationId);

        if (currentUserInLeaderBoard != null && currentUserInLeaderBoard.score < currentBestScore)
        {
            _users.First(u => u.googleAuthenticationId == PlayerManager.Instance.CurrentUser.googleAuthenticationId).score = currentBestScore;
            _users = _users.OrderByDescending(u => u.score).ToList();
        }
        
        if (currentUserInLeaderBoard == null && PlayerManager.Instance.CurrentUser != null && currentBestScore > 0)
        {
            var newUser = PlayerManager.Instance.CurrentUser;
            _users.Add(new User(newUser.username, newUser.googleAuthenticationId, currentBestScore));
            _users = _users.OrderByDescending(u => u.score).Take(10).ToList();
        }
        
        CreateLeaderboardWithRanks();
    }

    private void CreateLeaderboardWithRanks()
    {
        int currentRank = 1;
        int currentScore = int.MinValue;
        int playersAtCurrentRank = 0;
        
        for (int i = 0; i < _users.Count; i++)
        {
            if (_users[i].score != currentScore)
            {
                currentRank = i + 1;
                currentScore = _users[i].score;
            }
            
            var leaderInfo = Instantiate(_leaderInfoPrefab, _contentPanel.transform);
            bool isCurrentUser = PlayerManager.Instance.CurrentUser != null && 
                _users[i].googleAuthenticationId == PlayerManager.Instance.CurrentUser.googleAuthenticationId;
            
            leaderInfo.SetLeaderInfo(
                _users[i].username + (isCurrentUser ? " (You)" : ""), 
                _users[i].score,
                currentRank);
                
            _leadersInfo.Add(leaderInfo);
        }
    }

    private void HandleLeaderboardLoaded(List<User> loadedUsers)
    {
        _users = loadedUsers;
    }
    
    private void ClearLeaderboard()
    {
        if (_leadersInfo != null)
        {
            foreach (var leaderInfo in _leadersInfo)
            {
                if (leaderInfo != null)
                {
                    Destroy(leaderInfo.gameObject);
                }
            }
            _leadersInfo.Clear();
        }
    }
    
    private void OnDisable()
    {
        ClearLeaderboard();
    }
}