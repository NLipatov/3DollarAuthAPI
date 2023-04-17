using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthAPI;
using AuthAPI.DB.DBContext;
using AuthAPI.DTOs.User;
using AuthAPI.Models;
using AuthAPI.Services.Cryptography;
using AuthAPI.Services.UserCredentialsValidation;
using AuthAPI.Services.UserProvider;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Moq;
using Xunit;

namespace AuthAPI.Tests
{
    public class UserCredentialsValidatorTest
    {
        

        [Theory]
        [InlineData("testuser","testpassword", ValidationResult.Success)]
        public async Task ValidateCredentials_ReturnsSuccess_WhenValidCredentials(string name, string password, ValidationResult expected)
        {
            //Arrange
            var userProviderMock = new Mock<IUserProvider>();
            var cryptographyHelperMock = new Mock<ICryptographyHelper>();
            var cryptographyHelper = new CryptographyHelper();

            var request = new UserDTO { Username = name, Password = password };

            cryptographyHelper.CreateHashAndSalt(password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User { Username = name, PasswordHash = passwordHash, PasswordSalt = passwordSalt };

            userProviderMock.Setup(x => x.GetUserByUsernameAsync(request.Username)).ReturnsAsync(user);
            cryptographyHelperMock.Setup(x => x.VerifyHash(request.Password!, user.PasswordHash, user.PasswordSalt)).Returns(true);


            //Act
            var validator = new UserCredentialsValidator(userProviderMock.Object, cryptographyHelperMock.Object);
            var actual = await validator.ValidateCredentials(request);

            //Assert

            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("testuser", "testpassword", ValidationResult.WrongUsername)]
        public async Task ValidateCredentials_ReturnsWrong_WhenUserNull(string name, string password, ValidationResult expected)
        {
            //Arrange
            var userProviderMock = new Mock<IUserProvider>();
            var cryptographyHelperMock = new Mock<ICryptographyHelper>();

            var request = new UserDTO { Username = name, Password = password };

            userProviderMock.Setup(x => x.GetUserByUsernameAsync(request.Username)).ReturnsAsync((User)null);
          
            //Act
            var validator = new UserCredentialsValidator(userProviderMock.Object, cryptographyHelperMock.Object);
            var actual = await validator.ValidateCredentials(request);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("testuser", "testpassword", ValidationResult.WrongPassword)]
        public async Task ValidateCredentials_ReturnsWrong_WhenPasswordWrong(string name, string password, ValidationResult expected)
        {
            //Arrange
            var userProviderMock = new Mock<IUserProvider>();
            var cryptographyHelperMock = new Mock<ICryptographyHelper>();
            var cryptographyHelper = new CryptographyHelper();

            var request = new UserDTO { Username = name, Password = password };

            cryptographyHelper.CreateHashAndSalt(password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User { Username = name, PasswordHash = passwordHash, PasswordSalt = passwordSalt };

            userProviderMock.Setup(x => x.GetUserByUsernameAsync(request.Username)).ReturnsAsync(user);
            cryptographyHelperMock.Setup(x => x.VerifyHash(request.Password!, user.PasswordHash, user.PasswordSalt)).Returns(false);


            //Act
            var validator = new UserCredentialsValidator(userProviderMock.Object, cryptographyHelperMock.Object);
            var actual = await validator.ValidateCredentials(request);

            //Assert

            Assert.Equal(expected, actual);
        }

    }
}
