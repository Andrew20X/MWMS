using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Persistence.Context;

namespace MWMS.Persistence.Repositories;

public class AnnouncementRepository : GenericRepository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync()
    {
        return await _dbSet
            .Include(a => a.TargetEmployee)
            .Where(a => a.IsActive && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
