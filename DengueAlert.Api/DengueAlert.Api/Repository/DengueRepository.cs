using System.Linq;
using System.Threading.Tasks;
using DengueAlert.Api.Data;
using DengueAlert.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DengueAlert.Api.Repository
{
    public class DengueRepository : IDengueRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DengueRepository> _logger;


        public DengueRepository(AppDbContext context, ILogger<DengueRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpsertAsync(DengueAlerta alert)
        {
            var existingById = await _context.DengueAlerts.FindAsync(alert.Id);
            if (existingById != null)
            {
                _context.Entry(existingById).CurrentValues.SetValues(alert);
                existingById.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("Updated by Id: id={id} ey={ey} ew={ew}", alert.Id, alert.EndYear, alert.EndWeek);
            }
            else
            {
                var existing = await _context.DengueAlerts
                    .FirstOrDefaultAsync(x => x.EndWeek == alert.EndWeek && x.EndYear == alert.EndYear && x.GeoCode == alert.GeoCode);

                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(alert);
                    existing.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation("Updated by EW/EY: id={id} ey={ey} ew={ew}", alert.Id, alert.EndYear, alert.EndWeek);
                }
                else
                {
                    await _context.DengueAlerts.AddAsync(alert);
                    _logger.LogInformation("Inserted alert: id={id} ey={ey} ew={ew}", alert.Id, alert.EndYear, alert.EndWeek);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<DengueAlerta?> GetByWeekAsync(int ew, int ey, int geocode = 3106200)
        {
            return await _context.DengueAlerts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EndWeek == ew && x.EndYear == ey && x.GeoCode == geocode);
        }

        public async Task<IEnumerable<DengueAlerta>> GetLastNWeeksAsync(int n, int geocode = 3106200)
        {
            return await _context.DengueAlerts
                .AsNoTracking()
                .Where(x => x.GeoCode == geocode)
                .OrderByDescending(x => x.EndYear).ThenByDescending(x => x.EndWeek)
                .Take(n)
                .ToListAsync();
        }
    }
}
