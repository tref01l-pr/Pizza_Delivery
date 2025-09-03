using UnityEngine;
using System;
using System.Collections;
using UIScripts;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DatabaseManager databaseManager;
    [SerializeField] private LoginWithGoogle loginWithGoogle;
    [SerializeField] private LoadingScreenManager loadingScreenManager;
    
    [SerializeField] private MainMenu mainMenu;
    
    public static PlayerManager Instance { get; private set; }

    [Header("Settings")]
    public bool persistBetweenScenes = true;
    public bool autoLoginOnStart = true;
    public bool enableOfflineMode = true;

    [Header("Current Player Data")]
    private User currentUser;
    [SerializeField] private bool isLoggedIn = false;
    [SerializeField] private string currentGoogleId;
    [SerializeField] private bool isOnlineMode = false;
    [SerializeField] private bool hasUnsyncedData = false;

    // Events
    public event Action<User> OnPlayerLoaded;
    public event Action OnPlayerLoggedOut;
    public event Action<string> OnPlayerError;
    public event Action<int> OnScoreUpdated;
    public event Action<bool> OnConnectionStatusChanged;
    
    // Properties
    public User CurrentUser => currentUser;
    public bool IsLoggedIn => isLoggedIn;
    public string CurrentGoogleId => currentGoogleId;
    public bool IsOnlineMode => isOnlineMode;
    public bool HasUnsyncedData => hasUnsyncedData;

    // PlayerPrefs Keys
    public const string PREF_USER_GOOGLE_ID = "UserGoogleId";
    public const string PREF_USERNAME = "Username";
    public const string PREF_USER_SCORE = "UserScore";
    public const string PREF_LAST_UPDATED = "LastUpdated";
    public const string PREF_CREATED_AT = "CreatedAt";
    public const string PREF_IS_LOGGED_IN = "IsLoggedIn";
    public const string PREF_HAS_UNSYNCED_DATA = "HasUnsyncedData";

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            
            if (persistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Debug.LogWarning($"Another PlayerManager instance already exists. Destroying {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        // Auto-find dependencies if not assigned
        InitializeDependencies();
    }

    void Start()
    {
        if (autoLoginOnStart)
        {
            StartCoroutine(InitializePlayerWithDelay());
        }
    }

    IEnumerator InitializePlayerWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        InitializePlayer();
    }

    #region Initialization
    private void InitializeDependencies()
    {
        if (databaseManager == null)
        {
            databaseManager = DatabaseManager.Instance;
        }

        if (loginWithGoogle == null)
        {
            loginWithGoogle = FindObjectOfType<LoginWithGoogle>();
        }

        // Subscribe to events
        SubscribeToEvents();
        
        // Check initial connection status
        CheckConnectionStatus();
    }

    private void SubscribeToEvents()
    {
        if (databaseManager != null)
        {
            databaseManager.OnUserLoaded += HandleUserLoadedFromDatabase;
            databaseManager.OnError += HandleDatabaseError;
            databaseManager.OnFirebaseStatusChanged += HandleFirebaseStatusChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (databaseManager != null)
        {
            databaseManager.OnUserLoaded -= HandleUserLoadedFromDatabase;
            databaseManager.OnError -= HandleDatabaseError;
            databaseManager.OnFirebaseStatusChanged -= HandleFirebaseStatusChanged;
        }
    }

    private void InitializePlayer()
    {
        Debug.Log("Initializing player...");
        
        LoadUserFromPlayerPrefs();
        
        if (isLoggedIn && isOnlineMode)
        {
            SyncWithServer();
        }
    }

    private void CheckConnectionStatus()
    {
        bool hasInternet = Application.internetReachability != NetworkReachability.NotReachable;
        bool firebaseAvailable = databaseManager != null && databaseManager.IsFirebaseReady;
        
        bool wasOnline = isOnlineMode;
        isOnlineMode = hasInternet && firebaseAvailable;
        
        if (wasOnline != isOnlineMode)
        {
            Debug.Log($"Connection status changed: {(isOnlineMode ? "Online" : "Offline")}");
            OnConnectionStatusChanged?.Invoke(isOnlineMode);
            
            if (isOnlineMode && hasUnsyncedData && isLoggedIn)
            {
                SyncWithServer();
            }
            
            if (!isOnlineMode && hasInternet && databaseManager != null)
            {
                databaseManager.CheckConnectionStatus();
            }
        }
    }
    #endregion

    #region PlayerPrefs Management
    private void LoadUserFromPlayerPrefs()
    {
        string savedGoogleId = PlayerPrefs.GetString(PREF_USER_GOOGLE_ID, string.Empty);
        bool savedLoginState = PlayerPrefs.GetInt(PREF_IS_LOGGED_IN, 0) == 1;
        
        if (string.IsNullOrEmpty(savedGoogleId) || !savedLoginState)
        {
            Debug.Log("No saved user found, user needs to login.");
            SetLoginState(false);
            loadingScreenManager?.HideLoadingScreen();
            mainMenu?.ShowLoginMenu();
            return;
        }

        string username = PlayerPrefs.GetString(PREF_USERNAME, "Guest");
        int score = PlayerPrefs.GetInt(PREF_USER_SCORE, 0);
        long lastUpdated = long.Parse(PlayerPrefs.GetString(PREF_LAST_UPDATED, "0"));
        string createdAt = PlayerPrefs.GetString(PREF_CREATED_AT, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        hasUnsyncedData = PlayerPrefs.GetInt(PREF_HAS_UNSYNCED_DATA, 0) == 1;

        currentUser = new User
        {
            username = username,
            googleAuthenticationId = savedGoogleId,
            score = score,
            lastUpdated = lastUpdated,
            createdAt = createdAt
        };

        currentGoogleId = savedGoogleId;
        SetLoginState(true);

        Debug.Log($"User loaded from PlayerPrefs: {username} (Score: {score}, Unsynced: {hasUnsyncedData})");
        OnPlayerLoaded?.Invoke(currentUser);
        LoadLeaderboard();
        loadingScreenManager?.HideLoadingScreen();
        mainMenu?.ShowMainMenu();
    }
    
    private void SaveUserToPlayerPrefs()
    {
        if (currentUser == null)
        {
            Debug.LogWarning("Cannot save null user to PlayerPrefs");
            return;
        }

        PlayerPrefs.SetString(PREF_USER_GOOGLE_ID, currentUser.googleAuthenticationId);
        PlayerPrefs.SetString(PREF_USERNAME, currentUser.username);
        PlayerPrefs.SetInt(PREF_USER_SCORE, currentUser.score);
        PlayerPrefs.SetString(PREF_LAST_UPDATED, currentUser.lastUpdated.ToString());
        PlayerPrefs.SetString(PREF_CREATED_AT, currentUser.createdAt);
        PlayerPrefs.SetInt(PREF_IS_LOGGED_IN, isLoggedIn ? 1 : 0);
        PlayerPrefs.SetInt(PREF_HAS_UNSYNCED_DATA, hasUnsyncedData ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"User data saved to PlayerPrefs: {currentUser.username}");
    }
    
    private void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(PREF_USER_GOOGLE_ID);
        PlayerPrefs.DeleteKey(PREF_USERNAME);
        PlayerPrefs.DeleteKey(PREF_USER_SCORE);
        PlayerPrefs.DeleteKey(PREF_LAST_UPDATED);
        PlayerPrefs.DeleteKey(PREF_CREATED_AT);
        PlayerPrefs.DeleteKey(PREF_IS_LOGGED_IN);
        PlayerPrefs.DeleteKey(PREF_HAS_UNSYNCED_DATA);
        PlayerPrefs.Save();
        
        Debug.Log("PlayerPrefs cleared");
    }
    #endregion

    #region User Management
    public void OnGoogleLoginSuccess(string googleId, string username)
    {
        Debug.Log($"Google login successful: {username} ({googleId})");
        
        currentGoogleId = googleId;
        CheckConnectionStatus();

        if (isOnlineMode)
        {
            if (databaseManager != null)
            {
                databaseManager.SignInWithGoogleId(googleId, username, 0);
                LoadLeaderboard();
            }
        }
        else
        {
            CreateOfflineUser(googleId, username, 0);
        }
    }
    
    private void CreateOfflineUser(string googleId, string username, int score)
    {
        currentUser = new User(username, googleId, score);
        currentGoogleId = googleId;
        SetLoginState(true);
        hasUnsyncedData = true;

        SaveUserToPlayerPrefs();
        OnPlayerLoaded?.Invoke(currentUser);
        loadingScreenManager?.HideLoadingScreen();
        mainMenu?.ShowMainMenu();

        Debug.Log($"Offline user created: {username}");
    }
    
    private void SyncWithServer()
    {
        if (!isOnlineMode || databaseManager == null || string.IsNullOrEmpty(currentGoogleId))
        {
            return;
        }

        Debug.Log("Syncing with server...");
        
        StartCoroutine(SyncCoroutine());
    }

    private IEnumerator SyncCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (databaseManager != null)
        {
            if (hasUnsyncedData && currentUser != null)
            {
                databaseManager.SignInWithGoogleId(currentGoogleId, currentUser.username, currentUser.score);
            }
            else
            {
                databaseManager.SignInWithGoogleId(currentGoogleId, "", 0);
            }
        }
    }
    
    public void UpdatePlayerScore(int newScore)
    {
        if (!isLoggedIn)
        {
            HandlePlayerError("User not logged in");
            return;
        }

        if (currentUser.score >= newScore)
        {
            return;
        }

        Debug.Log($"Updating score for {currentUser?.username}: {newScore}");
        
        if (currentUser != null)
        {
            currentUser.UpdateScore(newScore);
        }

        SaveUserToPlayerPrefs();

        if (isOnlineMode && databaseManager != null)
        {
            databaseManager.UpdateUserScore(currentGoogleId, newScore);
            hasUnsyncedData = false;
        }
        else
        {
            hasUnsyncedData = true;
            SaveUserToPlayerPrefs();
            Debug.Log("Score updated offline, will sync when online");
        }

        OnScoreUpdated?.Invoke(newScore);
    }
    
    public void LogoutPlayer()
    {
        StartCoroutine(LogoutCoroutine());
    }

    private IEnumerator LogoutCoroutine()
    {
        Debug.Log("Starting logout process");
    
        ClearPlayerPrefs();
        currentUser = null;
        currentGoogleId = string.Empty;
        hasUnsyncedData = false;
        SetLoginState(false);

        if (loginWithGoogle != null)
        {
            loginWithGoogle.Logout();
        }
        
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            if (loginWithGoogle != null)
            {
                LoginWithGoogle.DestroyInstance();
            }
    
            if (databaseManager != null)
            {
                databaseManager.DestroyInstance();
            }

            if (loadingScreenManager != null)
            {
                Destroy(loadingScreenManager.gameObject);
            }
        }
    
        OnPlayerLoggedOut?.Invoke();
    
        yield return null;
    
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            DestroyInstance();
        }
    
        yield return null;
    
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            mainMenu?.ShowLoginMenu();
        }

    }

    public static void DestroyInstance()
    {
        if (Instance != null)
        {
            Instance.UnsubscribeFromEvents();
            Instance.ClearPlayerPrefs();
        
            if (Instance.gameObject != null)
            {
                Destroy(Instance.gameObject);
            }
        
            Instance = null;
            Debug.Log("PlayerManager instance destroyed");
        }
    }
    
    public void LoadLeaderboard()
    {
        if (!isOnlineMode)
        {
            HandlePlayerError("Leaderboard requires internet connection");
            return;
        }

        if (databaseManager != null)
        {
            databaseManager.LoadLeaderboard();
        }
        else
        {
            HandlePlayerError("DatabaseManager not available");
        }
    }
    #endregion

    #region Event Handlers
    private void HandleUserLoadedFromDatabase(User user)
    {
        Debug.Log($"User loaded from database: {user.username}, Score: {user.score}");
        
        if (currentUser != null && hasUnsyncedData)
        {
            if (currentUser.score > user.score)
            {
                Debug.Log($"Local score ({currentUser.score}) is higher than server ({user.score}), updating server");
                databaseManager.UpdateUserScore(user.googleAuthenticationId, currentUser.score);
                user.score = currentUser.score;
            }
        }

        currentUser = user;
        currentGoogleId = user.googleAuthenticationId;
        SetLoginState(true);
        hasUnsyncedData = false;

        SaveUserToPlayerPrefs();
        OnPlayerLoaded?.Invoke(user);
        LoadLeaderboard();
        loadingScreenManager?.HideLoadingScreen();
        mainMenu?.ShowMainMenu();
    }

    private void HandleDatabaseError(string error)
    {
        Debug.LogError($"Database error: {error}");
        
        if (error.ToLower().Contains("connection") || 
            error.ToLower().Contains("network") || 
            error.ToLower().Contains("offline"))
        {
            if (isOnlineMode)
            {
                isOnlineMode = false;
                OnConnectionStatusChanged?.Invoke(false);
                Debug.Log("Switched to offline mode due to connection error");
            }
        }
        
        HandlePlayerError($"Database error: {error}");
    }

    private void HandleFirebaseStatusChanged(bool firebaseReady)
    {
        Debug.Log($"Firebase status changed: {(firebaseReady ? "Ready" : "Not Ready")}");
        
        CheckConnectionStatus();
        
        if (firebaseReady && hasUnsyncedData && isLoggedIn)
        {
            Debug.Log("Firebase became available, syncing unsynced data");
            SyncWithServer();
        }
    }

    private void HandlePlayerError(string error)
    {
        Debug.LogError($"Player error: {error}");
        OnPlayerError?.Invoke(error);
    }

    private void SetLoginState(bool loggedIn)
    {
        isLoggedIn = loggedIn;
        Debug.Log($"Login state changed: {loggedIn}");
    }
    #endregion

    #region Public Getters
    public int GetCurrentScore()
    {
        return currentUser?.score ?? 0;
    }
    
    public string GetCurrentUsername()
    {
        return currentUser?.username ?? "Guest";
    }
    
    public bool HasSavedUser()
    {
        return PlayerPrefs.GetInt(PREF_IS_LOGGED_IN, 0) == 1 && 
               !string.IsNullOrEmpty(PlayerPrefs.GetString(PREF_USER_GOOGLE_ID, string.Empty));
    }
    
    public void RefreshUser()
    {
        if (isOnlineMode && !string.IsNullOrEmpty(currentGoogleId))
        {
            SyncWithServer();
        }
        else if (!isOnlineMode)
        {
            LoadUserFromPlayerPrefs();
        }
        else
        {
            HandlePlayerError("No current user to refresh");
        }
    }
    
    public void ForceSyncWithServer()
    {
        CheckConnectionStatus();
        
        if (!isOnlineMode)
        {
            if (databaseManager != null)
            {
                databaseManager.CheckAndReinitializeFirebase();
            }
            
            CheckConnectionStatus();
        }
        
        if (isOnlineMode && isLoggedIn)
        {
            SyncWithServer();
        }
        else
        {
            string reason = !isOnlineMode ? "offline or Firebase unavailable" : "not logged in";
            HandlePlayerError($"Cannot sync: {reason}");
        }
    }
    #endregion

    #region Unity Lifecycle
    void Update()
    {
        if (Time.frameCount % 300 == 0)
        {
            CheckConnectionStatus();
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
        
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && isLoggedIn)
        {
            CheckConnectionStatus();
            if (isOnlineMode && hasUnsyncedData)
            {
                SyncWithServer();
            }
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && isLoggedIn)
        {
            CheckConnectionStatus();
            if (isOnlineMode && hasUnsyncedData)
            {
                SyncWithServer();
            }
        }
    }
    #endregion

    #region Debug Methods
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugClearSavedUser()
    {
        ClearPlayerPrefs();
        currentUser = null;
        currentGoogleId = string.Empty;
        isLoggedIn = false;
        hasUnsyncedData = false;
        Debug.Log("All user data cleared (Debug)");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugPrintUserInfo()
    {
        Debug.Log($"=== Player Manager Debug Info ===");
        Debug.Log($"Is Logged In: {isLoggedIn}");
        Debug.Log($"Is Online Mode: {isOnlineMode}");
        Debug.Log($"Has Unsynced Data: {hasUnsyncedData}");
        Debug.Log($"Current Google ID: {currentGoogleId}");
        Debug.Log($"Current User: {currentUser?.ToString() ?? "null"}");
        Debug.Log($"Saved Google ID: {PlayerPrefs.GetString(PREF_USER_GOOGLE_ID, "none")}");
        Debug.Log($"Saved Username: {PlayerPrefs.GetString(PREF_USERNAME, "none")}");
        Debug.Log($"Saved Score: {PlayerPrefs.GetInt(PREF_USER_SCORE, 0)}");
        Debug.Log($"Saved Login State: {PlayerPrefs.GetInt(PREF_IS_LOGGED_IN, 0) == 1}");
        Debug.Log($"=================================");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugSimulateOffline()
    {
        isOnlineMode = false;
        OnConnectionStatusChanged?.Invoke(false);
        Debug.Log("Simulated offline mode (Debug)");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugSimulateOnline()
    {
        CheckConnectionStatus();
        Debug.Log("Attempted to restore online mode (Debug)");
    }
    #endregion

    public void SetMainMenu(MainMenu mainMenu1)
    {
        mainMenu = mainMenu1;
    }

    public void SetLoadingScreenManager(LoadingScreenManager loadingScreenManager1)
    {
        loadingScreenManager = loadingScreenManager1;
    }
}