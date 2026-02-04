namespace BudgetApp.Mobile.Services.Auth;

public interface IBiometricService
{
    Task<bool> IsAvailableAsync();
    Task<BiometricType> GetBiometricTypeAsync();
    Task<bool> AuthenticateAsync(string reason);
}

public enum BiometricType
{
    None,
    Fingerprint,
    FaceId,
    Iris
}
