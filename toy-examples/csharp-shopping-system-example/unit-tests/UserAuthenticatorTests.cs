using FakeItEasy;
using FakeItEasy.Configuration;
using FluentAssertions;
using NUnit.Framework;
using ShoppingSystem.Users;

namespace ShoppingSystem.Tests.Unit;

[TestFixture]
public class UserAuthenticatorTests
{
    #region Constants
    private const string NonExistentUsername = "non-existent-username";
    private const string RealUsername = "real-username";

    private const string CorrectPassword = "correct-password";
    private const string IncorrectPassword = "not-the-password";
    private const string HashedPassword = "hashed-password";

    private const string AuthenticationToken = "auth-token";
    #endregion

    #region Readonly Values
    private readonly User RealUser = new User(RealUsername, HashedPassword);
    #endregion

    #region Faked Dependencies
    private IPasswordHasher _PasswordHasher = null!;
    private ITokenService _TokenService = null!;
    private IUserRepository _UserRepository = null!;
    #endregion

    #region Call-Tos
    #region PasswordHasher
    private IReturnValueArgumentValidationConfiguration<string> CallToHashCorrectPassword =>
        A.CallTo(() => _PasswordHasher.HashPassword(CorrectPassword));
    private IReturnValueArgumentValidationConfiguration<bool> CallToVerifyCorrectPassword =>
        A.CallTo(() => _PasswordHasher.VerifyPassword(CorrectPassword, HashedPassword));
    private IReturnValueArgumentValidationConfiguration<bool> CallToVerifyIncorrectPassword =>
        A.CallTo(() => _PasswordHasher.VerifyPassword(IncorrectPassword, HashedPassword));
    #endregion

    #region TokenService
    private IReturnValueArgumentValidationConfiguration<Task<string>> CallToGenerateToken =>
        A.CallTo(() => _TokenService.GenerateTokenAsync(RealUser, A<CancellationToken>._));
    #endregion

    #region UserRepository
    private IReturnValueArgumentValidationConfiguration<Task<User?>> CallToGetUserByNonExistentUsername =>
        A.CallTo(() => _UserRepository.GetUserByUsernameAsync(NonExistentUsername, A<CancellationToken>._));
    private IReturnValueArgumentValidationConfiguration<Task<User?>> CallToGetUserByRealUsername =>
        A.CallTo(() => _UserRepository.GetUserByUsernameAsync(RealUsername, A<CancellationToken>._));
    private IReturnValueArgumentValidationConfiguration<Task> CallToSaveNewUser =>
        A.CallTo(() => _UserRepository.SaveUserAsync(new User(NonExistentUsername, HashedPassword), A<CancellationToken>._));
    #endregion
    #endregion

    #region Test Utils
    #endregion

    private UserAuthenticator _TestSubject = null!;

    [SetUp]
    public void SetUp()
    {
        // Create faked dependencies
        _PasswordHasher = A.Fake<IPasswordHasher>();
        _TokenService = A.Fake<ITokenService>();
        _UserRepository = A.Fake<IUserRepository>();

        // Setup call-tos
        CallToHashCorrectPassword.Returns(HashedPassword);
        CallToVerifyCorrectPassword.Returns(true);
        CallToVerifyIncorrectPassword.Returns(false);
        CallToGenerateToken.Returns(AuthenticationToken);
        CallToGetUserByNonExistentUsername.Returns(Task.FromResult<User?>(null));
        CallToGetUserByRealUsername.Returns(RealUser);
        CallToSaveNewUser.Returns(Task.CompletedTask);

        // Create test subject
        _TestSubject = new UserAuthenticator(
            _PasswordHasher,
            _TokenService,
            _UserRepository
        );
    }

    #region Tests per-method
    #region AuthenticateAsync tests
    [Test]
    public void AuthenticateAsync_WithEmptyUsername_ReturnsNull()
    {
        var result = _TestSubject.AuthenticateAsync(string.Empty, CorrectPassword, CancellationToken.None).Result;
        result.Should().BeNull();
    }

    [Test]
    public void AuthenticateAsync_WithEmptyPassword_ReturnsNull()
    {
        var result = _TestSubject.AuthenticateAsync(RealUsername, string.Empty, CancellationToken.None).Result;
        result.Should().BeNull();
    }

    [Test]
    public void AuthenticateAsync_UserWithUsernameDoesNotExist_ReturnsNull()
    {
        var result = _TestSubject.AuthenticateAsync(NonExistentUsername, CorrectPassword, CancellationToken.None).Result;
        result.Should().BeNull();

        CallToGetUserByNonExistentUsername.MustHaveHappenedOnceExactly();
    }

    [Test]
    public void AuthenticateAsync_IncorrectPassword_ReturnsNull()
    {
        var result = _TestSubject.AuthenticateAsync(RealUsername, IncorrectPassword, CancellationToken.None).Result;
        result.Should().BeNull();

        CallToGetUserByRealUsername.MustHaveHappenedOnceExactly();
        CallToVerifyIncorrectPassword.MustHaveHappenedOnceExactly();
    }

    [Test]
    public void AuthenticateAsync_CorrectPassword_ReturnsAuthToken()
    {
        var result = _TestSubject.AuthenticateAsync(RealUsername, CorrectPassword, CancellationToken.None).Result;
        result.Should().Be(AuthenticationToken);

        CallToGetUserByRealUsername.MustHaveHappenedOnceExactly();
        CallToVerifyCorrectPassword.MustHaveHappenedOnceExactly();
        CallToGenerateToken.MustHaveHappenedOnceExactly();
    }
    #endregion

    #region RegisterAsync tests
    [Test]
    public void RegisterAsync_WithEmptyUsername_ReturnsFalse()
    {
        var result = _TestSubject.Register(string.Empty, CorrectPassword, CancellationToken.None).Result;
        result.Should().BeFalse();
    }

    [Test]
    public void RegisterAsync_WithEmptyPassword_ReturnsFalse()
    {
        var result = _TestSubject.Register(RealUsername, string.Empty, CancellationToken.None).Result;
        result.Should().BeFalse();
    }

    [Test]
    public void RegisterAsync_UserWithUsernameAlreadyExists_ReturnsFalse()
    {
        CallToGetUserByRealUsername.Returns(RealUser);

        var result = _TestSubject.Register(RealUsername, CorrectPassword, CancellationToken.None).Result;
        result.Should().BeFalse();

        CallToGetUserByRealUsername.MustHaveHappenedOnceExactly();
    }

    [Test]
    public void RegisterAsync_SuccessfulRegistration_ReturnsTrue()
    {
        var result = _TestSubject.Register(NonExistentUsername, CorrectPassword, CancellationToken.None).Result;
        result.Should().BeTrue();

        CallToGetUserByNonExistentUsername.MustHaveHappenedOnceExactly();
        CallToSaveNewUser.MustHaveHappenedOnceExactly();
    }
    #endregion
    #endregion
}
