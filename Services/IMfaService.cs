namespace SAProject.Services
{
    public interface IMfaService
    {
        Task<string> GenerateTokenAsync(string userId);
        Task<bool> ValidateTokenAsync(string userId, string token);
        Task<bool> IsMfaRequiredAsync(string userId);
    }

}
