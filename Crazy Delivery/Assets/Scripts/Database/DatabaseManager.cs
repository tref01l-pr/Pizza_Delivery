using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Database;
using TMPro;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    // Singleton instance
    public static DatabaseManager Instance { get; private set; }
    
    private DatabaseReference _databaseReference;
    private bool _isFirebaseReady = false;
    private bool _isFirebaseAvailable = false;

    [Header("UI References (Optional)")]
    public TMP_InputField NameInputField;
    public TMP_InputField ScoreInputField;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI ScoreText;

    // Events
    public event Action<User> OnUserLoaded;
    public event Action<List<User>> OnLeaderboardLoaded;
    public event Action<string> OnError;
    public event Action<bool> OnFirebaseStatusChanged;

    // Properties
    public bool IsFirebaseReady => _isFirebaseReady && _isFirebaseAvailable;
    public bool IsFirebaseAvailable => _isFirebaseAvailable;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeFirebase();
    }

    #region Firebase Initialization
    private void InitializeFirebase()
    {
        // Проверяем доступность интернета перед инициализацией Firebase
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet connection - Firebase will not be initialized");
            _isFirebaseAvailable = false;
            _isFirebaseReady = false;
            OnFirebaseStatusChanged?.Invoke(false);
            return;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                try
                {
                    _databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                    _isFirebaseReady = true;
                    _isFirebaseAvailable = true;
                    Debug.Log("Firebase Database initialized successfully");
                    
                    // Уведомляем об успешной инициализации в главном потоке
                    MainThreadDispatcher.Instance?.Enqueue(() => {
                        OnFirebaseStatusChanged?.Invoke(true);
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError($"Firebase initialization error: {e.Message}");
                    _isFirebaseReady = false;
                    _isFirebaseAvailable = false;
                    
                    MainThreadDispatcher.Instance?.Enqueue(() => {
                        OnError?.Invoke($"Firebase setup error: {e.Message}");
                        OnFirebaseStatusChanged?.Invoke(false);
                    });
                }
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
                _isFirebaseReady = false;
                _isFirebaseAvailable = false;
                
                MainThreadDispatcher.Instance?.Enqueue(() => {
                    OnError?.Invoke($"Firebase initialization failed: {dependencyStatus}");
                    OnFirebaseStatusChanged?.Invoke(false);
                });
            }
        });
    }

    /// <summary>
    /// Проверяет и переинициализирует Firebase при восстановлении соединения
    /// </summary>
    public void CheckAndReinitializeFirebase()
    {
        if (!_isFirebaseReady && Application.internetReachability != NetworkReachability.NotReachable)
        {
            Debug.Log("Attempting to reinitialize Firebase...");
            InitializeFirebase();
        }
    }
    #endregion

    #region User Authentication & Management
    /// <summary>
    /// Главный метод входа в аккаунт по googleAuthenticationId
    /// Работает только при наличии Firebase соединения
    /// </summary>
    public void SignInWithGoogleId(string googleAuthenticationId, string fallbackUsername = "", int fallbackScore = 0)
    {
        // Проверяем доступность Firebase
        if (!IsFirebaseReady)
        {
            string errorMessage = _isFirebaseAvailable ? "Firebase not ready" : "No internet connection";
            Debug.LogWarning($"Cannot sign in with Firebase: {errorMessage}");
            OnError?.Invoke(errorMessage);
            return;
        }

        StartCoroutine(SignInWithGoogleIdCoroutine(googleAuthenticationId, fallbackUsername, fallbackScore));
    }

    private IEnumerator SignInWithGoogleIdCoroutine(string googleAuthenticationId, string fallbackUsername, int fallbackScore)
    {
        Debug.Log($"Attempting sign in with Google ID: {googleAuthenticationId}");

        // Дополнительная проверка соединения перед запросом
        if (!IsFirebaseReady)
        {
            OnError?.Invoke("Firebase connection lost during sign in");
            yield break;
        }

        var query = _databaseReference.Child("users")
            .OrderByChild("googleAuthenticationId")
            .EqualTo(googleAuthenticationId)
            .LimitToFirst(1);

        var task = query.GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Database query failed: {task.Exception}");
            HandleDatabaseException(task.Exception);
            yield break;
        }

        DataSnapshot snapshot = task.Result;

        if (snapshot.Exists && snapshot.ChildrenCount > 0)
        {
            foreach (DataSnapshot userSnapshot in snapshot.Children)
            {
                try
                {
                    User existingUser = JsonUtility.FromJson<User>(userSnapshot.GetRawJsonValue());
                    Debug.Log($"User found: {existingUser.username} with score: {existingUser.score}");
                    OnUserLoaded?.Invoke(existingUser);
                    yield break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse user data: {e.Message}");
                    OnError?.Invoke($"Failed to parse user data: {e.Message}");
                    yield break;
                }
            }
        }
        else
        {
            Debug.Log("User not found, creating new user");
            yield return StartCoroutine(CreateNewUserCoroutine(fallbackUsername, googleAuthenticationId, fallbackScore));
        }
    }

    /// <summary>
    /// Загружает пользователя по googleAuthenticationId (для автологина)
    /// </summary>
    public void LoadUser(string googleAuthenticationId)
    {
        if (!IsFirebaseReady)
        {
            OnError?.Invoke("Cannot load user: Firebase not available");
            return;
        }

        if (string.IsNullOrEmpty(googleAuthenticationId))
        {
            OnError?.Invoke("Google Authentication ID is empty");
            return;
        }

        StartCoroutine(LoadUserCoroutine(googleAuthenticationId));
    }

    private IEnumerator LoadUserCoroutine(string googleAuthenticationId)
    {
        Debug.Log($"Loading user: {googleAuthenticationId}");

        if (!IsFirebaseReady)
        {
            OnError?.Invoke("Firebase connection lost during user load");
            yield break;
        }

        var task = _databaseReference.Child("users").Child(googleAuthenticationId).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to load user: {task.Exception}");
            HandleDatabaseException(task.Exception);
            yield break;
        }

        DataSnapshot snapshot = task.Result;

        if (snapshot.Exists)
        {
            try
            {
                User user = JsonUtility.FromJson<User>(snapshot.GetRawJsonValue());
                Debug.Log($"User loaded successfully: {user.username}");
                OnUserLoaded?.Invoke(user);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse user data: {e.Message}");
                OnError?.Invoke($"Failed to parse user data: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"User not found: {googleAuthenticationId}");
            OnError?.Invoke($"User not found: {googleAuthenticationId}");
        }
    }

    private IEnumerator CreateNewUserCoroutine(string username, string googleAuthenticationId, int score)
    {
        if (string.IsNullOrEmpty(username))
        {
            username = $"Player_{UnityEngine.Random.Range(1000, 9999)}";
        }

        User newUser = new User(username, googleAuthenticationId, score);
        string json = JsonUtility.ToJson(newUser);
        
        if (!IsFirebaseReady)
        {
            OnError?.Invoke("Firebase connection lost during user creation");
            yield break;
        }
        
        var task = _databaseReference.Child("users").Child(googleAuthenticationId).SetRawJsonValueAsync(json);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to create user: {task.Exception}");
            HandleDatabaseException(task.Exception);
        }
        else
        {
            Debug.Log($"New user created: {username}");
            OnUserLoaded?.Invoke(newUser);
        }
    }
    #endregion

    #region Score Management
    /// <summary>
    /// Обновление счета пользователя
    /// </summary>
    public void UpdateUserScore(string googleAuthenticationId, int newScore)
    {
        if (!IsFirebaseReady)
        {
            Debug.LogWarning("Cannot update score: Firebase not available");
            OnError?.Invoke("Cannot update score: No internet connection");
            return;
        }

        StartCoroutine(UpdateUserScoreCoroutine(googleAuthenticationId, newScore));
    }

    private IEnumerator UpdateUserScoreCoroutine(string googleAuthenticationId, int newScore)
    {
        if (!IsFirebaseReady)
        {
            OnError?.Invoke("Firebase connection lost during score update");
            yield break;
        }

        // Обновляем и счет, и timestamp
        var updates = new Dictionary<string, object>
        {
            ["score"] = newScore,
            ["lastUpdated"] = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var task = _databaseReference.Child("users").Child(googleAuthenticationId).UpdateChildrenAsync(updates);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to update score: {task.Exception}");
            HandleDatabaseException(task.Exception);
        }
        else
        {
            Debug.Log($"Score updated to {newScore} for user {googleAuthenticationId}");
        }
    }
    #endregion

    #region Leaderboard
    /// <summary>
    /// Получение топ-10 игроков по очкам (только онлайн)
    /// </summary>
    public void LoadLeaderboard()
    {
        if (!IsFirebaseReady)
        {
            OnError?.Invoke("Leaderboard requires internet connection");
            return;
        }

        StartCoroutine(LoadLeaderboardCoroutine());
    }

    private IEnumerator LoadLeaderboardCoroutine()
    {
        Debug.Log("Loading leaderboard...");

        if (!IsFirebaseReady)
        {
            OnError?.Invoke("Firebase connection lost during leaderboard load");
            yield break;
        }

        var query = _databaseReference.Child("users")
            .OrderByChild("score")
            .LimitToLast(10);

        var task = query.GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Leaderboard query failed: {task.Exception}");
            HandleDatabaseException(task.Exception);
            yield break;
        }

        DataSnapshot snapshot = task.Result;
        List<User> leaderboard = new List<User>();

        if (snapshot.Exists)
        {
            foreach (DataSnapshot userSnapshot in snapshot.Children)
            {
                try
                {
                    User user = JsonUtility.FromJson<User>(userSnapshot.GetRawJsonValue());
                    leaderboard.Add(user);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to parse user data: {e.Message}");
                }
            }

            leaderboard = leaderboard.OrderByDescending(u => u.score).ToList();
        }

        Debug.Log($"Leaderboard loaded: {leaderboard.Count} entries");
        OnLeaderboardLoaded?.Invoke(leaderboard);
    }
    #endregion

    #region Error Handling
    private void HandleDatabaseException(Exception exception)
    {
        string errorMessage = GetReadableError(exception);
        
        // Проверяем, не связана ли ошибка с потерей соединения
        if (IsConnectionError(exception))
        {
            Debug.LogWarning("Database connection error detected, marking Firebase as unavailable");
            _isFirebaseReady = false;
            _isFirebaseAvailable = false;
            OnFirebaseStatusChanged?.Invoke(false);
            errorMessage = "Connection lost - switched to offline mode";
        }
        
        OnError?.Invoke(errorMessage);
    }

    private bool IsConnectionError(Exception exception)
    {
        if (exception == null) return false;
        
        string message = exception.Message.ToLower();
        return message.Contains("network") || 
               message.Contains("connection") || 
               message.Contains("timeout") ||
               message.Contains("unreachable") ||
               Application.internetReachability == NetworkReachability.NotReachable;
    }

    private string GetReadableError(Exception exception)
    {
        if (exception?.InnerException != null)
        {
            return exception.InnerException.Message;
        }
        return exception?.Message ?? "Unknown database error";
    }
    #endregion

    #region Connection Management
    /// <summary>
    /// Проверяет статус соединения и переинициализирует Firebase при необходимости
    /// </summary>
    public void CheckConnectionStatus()
    {
        bool hasInternet = Application.internetReachability != NetworkReachability.NotReachable;
        
        if (hasInternet && !_isFirebaseReady)
        {
            Debug.Log("Internet connection detected, attempting to reconnect to Firebase");
            CheckAndReinitializeFirebase();
        }
        else if (!hasInternet && _isFirebaseReady)
        {
            Debug.Log("Internet connection lost, Firebase marked as unavailable");
            _isFirebaseReady = false;
            _isFirebaseAvailable = false;
            OnFirebaseStatusChanged?.Invoke(false);
        }
    }

    /// <summary>
    /// Проверяет, может ли Firebase выполнить операцию
    /// </summary>
    public bool CanPerformFirebaseOperation()
    {
        return IsFirebaseReady && Application.internetReachability != NetworkReachability.NotReachable;
    }
    #endregion

    #region Unity Lifecycle
    void Update()
    {
        // Периодически проверяем статус соединения
        if (Time.frameCount % 600 == 0) // Каждые 10 секунд при 60 FPS
        {
            CheckConnectionStatus();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion

    #region Debug Methods
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugPrintStatus()
    {
        Debug.Log($"=== DatabaseManager Status ===");
        Debug.Log($"Firebase Ready: {_isFirebaseReady}");
        Debug.Log($"Firebase Available: {_isFirebaseAvailable}");
        Debug.Log($"Internet Reachability: {Application.internetReachability}");
        Debug.Log($"Can Perform Operations: {CanPerformFirebaseOperation()}");
        Debug.Log($"===============================");
    }
    #endregion
}