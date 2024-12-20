using IdentityProviderMicroservice.User;

namespace IdentityProviderMicroservice.Services
{
    public interface IIdentityProviderService
    {
        Task<AuthResult> VerifyUserPassword(string email, string password);
    }
}
