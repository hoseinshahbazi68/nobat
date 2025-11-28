using System.Linq.Expressions;
using Nobat.Domain.Common;
using Nobat.Domain.Entities.Schedules;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Interfaces;

namespace Nobat.Application.Interfaces;

// Extension interfaces for specific repositories
public interface IShiftRepository : IRepository<Shift>
{
    Task<IQueryable<Shift>> GetQueryableAsync(CancellationToken cancellationToken = default);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetWithRolesAsync(int id, CancellationToken cancellationToken = default);
}

