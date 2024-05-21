using Ethachat.Auth.Domain.Models;
using Ethachat.Auth.Infrastructure.DB.DBContext;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Services.RefreshHistoryService.Implementation;

class JwtRefreshHistoryService : IJwtRefreshHistoryService
{
    private readonly AuthContext _context;

    public JwtRefreshHistoryService(AuthContext context)
    {
        _context = context;
    }
    public async Task<List<UserAccessRefreshEventLog>> GetUserHistory(string username)
    {
        var history = await _context.UserAccessRefreshEventLogs
            .Where(x => x.User.Username == username)
            .ToListAsync();

        return history;
    }
}