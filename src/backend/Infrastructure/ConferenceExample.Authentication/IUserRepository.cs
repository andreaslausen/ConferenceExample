namespace ConferenceExample.Authentication;

public interface IUserRepository
{
    Task<User?> GetByEmail(string email);
    Task<User?> GetById(UserId id);
    Task Add(User user);
}
