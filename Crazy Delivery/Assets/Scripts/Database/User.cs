using System;
public class User
{
    public string username;
    public string googleAuthenticationId;
    public int score;
    public long lastUpdated;
    public string createdAt;

    public User() { }

    public User(string username, string googleAuthenticationId, int score)
    {
        this.username = username;
        this.googleAuthenticationId = googleAuthenticationId;
        this.score = score;
        lastUpdated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    public void UpdateScore(int newScore)
    {
        if (score >= newScore)
        {
            return;
        }
        score = newScore;
        lastUpdated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public override string ToString()
    {
        return $"User: {username} (ID: {googleAuthenticationId}, Score: {score}, Updated: {lastUpdated})";
    }
}