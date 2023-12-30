using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using AuthAPI.DB.Models.Fido;
using AuthAPI.Services.UserArea.UserProvider;
using AuthAPI.Services.WebAuthn;
using Fido2NetLib;
using Fido2NetLib.Objects;
using EthachatShared.Models.Authentication.Models;
using EthachatShared.Models.Authentication.Models.Credentials.Implementation;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Controllers;

[Route("api/[controller]")]
public class WebAuthnController : Controller
{
    private static string _jsonOptions = string.Empty;
    private IFido2 _fido2;

    public static IMetadataService Mds;

    //public static readonly DevelopmentInMemoryStore DemoStorage = new DevelopmentInMemoryStore();
    private readonly IUserProvider _userProvider;

    public WebAuthnController
    (
        IFido2 fido2,
        IUserProvider userProvider
    )
    {
        _fido2 = fido2;
        _userProvider = userProvider;
    }

    private string FormatException(Exception e)
    {
        return string.Format("{0}{1}", e.Message,
            e.InnerException != null ? " (" + e.InnerException.Message + ")" : "");
    }

    [HttpPost("makeCredentialOptions")]
    public async Task<JsonResult> MakeCredentialOptions([FromForm] string username,
        [FromForm] string displayName,
        [FromForm] string attType,
        [FromForm] string authType,
        [FromForm] string residentKey,
        [FromForm] string userVerification)
    {
        try
        {
            if (string.IsNullOrEmpty(username))
            {
                username = $"{displayName} (Usernameless user created at {DateTime.UtcNow})";
            }

            // 1. Get user from DB by username (in our example, auto create missing users)
            //var user = DemoStorage.GetOrAddUser(username, () => new Fido2User
            //{
            //    DisplayName = displayName,
            //    Name = username,
            //    Id = Encoding.UTF8.GetBytes(username) // byte representation of userID is required
            //});

            FidoUser? user = await _userProvider.GetFidoUserByUsernameAsync(username);
            if (user == null)
            {
                user = await _userProvider.RegisterFidoUser(username, displayName);
            }

            // 2. Get user existing keys by username
            //var existingKeys = DemoStorage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

            List<PublicKeyCredentialDescriptor> existingKeys = (await _userProvider.GetCredentialsByUserAsync(user))
                .Select(c => c.Descriptor).ToList();

            // 3. Create options
            var authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = false,
                UserVerification = userVerification.ToEnum<UserVerificationRequirement>()
            };

            if (!string.IsNullOrEmpty(authType))
                authenticatorSelection.AuthenticatorAttachment = authType.ToEnum<AuthenticatorAttachment>();

            var exts = new AuthenticationExtensionsClientInputs()
            {
                Extensions = true,
                UserVerificationMethod = true,
            };

            var options = _fido2.RequestNewCredential(new Fido2User
            {
                Id = user.UserId,
                DisplayName = user.DisplayName,
                Name = user.Name,
            }, existingKeys, authenticatorSelection, attType.ToEnum<AttestationConveyancePreference>(), exts);

            // 4. Temporarily store options, session/in-memory cache/redis/db
            //HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());
            _jsonOptions = options.ToJson();

            // 5. return options to client
            return Json(options);
        }
        catch (Exception e)
        {
            return Json(new CredentialCreateOptions { Status = "error", ErrorMessage = FormatException(e) });
        }
    }

    [HttpPost("makeCredential")]
    public async Task<JsonResult> MakeCredential([FromBody] AuthenticatorAttestationRawResponse attestationResponse,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. get the options we sent the client
            //var jsonOptions = HttpContext.Session.GetString("fido2.attestationOptions");
            var options = CredentialCreateOptions.FromJson(_jsonOptions);

            // 2. Create callback so that lib can verify credential id is unique to this user
            IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, cancellationToken) =>
            {
                //var users = await DemoStorage.GetUsersByCredentialIdAsync(args.CredentialId, cancellationToken);
                var users = await _userProvider.GetUsersByCredentialIdAsync(args.CredentialId, cancellationToken);
                if (users.Count > 0)
                    return false;

                return true;
            };

            // 2. Verify and make the credentials
            var success = await _fido2.MakeNewCredentialAsync(attestationResponse, options, callback,
                cancellationToken: cancellationToken);

            // 3. Store the credentials in db
            //DemoStorage.AddCredentialToUser(options.User, new StoredCredential
            //{
            //    Descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId),
            //    PublicKey = success.Result.PublicKey,
            //    UserHandle = success.Result.User.Id,
            //    SignatureCounter = success.Result.Counter,
            //    CredType = success.Result.CredType,
            //    RegDate = DateTime.Now,
            //    AaGuid = success.Result.Aaguid
            //});

            await _userProvider.AddCredentialToUser(new FidoUser
                {
                    Name = options.User.Name,
                    DisplayName = options.User.DisplayName,
                    UserId = options.User.Id
                },
                new FidoCredential
                {
                    Descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId),
                    PublicKey = success.Result.PublicKey,
                    UserHandle = success.Result.User.Id,
                    SignatureCounter = success.Result.Counter,
                    CredType = success.Result.CredType,
                    RegDate = DateTime.UtcNow,
                    AaGuid = success.Result.Aaguid
                });

            // Remove Certificates from success because System.Text.Json cannot serialize them properly. See https://github.com/passwordless-lib/fido2-net-lib/issues/328
            success.Result.AttestationCertificate = null;
            success.Result.AttestationCertificateChain = null;

            // 4. return "ok" to the client
            return Json(success);
        }
        catch (Exception e)
        {
            return Json(new Fido2.CredentialMakeResult(status: "error", errorMessage: FormatException(e),
                result: null));
        }
    }

    [HttpPost("assertionOptions")]
    public async Task<ActionResult> AssertionOptionsPost([FromForm] string username, [FromForm] string userVerification)
    {
        try
        {
            var existingCredentials = new List<PublicKeyCredentialDescriptor>();

            if (!string.IsNullOrEmpty(username))
            {
                // 1. Get user from DB
                //var user = DemoStorage.GetUser(username) ?? throw new ArgumentException("Username was not registered");
                FidoUser user = await _userProvider.GetFidoUserByUsernameAsync(username);

                // 2. Get registered credentials from database
                //existingCredentials = DemoStorage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();
                var existingCredential = (await _userProvider.GetCredentialsByUserAsync(user))
                    .Select(c => c.Descriptor)
                    .ToList();
            }

            var exts = new AuthenticationExtensionsClientInputs()
            {
                UserVerificationMethod = true
            };

            // 3. Create options
            var uv = string.IsNullOrEmpty(userVerification)
                ? UserVerificationRequirement.Discouraged
                : userVerification.ToEnum<UserVerificationRequirement>();
            var options = _fido2.GetAssertionOptions(
                existingCredentials,
                uv,
                exts
            );

            // 4. Temporarily store options, session/in-memory cache/redis/db
            OptionsStorage.usernameToOptions.TryAdd(username, options);

            // 5. Return options to client
            return Json(options);
        }

        catch (Exception e)
        {
            return Json(new AssertionOptions { Status = "error", ErrorMessage = FormatException(e) });
        }
    }

    [HttpPost("makeAssertion/{username}")]
    public async Task<JsonResult> MakeAssertion([FromBody] AuthenticatorAssertionRawResponse clientResponse,
        string username, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get the assertion options we sent the client
            OptionsStorage.usernameToOptions.TryGetValue(username, out var options);
            OptionsStorage.usernameToOptions.TryRemove(username, out _);

            await _userProvider.ResetCounter(clientResponse.Id);
            // 2. Get registered credential from database
            //var creds = DemoStorage.GetCredentialById(clientResponse.Id) ?? throw new Exception("Unknown credentials");
            var creds = await _userProvider.GetCredentialById(clientResponse.Id) ??
                        throw new Exception("Unknown credentials");

            // 3. Get credential counter from database
            var storedCounter = creds.SignatureCounter;

            // 4. Create callback to check if userhandle owns the credentialId
            IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
            {
                //var storedCreds = await DemoStorage.GetCredentialsByUserHandleAsync(args.UserHandle, cancellationToken);
                var storedCreds =
                    await _userProvider.GetCredentialsByUserHandleAsync(args.UserHandle, cancellationToken);
                return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
            };

            // 5. Make the assertion
            var res = await _fido2.MakeAssertionAsync(clientResponse, options, creds.PublicKey, storedCounter, callback,
                cancellationToken: cancellationToken);

            //6. Update counter
            await _userProvider.UpdateCounter(clientResponse.Id, 0);
            
            // 7. return OK to client
            return Json(res);
        }
        catch (Exception e)
        {
            return Json(new AssertionVerificationResult { Status = "error", ErrorMessage = FormatException(e) });
        }
    }

    [HttpPost("renewCredentialId")]
    public async Task<AuthResult> RenewCredentialId([FromBody]RefreshCredentialIdDto credentialIdDto)
    {
        try
        {
            var credentialIdBytes = Convert.FromBase64String(credentialIdDto.CredentialId);
            await _userProvider
                .UpdateCounter(credentialIdBytes, credentialIdDto.Counter);

            return new AuthResult
            {
                Result = AuthResultType.Success
            };
        }
        catch (Exception e)
        {
            return new AuthResult
            {
                Result = AuthResultType.Fail
            };
        }
    }

    [HttpGet("username/{credentialId}")]
    public async Task<string> GetUsernameByCredentialId(string credentialId)
    {
        var credentialIdBytes = Convert.FromBase64String(Uri.UnescapeDataString(credentialId));
        return await _userProvider.GetUsernameByCredentialId(credentialIdBytes);
    }

    [HttpPost("validate")]
    public async Task<bool> Validate([FromBody] WebAuthnPair webAuthnPair)
    {
        var credentialIdBytes = Convert.FromBase64String(Uri.UnescapeDataString(webAuthnPair.CredentialId));
        return await _userProvider.ValidateCredentials(credentialIdBytes, webAuthnPair.Counter);
    }
}