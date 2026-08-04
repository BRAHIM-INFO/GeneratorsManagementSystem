using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models.Geography;
using Microsoft.EntityFrameworkCore;

namespace GeneratorsManagementSystem.Services
{
    public class GeographyService : IGeographyService
    {
        private readonly ApplicationDbContext _db;

        public GeographyService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Governorate>> GetAllGovernoratesAsync()
        {
            return await _db.Governorates
                .Where(g => g.IsActive)
                .OrderBy(g => g.DisplayOrder)
                .ThenBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<Governorate?> GetGovernorateAsync(int id)
        {
            return await _db.Governorates.FindAsync(id);
        }

        public async Task<List<District>> GetDistrictsByGovernorateAsync(int governorateId)
        {
            return await _db.Districts
                .Where(d => d.GovernorateId == governorateId && d.IsActive)
                .OrderBy(d => d.DisplayOrder)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<District?> GetDistrictAsync(int id)
        {
            return await _db.Districts.FindAsync(id);
        }

        public async Task<List<Neighborhood>> GetNeighborhoodsByDistrictAsync(int districtId)
        {
            return await _db.Neighborhoods
                .Where(n => n.DistrictId == districtId && n.IsActive)
                .OrderBy(n => n.DisplayOrder)
                .ThenBy(n => n.Name)
                .ToListAsync();
        }

        public async Task<Neighborhood?> GetNeighborhoodAsync(int id)
        {
            return await _db.Neighborhoods.FindAsync(id);
        }

        public async Task<List<Alley>> GetAlleysByNeighborhoodAsync(int neighborhoodId)
        {
            return await _db.Alleys
                .Where(a => a.NeighborhoodId == neighborhoodId && a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Alley?> GetAlleyAsync(int id)
        {
            return await _db.Alleys.FindAsync(id);
        }
    }
}