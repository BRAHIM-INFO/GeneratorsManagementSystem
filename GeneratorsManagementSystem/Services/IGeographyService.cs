using GeneratorsManagementSystem.Models.Geography;

namespace GeneratorsManagementSystem.Services
{
    public interface IGeographyService
    {
        // ═══ Governorates ═══
        Task<List<Governorate>> GetAllGovernoratesAsync();
        Task<Governorate?> GetGovernorateAsync(int id);

        // ═══ Districts (بحسب المحافظة) ═══
        Task<List<District>> GetDistrictsByGovernorateAsync(int governorateId);
        Task<District?> GetDistrictAsync(int id);

        // ═══ Neighborhoods (بحسب القضاء) ═══
        Task<List<Neighborhood>> GetNeighborhoodsByDistrictAsync(int districtId);
        Task<Neighborhood?> GetNeighborhoodAsync(int id);

        // ═══ Alleys (بحسب الحي) ═══
        Task<List<Alley>> GetAlleysByNeighborhoodAsync(int neighborhoodId);
        Task<Alley?> GetAlleyAsync(int id);
    }
}