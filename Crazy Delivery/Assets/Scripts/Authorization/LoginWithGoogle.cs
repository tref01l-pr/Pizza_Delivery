using System;
using System.Collections;
using Google;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LoginWithGoogle : MonoBehaviour
{
    public string GoogleAPI = "570130608032-gibhr7rvtn0aq6c9g2s28444i869lqia.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;

    FirebaseAuth auth;
    FirebaseUser user;

    [Header("Persistence Settings")]
    public bool persistBetweenScenes = true;

    [Header("UI Elements")]
    public TextMeshProUGUI Username, UserEmail, ErrorMessage, StatusMessage;
    public Image UserProfilePic;
    public Button LoginButton, LogoutButton;
    public GameObject LoadingPanel;

    // Singleton pattern
    public static LoginWithGoogle Instance { get; private set; }

    private string imageUrl;
    private bool isGoogleSignInInitialized = false;
    private bool isSigningIn = false;

    void Awake()
    {
        // Singleton pattern implementation
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
            Debug.LogWarning($"Another LoginWithGoogle instance already exists. Destroying {gameObject.name}");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        try
        {
            InitFirebase();
            SubscribeToPlayerManagerEvents();
            UpdateUIBasedOnLoginState();
        }
        catch (Exception e)
        {
            ShowError($"Initialization failed: {e.Message}");
        }
    }

    void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    #region PlayerManager Integration
    private void SubscribeToPlayerManagerEvents()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnPlayerLoaded += HandlePlayerLoaded;
            PlayerManager.Instance.OnPlayerLoggedOut += HandlePlayerLoggedOut;
            PlayerManager.Instance.OnPlayerError += HandlePlayerError;
            PlayerManager.Instance.OnConnectionStatusChanged += HandleConnectionStatusChanged;
        }
    }

    private void UnsubscribeFromPlayerManagerEvents()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnPlayerLoaded -= HandlePlayerLoaded;
            PlayerManager.Instance.OnPlayerLoggedOut -= HandlePlayerLoggedOut;
            PlayerManager.Instance.OnPlayerError -= HandlePlayerError;
            PlayerManager.Instance.OnConnectionStatusChanged -= HandleConnectionStatusChanged;
        }
    }

    private void HandlePlayerLoaded(User userData)
    {
        Debug.Log($"Player loaded: {userData.username}");
        UpdateUIWithUserData(userData);
        SetLoadingState(false);
        
        if (StatusMessage != null)
        {
            string mode = PlayerManager.Instance.IsOnlineMode ? "Online" : "Offline";
            string syncStatus = PlayerManager.Instance.HasUnsyncedData ? " (Unsynced)" : "";
            StatusMessage.text = $"Logged in ({mode}){syncStatus}";
        }
    }

    private void HandlePlayerLoggedOut()
    {
        Debug.Log("Player logged out");
        ClearUI();
        UpdateUIBasedOnLoginState();
        
        if (StatusMessage != null)
        {
            StatusMessage.text = "Logged out";
        }
    }

    private void HandlePlayerError(string error)
    {
        ShowError(error);
        SetLoadingState(false);
    }

    private void HandleConnectionStatusChanged(bool isOnline)
    {
        Debug.Log($"Connection status changed: {(isOnline ? "Online" : "Offline")}");
        
        if (StatusMessage != null && PlayerManager.Instance != null && PlayerManager.Instance.IsLoggedIn)
        {
            string mode = isOnline ? "Online" : "Offline";
            string syncStatus = PlayerManager.Instance.HasUnsyncedData ? " (Unsynced)" : "";
            StatusMessage.text = $"Logged in ({mode}){syncStatus}";
        }
    }
    #endregion

    #region Public Methods
    public void Login()
    {
        if (isSigningIn)
        {
            Debug.Log("Already signing in...");
            return;
        }

        try
        {
            SetLoadingState(true);
            isSigningIn = true;
            ClearMessages();

            if (!isGoogleSignInInitialized)
            {
                GoogleSignIn.Configuration = new GoogleSignInConfiguration
                {
                    RequestIdToken = true,
                    WebClientId = GoogleAPI,
                    RequestEmail = true
                };
                isGoogleSignInInitialized = true;
            }

            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = GoogleAPI
            };
            GoogleSignIn.Configuration.RequestEmail = true;

            Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

            TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
            signIn.ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("Google Sign-In was cancelled");
                    MainThreadDispatcher.Instance?.Enqueue(() => {
                        ShowError("Sign-in was cancelled");
                        SetLoadingState(false);
                        isSigningIn = false;
                    });
                }
                else if (task.IsFaulted)
                {
                    Debug.Log("Google Sign-In failed: " + task.Exception);
                    MainThreadDispatcher.Instance?.Enqueue(() => {
                        ShowError($"Sign-in failed: {GetReadableError(task.Exception)}");
                        SetLoadingState(false);
                        isSigningIn = false;
                    });
                }
                else
                {
                    Debug.Log("Google Sign-In successful");
                    
                    // Аутентификация с Firebase
                    Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(
                        ((Task<GoogleSignInUser>)task).Result.IdToken, null);
                    
                    auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
                    {
                        if (authTask.IsCanceled)
                        {
                            MainThreadDispatcher.Instance?.Enqueue(() => {
                                ShowError("Firebase authentication was cancelled");
                                SetLoadingState(false);
                                isSigningIn = false;
                            });
                        }
                        else if (authTask.IsFaulted)
                        {
                            MainThreadDispatcher.Instance?.Enqueue(() => {
                                ShowError($"Firebase authentication failed: {GetReadableError(authTask.Exception)}");
                                SetLoadingState(false);
                                isSigningIn = false;
                            });
                        }
                        else
                        {
                            MainThreadDispatcher.Instance?.Enqueue(() => {
                                OnFirebaseAuthSuccess(((Task<FirebaseUser>)authTask).Result);
                            });
                        }
                    });
                }
            });
        }
        catch (Exception e)
        {
            ShowError($"Login error: {e.Message}");
            SetLoadingState(false);
            isSigningIn = false;
        }
    }

    public void Logout()
    {
        if (auth != null)
        {
            auth.SignOut();
        }
        
        GoogleSignIn.DefaultInstance.SignOut();
        
        user = null;
        ClearUI();
        ClearMessages();
        
        // Уведомляем PlayerManager о выходе
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.LogoutPlayer();
        }
        
        Debug.Log("User logged out");
    }

    public void GetInfo()
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.IsLoggedIn)
        {
            User currentUser = PlayerManager.Instance.CurrentUser;
            if (Username != null) Username.text = currentUser.username;
            if (UserEmail != null) UserEmail.text = user?.Email ?? "No email";
        }
        else
        {
            if (Username != null) Username.text = "Not logged in";
            if (UserEmail != null) UserEmail.text = "";
        }
    }

    /// <summary>
    /// Получает текущего Firebase пользователя
    /// </summary>
    public FirebaseUser GetCurrentFirebaseUser()
    {
        return user;
    }

    /// <summary>
    /// Проверяет, выполнен ли вход в Google
    /// </summary>
    public bool IsGoogleSignedIn()
    {
        return user != null;
    }

    /// <summary>
    /// Обновляет UI элементы с новыми ссылками (для использования между сценами)
    /// </summary>
    public void UpdateUIReferences(TextMeshProUGUI username = null, TextMeshProUGUI userEmail = null, 
                                   TextMeshProUGUI errorMessage = null, TextMeshProUGUI statusMessage = null,
                                   Image userProfilePic = null, Button loginButton = null, 
                                   Button logoutButton = null, GameObject loadingPanel = null)
    {
        if (username != null) Username = username;
        if (userEmail != null) UserEmail = userEmail;
        if (errorMessage != null) ErrorMessage = errorMessage;
        if (statusMessage != null) StatusMessage = statusMessage;
        if (userProfilePic != null) UserProfilePic = userProfilePic;
        if (loginButton != null) LoginButton = loginButton;
        if (logoutButton != null) LogoutButton = logoutButton;
        if (loadingPanel != null) LoadingPanel = loadingPanel;

        // Обновляем UI с текущим состоянием
        UpdateUIBasedOnLoginState();
        
        // Если пользователь вошел, обновляем данные
        if (PlayerManager.Instance != null && PlayerManager.Instance.IsLoggedIn)
        {
            UpdateUIWithUserData(PlayerManager.Instance.CurrentUser);
        }
    }
    #endregion

    #region Private Methods
    private void OnFirebaseAuthSuccess(FirebaseUser firebaseUser)
    {
        user = firebaseUser;
        
        // Обновляем UI с данными Firebase
        UpdateUIWithFirebaseData();
        
        // Загружаем аватар если есть
        if (user.PhotoUrl != null)
        {
            StartCoroutine(LoadImage(user.PhotoUrl.ToString()));
        }

        // Уведомляем PlayerManager о входе
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnGoogleLoginSuccess(
                user.UserId,
                user.DisplayName ?? "Unknown Player"
            );
        }
        else
        {
            Debug.LogError("PlayerManager.Instance is null!");
            ShowError("PlayerManager not available");
            SetLoadingState(false);
        }

        isSigningIn = false;
    }

    private void UpdateUIWithFirebaseData()
    {
        if (Username != null) Username.text = user.DisplayName ?? "Unknown";
        if (UserEmail != null) UserEmail.text = user.Email ?? "No email";
        
        Debug.Log($"Firebase Auth Success - User: {user.DisplayName}, Email: {user.Email}, ID: {user.UserId}");
    }

    private void UpdateUIWithUserData(User userData)
    {
        if (Username != null) Username.text = userData.username;
        if (UserEmail != null) UserEmail.text = user?.Email ?? "No email";
        
        UpdateUIBasedOnLoginState();
    }

    private void UpdateUIBasedOnLoginState()
    {
        bool isLoggedIn = PlayerManager.Instance != null && PlayerManager.Instance.IsLoggedIn;
        
        if (LoginButton != null) LoginButton.gameObject.SetActive(!isLoggedIn);
        if (LogoutButton != null) LogoutButton.gameObject.SetActive(isLoggedIn);
        
        if (!isLoggedIn)
        {
            if (Username != null) Username.text = "Not logged in";
            if (UserEmail != null) UserEmail.text = "";
        }
    }
    #endregion

    #region Helper Methods
    private void SetLoadingState(bool isLoading)
    {
        if (LoadingPanel != null) LoadingPanel.SetActive(isLoading);
        if (LoginButton != null) LoginButton.interactable = !isLoading;
    }

    private void ShowError(string error)
    {
        if (ErrorMessage != null) ErrorMessage.text = error;
        Debug.LogError(error);
    }

    private void ClearMessages()
    {
        if (ErrorMessage != null) ErrorMessage.text = "";
        if (StatusMessage != null) StatusMessage.text = "";
    }

    private void ClearUI()
    {
        if (Username != null) Username.text = "";
        if (UserEmail != null) UserEmail.text = "";
        if (UserProfilePic != null) UserProfilePic.sprite = null;
    }

    private string GetReadableError(Exception exception)
    {
        if (exception?.InnerException != null)
        {
            return exception.InnerException.Message;
        }
        return exception?.Message ?? "Unknown error";
    }
    #endregion

    #region Image Loading
    private string CheckImageUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            return url;
        }
        return imageUrl;
    }

    IEnumerator LoadImage(string imageUri)
    {
        if (string.IsNullOrEmpty(imageUri))
        {
            Debug.Log("Image URI is null or empty");
            yield break;
        }

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUri);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            Debug.Log("Profile image loaded successfully");
            
            if (UserProfilePic != null)
            {
                UserProfilePic.sprite = Sprite.Create(
                    texture, 
                    new Rect(0, 0, texture.width, texture.height), 
                    new Vector2(0.5f, 0.5f)
                );
            }
        }
        else
        {
            Debug.Log("Error loading profile image: " + www.error);
        }
    }
    #endregion

    #region Unity Lifecycle
    void OnDestroy()
    {
        UnsubscribeFromPlayerManagerEvents();
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion
}

// MainThreadDispatcher для выполнения кода в главном потоке
public class MainThreadDispatcher : MonoBehaviour
{
    public static MainThreadDispatcher Instance { get; private set; }
    private readonly System.Collections.Generic.Queue<System.Action> _actions = new System.Collections.Generic.Queue<System.Action>();

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
        }
    }

    void Update()
    {
        lock (_actions)
        {
            while (_actions.Count > 0)
            {
                _actions.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(System.Action action)
    {
        lock (_actions)
        {
            _actions.Enqueue(action);
        }
    }
}