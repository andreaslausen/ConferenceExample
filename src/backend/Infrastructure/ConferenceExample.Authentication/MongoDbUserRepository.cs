using MongoDB.Driver;

namespace ConferenceExample.Authentication;

public class MongoDbUserRepository : IUserRepository
{
    private readonly IMongoCollection<StoredUser> _usersCollection;

    public MongoDbUserRepository(IMongoDatabase mongoDatabase)
    {
        _usersCollection = mongoDatabase.GetCollection<StoredUser>("users");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Unique index on email for fast lookups and to prevent duplicate emails
        var emailIndexKeys = Builders<StoredUser>.IndexKeys.Ascending(u => u.Email);
        var emailIndexModel = new CreateIndexModel<StoredUser>(
            emailIndexKeys,
            new CreateIndexOptions { Name = "idx_email", Unique = true }
        );

        _usersCollection.Indexes.CreateOne(emailIndexModel);
    }

    public async Task<User?> GetByEmail(string email)
    {
        var filter = Builders<StoredUser>.Filter.Eq(u => u.Email, email);
        var storedUser = await _usersCollection.Find(filter).FirstOrDefaultAsync();
        return storedUser?.ToUser();
    }

    public async Task<User?> GetById(UserId id)
    {
        var filter = Builders<StoredUser>.Filter.Eq(u => u.Id, id.Value.ToString());
        var storedUser = await _usersCollection.Find(filter).FirstOrDefaultAsync();
        return storedUser?.ToUser();
    }

    public async Task Add(User user)
    {
        var storedUser = StoredUser.FromUser(user);
        await _usersCollection.InsertOneAsync(storedUser);
    }
}
