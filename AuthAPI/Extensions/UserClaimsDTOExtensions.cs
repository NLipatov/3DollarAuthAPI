using System.Security.Claims;
using AuthAPI.DB.Models;
using EthachatShared.Models.Authentication.Models.AuthenticatedUserRepresentation.Claims;

namespace AuthAPI.Extensions
{
    public static class UserClaimsDtoExtensions
    {
        public static List<UserClaim> ExtractClaims(this List<UserClaimsDto> dtos)
        {
            List<UserClaim> claims = new();

            foreach (var dto in dtos)
            {
                string? claimType = typeof(ClaimTypes)?.GetField(dto.Name)?.GetValue(null)?.ToString();
                if (!string.IsNullOrWhiteSpace(claimType))
                {
                    claims.Add(
                    new UserClaim
                    {
                        Name = dto.Name,
                        Value = dto.Value,
                        Type = claimType
                    });
                }
            }

            return claims;
        }
    }
}
