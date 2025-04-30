using Models.Entities;
using Storage;

namespace Services;

public interface IUserService
{
    Task<User> AuthenticationAsync(int id, string firstName, string lastName, string username, string hash, long authDate);
    Task<User> GetUserById(int id);
}