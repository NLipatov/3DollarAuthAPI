using AuthAPI.DB.Models;

namespace AuthAPI.Services.RefreshHistoryService;

public interface IJwtRefreshHistoryService
{
    public Task<List<RefreshTokenEvent>> GetUserHistory(string username);
}