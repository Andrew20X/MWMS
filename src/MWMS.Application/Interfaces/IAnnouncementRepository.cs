using System.Collections.Generic;
using System.Threading.Tasks;
using MWMS.Domain.Entities;

namespace MWMS.Application.Interfaces;

public interface IAnnouncementRepository : IGenericRepository<Announcement>
{
    Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync();
}
