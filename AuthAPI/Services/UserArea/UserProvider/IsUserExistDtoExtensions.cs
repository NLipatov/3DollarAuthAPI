using Ethachat.Auth.Domain.Models;
using Ethachat.Auth.Domain.Models.Fido;
using EthachatShared.Models.Users;

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