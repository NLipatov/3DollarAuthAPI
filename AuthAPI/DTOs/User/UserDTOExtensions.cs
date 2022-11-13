using AuthAPI.Models;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using UserRich = AuthAPI.Models.User;

namespace AuthAPI.DTOs.User
{
    public static class UserDTOExtensions
    {
        public static List<UserClaim> ExtractClaims(this UserDTO dto)
        {
            List<UserClaim> claims = new();

            if(dto.Email != null)
            {
                claims.Add(new()
                {
                    Type = ClaimTypes.Email,
                    Name = "Email",
                    Value = dto.Email
                });
            }

            if(dto.Name!= null)
            {
                claims.Add(new()
                {
                    Type = ClaimTypes.Name,
                    Name = "Name",
                    Value = dto.Name
                });
            }

            if(dto.Surname != null)
            {
                claims.Add(new()
                {
                    Type = ClaimTypes.Surname,
                    Name = "Surname",
                    Value = dto.Surname
                });
            }

            if(dto.Phonename != null)
            {
                claims.Add(new()
                {
                    Type = ClaimTypes.MobilePhone,
                    Name = "Phonenumber",
                    Value = dto.Phonename
                });
            }

            return claims;
        }
    }
}
