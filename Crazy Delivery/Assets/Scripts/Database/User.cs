using System;
public class User
{
    public string username;
    public string googleAuthenticationId;
    public int score;
    public long lastUpdated; // Unix timestamp для отслеживания обновлений
    public string createdAt;  // Дата создания аккаунта

    public User() { } // Пустой конструктор для десериализации

    public User(string username, string googleAuthenticationId, int score)
    {
        this.username = username;
        this.googleAuthenticationId = googleAuthenticationId;
        this.score = score;
        this.lastUpdated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Обновляет счет и время последнего обновления
    /// </summary>
    public void UpdateScore(int newScore)
    {
        this.score = newScore;
        this.lastUpdated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Проверяет валидность пользователя
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(username) && 
               !string.IsNullOrEmpty(googleAuthenticationId) && 
               score >= 0;
    }

    /// <summary>
    /// Создает копию пользователя
    /// </summary>
    public User Clone()
    {
        return new User
        {
            username = this.username,
            googleAuthenticationId = this.googleAuthenticationId,
            score = this.score,
            lastUpdated = this.lastUpdated,
            createdAt = this.createdAt
        };
    }

    /// <summary>
    /// Сравнивает двух пользователей по времени обновления
    /// </summary>
    public bool IsNewerThan(User other)
    {
        return other == null || this.lastUpdated > other.lastUpdated;
    }

    public override string ToString()
    {
        return $"User: {username} (ID: {googleAuthenticationId}, Score: {score}, Updated: {lastUpdated})";
    }
}