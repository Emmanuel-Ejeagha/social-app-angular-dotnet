using api.Models;

namespace api.Services;

public interface IUserService
{
    Task CreateAsync(User user, CancellationToken cancellationToken = default);
}
