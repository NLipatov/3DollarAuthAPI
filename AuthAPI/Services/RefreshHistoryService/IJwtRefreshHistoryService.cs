using Ethachat.Auth.Domain.Models;

namespace AuthAPI.Services.RefreshHistoryService;

public interface IJwtRefreshHistoryService
{
    public Task<List<UserAccessRefreshEventLog>> GetUserHistory(string username);
}