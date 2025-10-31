using DengueAlert.Api.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DengueAlert.Api.Repository
{
        public interface IDengueRepository
        {
            Task UpsertAsync(DengueAlerta alert); 
            Task<DengueAlerta?> GetByWeekAsync(int ew, int ey, int geocode = 3106200);
            Task<IEnumerable<DengueAlerta>> GetLastNWeeksAsync(int n, int geocode = 3106200);
        }
}
