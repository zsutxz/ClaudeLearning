# OAuth2 Authentication System

## Overview
This authentication system provides a flexible and secure way to implement OAuth2 authentication in Unity applications. It supports multiple providers and handles token management, refresh, and secure storage.

## Features
- Support for multiple OAuth2 providers (Google, Facebook, and custom providers)
- Secure token storage and management
- Automatic token refresh before expiration
- Singleton pattern for easy access
- Event-driven architecture for authentication state changes
- UI controller for easy integration

## Architecture
The system is composed of several key components:

1. **AuthenticationManager**: The main singleton that coordinates authentication operations
2. **OAuth2Provider**: Abstract base class for OAuth2 providers
3. **TokenManager**: Handles secure storage and retrieval of tokens
4. **AuthUIController**: UI component for handling user authentication interactions

## Setup

### 1. Installation
The system requires no additional packages beyond standard Unity libraries.

### 2. Provider Configuration
To configure providers, you need to:
1. Create an application in the provider's developer console
2. Obtain client ID and client secret
3. Configure redirect URIs
4. Set these values in the AuthUIController component

### 3. Scene Setup
1. Create an empty GameObject in your scene
2. Attach the AuthUIController script
3. Configure the provider settings in the inspector
4. Connect UI elements (buttons, text, etc.)

## Usage

### Basic Authentication Flow
```csharp
// Get the authentication manager instance
var authManager = AuthenticationManager.Instance;

// Register providers
var googleProvider = new GoogleOAuth2Provider(clientId, clientSecret, redirectUri);
authManager.RegisterProvider("Google", googleProvider);

// Authenticate
authManager.Authenticate("Google", (success, message) => {
    if (success) {
        Debug.Log("Authentication successful!");
    } else {
        Debug.LogError($"Authentication failed: {message}");
    }
});
```

### Checking Authentication Status
```csharp
var authManager = AuthenticationManager.Instance;
if (authManager.IsAuthenticated()) {
    // User is authenticated
    string token = authManager.GetAccessToken("Google");
    // Use token for API calls
}
```

### Logout
```csharp
var authManager = AuthenticationManager.Instance;
authManager.Logout("Google");
// or
authManager.LogoutAll();
```

## Security Considerations
1. Tokens are stored using PlayerPrefs, which is not secure for production applications
2. In a production environment, consider using:
   - Encryption for token storage
   - Secure platform-specific storage (Keystore, Keychain)
   - Backend token validation
3. Always use HTTPS for communication with OAuth2 endpoints
4. Implement proper error handling and token refresh logic

## Extending to New Providers
To add support for a new OAuth2 provider:

1. Create a new class that inherits from OAuth2Provider
2. Implement the Authenticate, RefreshToken, and Logout methods
3. Register the provider with the AuthenticationManager

```csharp
public class CustomOAuth2Provider : OAuth2Provider
{
    public CustomOAuth2Provider(string clientId, string clientSecret, string redirectUri) 
        : base(clientId, clientSecret, redirectUri)
    {
    }

    public override void Authenticate(Action<bool, string, string> callback)
    {
        // Implement authentication flow
    }

    public override void RefreshToken(Action<bool, string, string> callback)
    {
        // Implement token refresh
    }

    public override void Logout()
    {
        // Implement logout
    }
}
```

## Testing
The system includes simulated authentication flows for testing purposes. In a real application, you would replace these simulations with actual OAuth2 flows.

## Troubleshooting
- Ensure client IDs and secrets are correct
- Verify redirect URIs are properly configured in both the application and provider console
- Check network connectivity
- Confirm the system clock is accurate (important for token validation)

## License
This authentication system is provided as-is for educational and development purposes.