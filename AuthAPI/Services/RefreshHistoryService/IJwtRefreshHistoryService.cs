using AuthAPI.DB.Models;

namespace AuthAPI.Services.RefreshHistoryService;

public interface IJwtRefreshHistoryService
{
    public Task<List<RefreshTokenHistory>> GetUserHistory(string username);
}