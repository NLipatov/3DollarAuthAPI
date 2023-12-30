using AuthAPI.DB.Models;
using AuthAPI.DB.Models.Fido;
using LimpShared.Models.Users;

namespace AuthAPI.Services.UserArea.UserProvider;

internal static class IsUserExistDtoExtensions
{
    internal static IsUserExistDto AsUserExistDTO(this FidoUser fidoUser)
        => new()
        {
            IsExist = true,
            Username = fidoUser.Name
        };

    internal static IsUserExistDto AsUserExistDTO(this User user)
        => new()
        {
            IsExist = true,
            Username = user.Username
        };
}