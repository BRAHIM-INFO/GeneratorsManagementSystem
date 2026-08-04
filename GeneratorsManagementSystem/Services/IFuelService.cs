using GeneratorsManagementSystem.Models.Fuel;

namespace GeneratorsManagementSystem.Services
{
    public interface IFuelService
    {
        // ═══ الحصص ═══
        Task<string> GenerateAllocationNumberAsync();
        Task<List<FuelAllocation>> GetAllAllocationsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<FuelAllocation?> GetAllocationByIdAsync(int id);
        Task<FuelAllocation> CreateAllocationAsync(FuelAllocation allocation, string createdBy);
        Task<FuelAllocation> UpdateAllocationAsync(FuelAllocation allocation, string updatedBy);
        Task<bool> DeleteAllocationAsync(int id);
        Task<List<FuelAllocation>> GetAvailableAllocationsAsync(FuelKind fuelKind);

        // ═══ الاستهلاك ═══
        Task<string> GenerateConsumptionNumberAsync();
        Task<List<FuelConsumption>> GetAllConsumptionsAsync(DateTime? startDate = null, DateTime? endDate = null, int? generatorId = null);
        Task<FuelConsumption?> GetConsumptionByIdAsync(int id);
        Task<FuelConsumption> CreateConsumptionAsync(FuelConsumption consumption, string createdBy);
        Task<bool> DeleteConsumptionAsync(int id);

        // ═══ الإحصائيات ═══
        Task<FuelDashboard> GetDashboardAsync();
        Task<List<GeneratorFuelStats>> GetGeneratorFuelStatsAsync();
    }

    public class FuelDashboard
    {
        public decimal TotalAllocated { get; set; }
        public decimal TotalConsumed { get; set; }
        public decimal TotalRemaining { get; set; }
        public decimal ConsumptionPercentage { get; set; }

        public decimal GovernmentQuota { get; set; }
        public decimal MarketPurchase { get; set; }
        public decimal Donations { get; set; }

        public decimal TotalCost { get; set; }
        public decimal TodayConsumption { get; set; }
        public decimal MonthConsumption { get; set; }

        public int ActiveAllocations { get; set; }
        public int TotalConsumptionRecords { get; set; }

        public Dictionary<string, decimal> ConsumptionByFuelKind { get; set; } = new();
        public Dictionary<string, decimal> AllocationBySource { get; set; } = new();
    }

    public class GeneratorFuelStats
    {
        public int GeneratorId { get; set; }
        public string GeneratorNumber { get; set; } = string.Empty;
        public string GeneratorName { get; set; } = string.Empty;
        public decimal TotalConsumed { get; set; }
        public decimal TotalCost { get; set; }
        public int ConsumptionCount { get; set; }
        public DateTime? LastConsumption { get; set; }
    }
}