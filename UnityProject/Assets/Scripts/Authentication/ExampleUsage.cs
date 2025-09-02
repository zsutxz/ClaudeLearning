using UnityEngine;

namespace Authentication
{
    /// <summary>
    /// Example usage of the authentication system
    /// </summary>
    public class ExampleUsage : MonoBehaviour
    {
        [Header("Provider Configuration")]
        public string googleClientId = "YOUR_GOOGLE_CLIENT_ID";
        public string googleClientSecret = "YOUR_GOOGLE_CLIENT_SECRET";
        public string googleRedirectUri = "YOUR_GOOGLE_REDIRECT_URI";
        
        public string facebookClientId = "YOUR_FACEBOOK_CLIENT_ID";
        public string facebookClientSecret = "YOUR_FACEBOOK_CLIENT_SECRET";
        public string facebookRedirectUri = "YOUR_FACEBOOK_REDIRECT_URI";

        private AuthenticationManager _authManager;
        
        private void Start()
        {
            // Initialize the authentication system
            InitializeAuthentication();
            
            // Subscribe to authentication events
            _authManager.OnAuthStateChanged += OnAuthStateChanged;
            _authManager.OnError += OnAuthenticationError;
        }

        private void InitializeAuthentication()
        {
            // Get the singleton instance
            _authManager = AuthenticationManager.Instance;
            
            // Create and register providers
            var googleProvider = new GoogleOAuth2Provider(googleClientId, googleClientSecret, googleRedirectUri);
            var facebookProvider = new FacebookOAuth2Provider(facebookClientId, facebookClientSecret, facebookRedirectUri);
            
            _authManager.RegisterProvider("Google", googleProvider);
            _authManager.RegisterProvider("Facebook", facebookProvider);
        }

        /// <summary>
        /// Example method to authenticate with Google
        /// </summary>
        public void LoginWithGoogle()
        {
            _authManager.Authenticate("Google", (success, message) =>
            {
                if (success)
                {
                    Debug.Log("Successfully authenticated with Google!");
                    // You can now use the access token for API calls
                    string accessToken = _authManager.GetAccessToken("Google");
                }
                else
                {
                    Debug.LogError($"Google authentication failed: {message}");
                }
            });
        }

        /// <summary>
        /// Example method to authenticate with Facebook
        /// </summary>
        public void LoginWithFacebook()
        {
            _authManager.Authenticate("Facebook", (success, message) =>
            {
                if (success)
                {
                    Debug.Log("Successfully authenticated with Facebook!");
                    // You can now use the access token for API calls
                    string accessToken = _authManager.GetAccessToken("Facebook");
                }
                else
                {
                    Debug.LogError($"Facebook authentication failed: {message}");
                }
            });
        }

        /// <summary>
        /// Example method to logout from all providers
        /// </summary>
        public void Logout()
        {
            _authManager.LogoutAll();
            Debug.Log("Logged out from all providers");
        }

        /// <summary>
        /// Handle authentication state changes
        /// </summary>
        private void OnAuthStateChanged(AuthState state)
        {
            switch (state)
            {
                case AuthState.NotAuthenticated:
                    Debug.Log("User is not authenticated");
                    break;
                case AuthState.Authenticating:
                    Debug.Log("Authentication in progress...");
                    break;
                case AuthState.Authenticated:
                    Debug.Log("User is authenticated");
                    break;
                case AuthState.Error:
                    Debug.Log("Authentication error occurred");
                    break;
            }
        }

        /// <summary>
        /// Handle authentication errors
        /// </summary>
        private void OnAuthenticationError(string error)
        {
            Debug.LogError($"Authentication error: {error}");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_authManager != null)
            {
                _authManager.OnAuthStateChanged -= OnAuthStateChanged;
                _authManager.OnError -= OnAuthenticationError;
            }
        }
    }
}