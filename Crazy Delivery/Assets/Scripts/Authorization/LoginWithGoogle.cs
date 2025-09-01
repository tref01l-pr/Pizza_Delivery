using System;
using System.Collections;
using Google;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using UIScripts;
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

    public LoadingScreenManager LoadingPanel;

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
        }
        catch (Exception e)
        {
            Debug.Log($"Initialization failed: {e.Message}");
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
        LoadingPanel.HideLoadingScreen();
    }

    private void HandlePlayerLoggedOut()
    {
        Debug.Log("Player logged out");
    }

    private void HandlePlayerError(string error)
    {
        Debug.Log(error);
    }

    private void HandleConnectionStatusChanged(bool isOnline)
    {
        Debug.Log($"Connection status changed: {(isOnline ? "Online" : "Offline")}");
        
        if (PlayerManager.Instance != null && PlayerManager.Instance.IsLoggedIn)
        {
            string mode = isOnline ? "Online" : "Offline";
            string syncStatus = PlayerManager.Instance.HasUnsyncedData ? " (Unsynced)" : "";
            Debug.Log($"Logged in ({mode}){syncStatus}");
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
        LoadingPanel.ShowLoadingScreen();
        isSigningIn = true;

#if UNITY_EDITOR
        // В редакторе сразу вызываем успешную авторизацию
        isSigningIn = false;
        OnFirebaseAuthSuccess("editor_user_id", "Editor User");
#else
        // Выполняем реальную авторизацию Google
        PerformRealGoogleSignIn();
#endif
    }
    catch (Exception e)
    {
        Debug.LogError($"Login failed with exception: {e.Message}");
        LoadingPanel.HideLoadingScreen();
        isSigningIn = false;
    }
}

    private void PerformRealGoogleSignIn()
    {
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
                signInCompleted.SetCanceled();
                Debug.Log("Cancelled");
                isSigningIn = false;
            }
            else if (task.IsFaulted)
            {
                signInCompleted.SetException(task.Exception);
                isSigningIn = false;
                Debug.Log("Faulted " + task.Exception);
            }
            else
            {
                Credential credential = GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
                auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
                {
                    if (authTask.IsCanceled)
                    {
                        signInCompleted.SetCanceled();
                        isSigningIn = false;
                    }
                    else if (authTask.IsFaulted)
                    {
                        signInCompleted.SetException(authTask.Exception);
                        isSigningIn = false;
                        Debug.Log("Faulted In Auth " + task.Exception);
                    }
                    else
                    {
                        signInCompleted.SetResult(((Task<FirebaseUser>)authTask).Result);
                        Debug.Log("Success");
                        isSigningIn = false;
                        user = auth.CurrentUser;
                        OnFirebaseAuthSuccess(user.UserId, user.DisplayName);

                        StartCoroutine(LoadImage(CheckImageUrl(user.PhotoUrl.ToString())));
                    }
                });
            }
        });
    }

    public void Logout()
    {
        if (auth != null)
        {
            auth.SignOut();
        }
        
        GoogleSignIn.DefaultInstance.SignOut();
        
        user = null;
        
        Debug.Log("User logged out");
    }

    public void GetInfo()
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.IsLoggedIn)
        {
            User currentUser = PlayerManager.Instance.CurrentUser;
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
    #endregion

    #region Private Methods
    private void OnFirebaseAuthSuccess(string userId, string displayName)
    {
        // Уведомляем PlayerManager о входе
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnGoogleLoginSuccess(
                userId,
                displayName ?? "Unknown Player"
            );
        }
        else
        {
            Debug.LogError("PlayerManager.Instance is null!");
            LoadingPanel.HideLoadingScreen();
        }

        isSigningIn = false;
    }
    

    
    #endregion

    #region Helper Methods

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