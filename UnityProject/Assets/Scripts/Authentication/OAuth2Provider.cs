using System;
using UnityEngine;

namespace Authentication
{
    public abstract class OAuth2Provider
    {
        protected string _clientId;
        protected string _clientSecret;
        protected string _redirectUri;
        protected string _accessToken;
        protected string _refreshToken;
        protected DateTime _tokenExpiry;

        public OAuth2Provider(string clientId, string clientSecret, string redirectUri)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
        }

        public abstract void Authenticate(Action<bool, string, string> callback);
        public abstract void RefreshToken(Action<bool, string, string> callback);
        public abstract void Logout();

        public string GetAccessToken()
        {
            return _accessToken;
        }

        public bool IsTokenExpired()
        {
            return DateTime.Now >= _tokenExpiry;
        }

        protected void SetTokens(string accessToken, string refreshToken, int expiresIn)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _tokenExpiry = DateTime.Now.AddSeconds(expiresIn - 300); // Refresh 5 minutes before actual expiry
        }

        protected string GenerateState()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}