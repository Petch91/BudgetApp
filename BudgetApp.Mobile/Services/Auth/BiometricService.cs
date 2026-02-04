using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using Serilog;

namespace BudgetApp.Mobile.Services.Auth;

public class BiometricService : IBiometricService
{
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            return await CrossFingerprint.Current.IsAvailableAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking biometric availability");
            return false;
        }
    }

    public async Task<BiometricType> GetBiometricTypeAsync()
    {
        try
        {
            var authType = await CrossFingerprint.Current.GetAuthenticationTypeAsync();

            return authType switch
            {
                AuthenticationType.Fingerprint => BiometricType.Fingerprint,
                AuthenticationType.Face => BiometricType.FaceId,
                AuthenticationType.Iris => BiometricType.Iris,
                _ => BiometricType.None
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting biometric type");
            return BiometricType.None;
        }
    }

    public async Task<bool> AuthenticateAsync(string reason)
    {
        try
        {
            var request = new AuthenticationRequestConfiguration(
                "BudgetApp",
                reason)
            {
                AllowAlternativeAuthentication = true,
                ConfirmationRequired = false
            };

            var result = await CrossFingerprint.Current.AuthenticateAsync(request);

            if (result.Authenticated)
            {
                Log.Information("Biometric authentication successful");
                return true;
            }

            Log.Warning("Biometric authentication failed: {Status}", result.Status);
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during biometric authentication");
            return false;
        }
    }
}
