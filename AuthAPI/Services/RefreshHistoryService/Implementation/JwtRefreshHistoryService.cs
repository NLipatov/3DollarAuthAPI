using AuthAPI.DB.DBContext;
using AuthAPI.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Services.RefreshHistoryService.Implementation;

class JwtRefreshHistoryService : IJwtRefreshHistoryService
{
    private readonly AuthContext _context;

    public JwtRefreshHistoryService(AuthContext context)
    {
        _context = context;
    }
    public async Task<List<RefreshTokenEvent>> GetUserHistory(string username)
    {
        var history = await _context.RefreshTokenEvents
            .Where(x => x.User.Username == username)
            .ToListAsync();

        return history;
    }
}