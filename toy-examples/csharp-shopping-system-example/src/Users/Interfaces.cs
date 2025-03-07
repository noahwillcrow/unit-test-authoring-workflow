namespace ShoppingSystem.Users;

public class User
{
    public string Username { get; set; }
    public string HashedPassword { get; set; }

    public User(string username, string hashedPassword)
    {
        Username = username;
        HashedPassword = hashedPassword;
    }

    public override bool Equals(object? obj)
    {
        if (!(obj is User other))
        {
            return false;
        }

        return Username == other.Username && HashedPassword == other.HashedPassword;
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken);
    Task SaveUserAsync(User user, CancellationToken cancellationToken);
}

public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string hashedPassword);
}

public interface ITokenService
{
    Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken);
}
