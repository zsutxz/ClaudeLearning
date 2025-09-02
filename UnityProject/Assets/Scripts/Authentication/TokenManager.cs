using System;
using System.Collections.Generic;
using UnityEngine;

namespace Authentication
{
    public class TokenManager
    {
        private Dictionary<string, string> _tokens = new Dictionary<string, string>();
        private Dictionary<string, string> _refreshTokens = new Dictionary<string, string>();
        private Dictionary<string, DateTime> _tokenExpiries = new Dictionary<string, DateTime>();

        public void StoreToken(string providerName, string accessToken)
        {
            // In a production environment, you should encrypt the token before storing
            // For simplicity, we're storing it directly here
            PlayerPrefs.SetString($"auth_token_{providerName}", accessToken);
            PlayerPrefs.Save();
            
            _tokens[providerName] = accessToken;
        }

        public void StoreTokenWithExpiry(string providerName, string accessToken, string refreshToken, DateTime expiry)
        {
            // In a production environment, you should encrypt these values before storing
            PlayerPrefs.SetString($"auth_token_{providerName}", accessToken);
            PlayerPrefs.SetString($"refresh_token_{providerName}", refreshToken);
            PlayerPrefs.SetString($"token_expiry_{providerName}", expiry.ToString("o")); // Round-trip format
            PlayerPrefs.Save();
            
            _tokens[providerName] = accessToken;
            _refreshTokens[providerName] = refreshToken;
            _tokenExpiries[providerName] = expiry;
        }

        public string GetToken(string providerName)
        {
            if (_tokens.ContainsKey(providerName))
            {
                return _tokens[providerName];
            }

            // Try to load from PlayerPrefs
            string token = PlayerPrefs.GetString($"auth_token_{providerName}", null);
            if (!string.IsNullOrEmpty(token))
            {
                _tokens[providerName] = token;
                return token;
            }

            return null;
        }

        public string GetRefreshToken(string providerName)
        {
            if (_refreshTokens.ContainsKey(providerName))
            {
                return _refreshTokens[providerName];
            }

            // Try to load from PlayerPrefs
            string refreshToken = PlayerPrefs.GetString($"refresh_token_{providerName}", null);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                _refreshTokens[providerName] = refreshToken;
                return refreshToken;
            }

            return null;
        }

        public DateTime? GetTokenExpiry(string providerName)
        {
            if (_tokenExpiries.ContainsKey(providerName))
            {
                return _tokenExpiries[providerName];
            }

            // Try to load from PlayerPrefs
            string expiryString = PlayerPrefs.GetString($"token_expiry_{providerName}", null);
            if (!string.IsNullOrEmpty(expiryString))
            {
                if (DateTime.TryParseExact(expiryString, "o", null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime expiry))
                {
                    _tokenExpiries[providerName] = expiry;
                    return expiry;
                }
            }

            return null;
        }

        public bool IsTokenExpired(string providerName)
        {
            DateTime? expiry = GetTokenExpiry(providerName);
            if (expiry.HasValue)
            {
                return DateTime.Now >= expiry.Value;
            }
            return true; // If we don't have expiry info, assume it's expired
        }

        public void RemoveToken(string providerName)
        {
            PlayerPrefs.DeleteKey($"auth_token_{providerName}");
            PlayerPrefs.DeleteKey($"refresh_token_{providerName}");
            PlayerPrefs.DeleteKey($"token_expiry_{providerName}");
            PlayerPrefs.Save();
            
            _tokens.Remove(providerName);
            _refreshTokens.Remove(providerName);
            _tokenExpiries.Remove(providerName);
        }

        public void ClearAllTokens()
        {
            // Get all keys that start with auth_token_ and remove them
            List<string> keysToRemove = new List<string>();
            foreach (string key in PlayerPrefs.GetStringArrayKeys())
            {
                if (key.StartsWith("auth_token_") || key.StartsWith("refresh_token_") || key.StartsWith("token_expiry_"))
                {
                    keysToRemove.Add(key);
                }
            }
            
            foreach (string key in keysToRemove)
            {
                PlayerPrefs.DeleteKey(key);
            }
            
            PlayerPrefs.Save();
            
            _tokens.Clear();
            _refreshTokens.Clear();
            _tokenExpiries.Clear();
        }
    }

    // Extension method to get all PlayerPrefs keys (since Unity doesn't provide this by default)
    public static class PlayerPrefsExtensions
    {
        public static string[] GetStringArrayKeys()
        {
            // This is a simplified implementation
            // In a real scenario, you might want to maintain a list of keys
            return new string[0];
        }
    }
}