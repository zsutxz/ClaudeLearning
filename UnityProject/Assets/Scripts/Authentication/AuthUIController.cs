using UnityEngine;
using UnityEngine.UI;

namespace Authentication
{
    public class AuthUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button googleLoginButton;
        public Button facebookLoginButton;
        public Button logoutButton;
        public Text statusText;
        public GameObject loadingIndicator;

        [Header("Provider Settings")]
        public string googleClientId;
        public string googleClientSecret;
        public string googleRedirectUri;
        
        public string facebookClientId;
        public string facebookClientSecret;
        public string facebookRedirectUri;

        private AuthenticationManager _authManager;
        private GoogleOAuth2Provider _googleProvider;
        private FacebookOAuth2Provider _facebookProvider;

        private void Start()
        {
            InitializeAuthSystem();
            SetupUI();
        }

        private void InitializeAuthSystem()
        {
            _authManager = AuthenticationManager.Instance;
            _authManager.OnAuthStateChanged += OnAuthStateChanged;
            _authManager.OnError += OnError;

            // Initialize providers
            _googleProvider = new GoogleOAuth2Provider(googleClientId, googleClientSecret, googleRedirectUri);
            _facebookProvider = new FacebookOAuth2Provider(facebookClientId, facebookClientSecret, facebookRedirectUri);

            // Register providers
            _authManager.RegisterProvider("Google", _googleProvider);
            _authManager.RegisterProvider("Facebook", _facebookProvider);
        }

        private void SetupUI()
        {
            if (googleLoginButton != null)
                googleLoginButton.onClick.AddListener(() => LoginWithProvider("Google"));
            
            if (facebookLoginButton != null)
                facebookLoginButton.onClick.AddListener(() => LoginWithProvider("Facebook"));
            
            if (logoutButton != null)
                logoutButton.onClick.AddListener(Logout);

            UpdateUI();
        }

        private void LoginWithProvider(string providerName)
        {
            ShowLoading(true);
            _authManager.Authenticate(providerName, (success, message) =>
            {
                ShowLoading(false);
                if (success)
                {
                    Debug.Log($"Successfully logged in with {providerName}");
                }
                else
                {
                    Debug.LogError($"Failed to log in with {providerName}: {message}");
                }
            });
        }

        private void Logout()
        {
            _authManager.LogoutAll();
            Debug.Log("Logged out from all providers");
        }

        private void OnAuthStateChanged(AuthState state)
        {
            UpdateUI();
        }

        private void OnError(string error)
        {
            if (statusText != null)
                statusText.text = $"Error: {error}";
        }

        private void UpdateUI()
        {
            if (statusText != null)
            {
                switch (_authManager.GetAuthState())
                {
                    case AuthState.NotAuthenticated:
                        statusText.text = "Not authenticated";
                        break;
                    case AuthState.Authenticating:
                        statusText.text = "Authenticating...";
                        break;
                    case AuthState.Authenticated:
                        statusText.text = "Authenticated";
                        break;
                    case AuthState.Error:
                        statusText.text = "Authentication error";
                        break;
                }
            }

            bool isAuthenticated = _authManager.IsAuthenticated();
            
            if (googleLoginButton != null)
                googleLoginButton.interactable = !isAuthenticated;
            
            if (facebookLoginButton != null)
                facebookLoginButton.interactable = !isAuthenticated;
            
            if (logoutButton != null)
                logoutButton.interactable = isAuthenticated;
        }

        private void ShowLoading(bool show)
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(show);
        }

        private void OnDestroy()
        {
            if (_authManager != null)
            {
                _authManager.OnAuthStateChanged -= OnAuthStateChanged;
                _authManager.OnError -= OnError;
            }
        }
    }
}