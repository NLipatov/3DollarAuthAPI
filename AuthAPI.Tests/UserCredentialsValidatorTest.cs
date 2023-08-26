using AuthAPI.DB.Models;
using AuthAPI.Services.Cryptography;
using AuthAPI.Services.UserArea.UserProvider;
using AuthAPI.Services.UserCredentialsValidation;
using LimpShared.Models.Authentication.Models.UserAuthentication;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

namespace AuthAPI.Tests
{
    public class UserCredentialsValidatorTest
    {
        private (Mock<IUserProvider> userProviderMock, Mock<ICryptographyHelper> cryptographyHelperMock, UserAuthentication request, User user) Arrange(string name, string password, bool passwordMatch)
        {
            var userProviderMock = new Mock<IUserProvider>();
            var cryptographyHelperMock = new Mock<ICryptographyHelper>();
            var cryptographyHelper = new CryptographyHelper();

            var request = new UserAuthentication { Username = name, Password = password };

            cryptographyHelper.CreateHashAndSalt(password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User { Username = name, PasswordHash = passwordHash, PasswordSalt = passwordSalt };

            userProviderMock.Setup(x => x.GetUserByUsernameAsync(request.Username)).ReturnsAsync(user);
            cryptographyHelperMock.Setup(x => x.VerifyHash(request.Password!, user.PasswordHash, user.PasswordSalt)).Returns(passwordMatch);

            return (userProviderMock, cryptographyHelperMock, request, user);
        }

        [Theory]
        [InlineData("testuser", "testpassword", ValidationResult.Success)]
        public async Task ValidateCredentials_ReturnsSuccess_WhenValidCredentials(string name, string password, ValidationResult expected)
        {
            var (userProviderMock, cryptographyHelperMock, request, user) = Arrange(name, password, true);

            var validator = new UserCredentialsValidator(userProviderMock.Object, cryptographyHelperMock.Object);
            var actual = await validator.ValidateCredentials(request);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("testuser", "testpassword", ValidationResult.WrongUsername)]
        public async Task ValidateCredentials_ReturnsWrong_WhenUserNull(string name, string password, ValidationResult expected)
        {
            var (userProviderMock, cryptographyHelperMock, request, user) = Arrange(name, password, true);
            userProviderMock.Setup(x => x.GetUserByUsernameAsync(request.Username)).ReturnsAsync((User?)null);

            var validator = new UserCredentialsValidator(userProviderMock.Object, cryptographyHelperMock.Object);
            var actual = await validator.ValidateCredentials(request);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("testuser", "testpassword", ValidationResult.WrongPassword)]
        public async Task ValidateCredentials_ReturnsWrong_WhenPasswordWrong(string name, string password, ValidationResult expected)
        {
            var (userProviderMock, cryptographyHelperMock, request, user) = Arrange(name, password, false);

            var validator = new UserCredentialsValidator(userProviderMock.Object, cryptographyHelperMock.Object);
            var actual = await validator.ValidateCredentials(request);

            Assert.Equal(expected, actual);
        }
    }
}
