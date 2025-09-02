using System;
using System.Collections.Generic;
using UnityEngine;

namespace Authentication
{
    public class AuthenticationManager : MonoBehaviour
    {
        private static AuthenticationManager _instance;
        public static AuthenticationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject authManagerObject = new GameObject("AuthenticationManager");
                    _instance = authManagerObject.AddComponent<AuthenticationManager>();
                }
                return _instance;
            }
        }

        private Dictionary<string, OAuth2Provider> _providers = new Dictionary<string, OAuth2Provider>();
        private TokenManager _tokenManager;
        private AuthState _authState = AuthState.NotAuthenticated;

        public event Action<AuthState> OnAuthStateChanged;
        public event Action<string> OnError;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                _tokenManager = new TokenManager();
                InitializeProviders();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeProviders()
        {
            // Initialize default providers
            // Additional providers can be registered at runtime
        }

        public void RegisterProvider(string providerName, OAuth2Provider provider)
        {
            if (!_providers.ContainsKey(providerName))
            {
                _providers.Add(providerName, provider);
            }
        }

        public OAuth2Provider GetProvider(string providerName)
        {
            if (_providers.ContainsKey(providerName))
            {
                return _providers[providerName];
            }
            return null;
        }

        public void Authenticate(string providerName, Action<bool, string> callback)
        {
            if (_providers.ContainsKey(providerName))
            {
                _authState = AuthState.Authenticating;
                OnAuthStateChanged?.Invoke(_authState);

                _providers[providerName].Authenticate((success, token, error) =>
                {
                    if (success)
                    {
                        _tokenManager.StoreToken(providerName, token);
                        _authState = AuthState.Authenticated;
                        OnAuthStateChanged?.Invoke(_authState);
                        callback?.Invoke(true, "Authentication successful");
                    }
                    else
                    {
                        _authState = AuthState.NotAuthenticated;
                        OnAuthStateChanged?.Invoke(_authState);
                        OnError?.Invoke(error);
                        callback?.Invoke(false, error);
                    }
                });
            }
            else
            {
                string error = $"Provider {providerName} not found";
                OnError?.Invoke(error);
                callback?.Invoke(false, error);
            }
        }

        public bool IsAuthenticated()
        {
            return _authState == AuthState.Authenticated;
        }

        public AuthState GetAuthState()
        {
            return _authState;
        }

        public string GetAccessToken(string providerName)
        {
            return _tokenManager.GetToken(providerName);
        }

        public void Logout(string providerName)
        {
            if (_providers.ContainsKey(providerName))
            {
                _providers[providerName].Logout();
                _tokenManager.RemoveToken(providerName);
                _authState = AuthState.NotAuthenticated;
                OnAuthStateChanged?.Invoke(_authState);
            }
        }

        public void LogoutAll()
        {
            foreach (var provider in _providers.Values)
            {
                provider.Logout();
            }
            _tokenManager.ClearAllTokens();
            _authState = AuthState.NotAuthenticated;
            OnAuthStateChanged?.Invoke(_authState);
        }
    }

    public enum AuthState
    {
        NotAuthenticated,
        Authenticating,
        Authenticated,
        Error
    }
}