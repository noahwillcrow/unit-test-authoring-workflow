namespace ShoppingSystem.Users;

public class UserAuthenticator
{
    private readonly IPasswordHasher _PasswordHasher;
    private readonly ITokenService _TokenService;
    private readonly IUserRepository _UserRepository;

    public UserAuthenticator(IPasswordHasher passwordHasher, ITokenService tokenService, IUserRepository userRepository)
    {
        _PasswordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _TokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<string?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null; // Invalid input
        }

        var user = await _UserRepository.GetUserByUsernameAsync(username, cancellationToken);
        if (user == null || !_PasswordHasher.VerifyPassword(password, user.HashedPassword))
        {
            return null; // Authentication failed
        }

        return await _TokenService.GenerateTokenAsync(user, cancellationToken);
    }

    public async Task<bool> Register(string username, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false; // Invalid input
        }

        var existingUserForUsername = await _UserRepository.GetUserByUsernameAsync(username, cancellationToken);
        if (existingUserForUsername != null)
        {
            return false; // User already exists
        }

        string hashedPassword = _PasswordHasher.HashPassword(password);
        await _UserRepository.SaveUserAsync(new User(username, hashedPassword), cancellationToken);

        return true; // Registration successful
    }
}
