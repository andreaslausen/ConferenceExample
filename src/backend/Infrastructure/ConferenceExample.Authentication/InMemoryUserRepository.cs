namespace ConferenceExample.Authentication;

public class InMemoryUserRepository : IUserRepository
{
    private readonly Lock _lock = new();
    private readonly List<User> _users = [];

    public Task<User?> GetByEmail(string email)
    {
        lock (_lock)
        {
            var user = _users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)
            );
            return Task.FromResult(user);
        }
    }

    public Task<User?> GetById(UserId id)
    {
        lock (_lock)
        {
            var user = _users.FirstOrDefault(u => u.Id.Value == id.Value);
            return Task.FromResult(user);
        }
    }

    public Task Add(User user)
    {
        lock (_lock)
        {
            _users.Add(user);
        }

        return Task.CompletedTask;
    }
}
