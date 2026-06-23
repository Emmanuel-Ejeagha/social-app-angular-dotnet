using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace api.Services;

public class UserService : IUserService
{
    private readonly IMongoCollection<User> _userCollection;

    public UserService(IOptions<MongoDbSettings> settings)
    {
        ArgumentNullException.ThrowIfNull(settings?.Value);

        var s = settings.Value;
        var client = new MongoClient(s.ConnectionString);
        var db = client.GetDatabase(s.DatabaseName);

        _userCollection = db.GetCollection<User>(s.UserCollection);
    }

    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        await _userCollection.InsertOneAsync(user, cancellationToken: ct);
    }
}
