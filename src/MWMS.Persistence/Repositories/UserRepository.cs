using Microsoft.EntityFrameworkCore;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;

namespace MWMS.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Username == username);
    }
}
