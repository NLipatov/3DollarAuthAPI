using AuthAPI.Models;
using AutoMapper.Internal;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace AuthAPI.DTOs.Claims
{
    public static class UserClaimsDTOExtensions
    {
        public static List<UserClaim> ExtractClaims(this List<UserClaimsDTO> dtos)
        {
            List<UserClaim> claims = new();

            foreach (var dto in dtos)
            {
                string? claimType = typeof(ClaimTypes)?.GetField(dto.Name)?.GetValue(null)?.ToString();
                if(!string.IsNullOrWhiteSpace(claimType))
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
