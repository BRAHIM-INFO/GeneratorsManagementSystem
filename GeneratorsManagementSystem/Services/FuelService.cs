using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Models.Fuel;
using Microsoft.EntityFrameworkCore;

namespace GeneratorsManagementSystem.Services
{
    public class FuelService : IFuelService
    {
        private readonly ApplicationDbContext _db;

        public FuelService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ═══ توليد رقم الحصة: ALC-26-00001 ═══
        public async Task<string> GenerateAllocationNumberAsync()
        {
            var year = DateTime.Now.ToString("yy");
            var prefix = $"ALC-{year}-";

            var lastNumber = await _db.FuelAllocations
                .Where(a => a.AllocationNumber.StartsWith(prefix))
                .OrderByDescending(a => a.AllocationNumber)
                .Select(a => a.AllocationNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastSeq = lastNumber.Replace(prefix, "");
                if (int.TryParse(lastSeq, out int parsed))
                    nextSeq = parsed + 1;
            }

            return $"{prefix}{nextSeq:D5}";
        }

        // ═══ توليد رقم الاستهلاك: FUEL-26-00001 ═══
        public async Task<string> GenerateConsumptionNumberAsync()
        {
            var year = DateTime.Now.ToString("yy");
            var prefix = $"FUEL-{year}-";

            var lastNumber = await _db.FuelConsumptions
                .Where(c => c.ConsumptionNumber.StartsWith(prefix))
                .OrderByDescending(c => c.ConsumptionNumber)
                .Select(c => c.ConsumptionNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastSeq = lastNumber.Replace(prefix, "");
                if (int.TryParse(lastSeq, out int parsed))
                    nextSeq = parsed + 1;
            }

            return $"{prefix}{nextSeq:D5}";
        }

        // ═══ كل الحصص ═══
        public async Task<List<FuelAllocation>> GetAllAllocationsAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _db.FuelAllocations
                .Include(a => a.Consumptions)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.AllocationDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.AllocationDate <= endDate.Value);

            return await query
                .OrderByDescending(a => a.AllocationDate)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
        }

        public async Task<FuelAllocation?> GetAllocationByIdAsync(int id)
        {
            return await _db.FuelAllocations
                .Include(a => a.Consumptions)
                    .ThenInclude(c => c.Generator)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<FuelAllocation> CreateAllocationAsync(FuelAllocation allocation, string createdBy)
        {
            allocation.AllocationNumber = await GenerateAllocationNumberAsync();
            allocation.TotalCost = allocation.Quantity * allocation.PricePerLiter;
            allocation.CreatedAt = DateTime.Now;
            allocation.CreatedBy = createdBy;

            _db.FuelAllocations.Add(allocation);
            await _db.SaveChangesAsync();
            return allocation;
        }

        public async Task<FuelAllocation> UpdateAllocationAsync(FuelAllocation allocation, string updatedBy)
        {
            var existing = await _db.FuelAllocations.FindAsync(allocation.Id)
                ?? throw new Exception("الحصة غير موجودة");

            existing.FuelKind = allocation.FuelKind;
            existing.Source = allocation.Source;
            existing.Quantity = allocation.Quantity;
            existing.PricePerLiter = allocation.PricePerLiter;
            existing.TotalCost = allocation.Quantity * allocation.PricePerLiter;
            existing.AllocationDate = allocation.AllocationDate;
            existing.AllocationMonth = allocation.AllocationMonth;
            existing.AllocationYear = allocation.AllocationYear;
            existing.Supplier = allocation.Supplier;
            existing.ReferenceNumber = allocation.ReferenceNumber;
            existing.ReceivedBy = allocation.ReceivedBy;
            existing.Notes = allocation.Notes;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAllocationAsync(int id)
        {
            var allocation = await _db.FuelAllocations
                .Include(a => a.Consumptions)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (allocation == null) return false;

            if (allocation.Consumptions.Any())
                throw new Exception(
                    "لا يمكن حذف الحصة لوجود سجلات استهلاك مرتبطة بها. " +
                    "احذف سجلات الاستهلاك أولاً.");

            _db.FuelAllocations.Remove(allocation);
            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ الحصص المتاحة (بها كمية متبقية) ═══
        public async Task<List<FuelAllocation>> GetAvailableAllocationsAsync(FuelKind fuelKind)
        {
            var allocations = await _db.FuelAllocations
                .Include(a => a.Consumptions)
                .Where(a => a.FuelKind == fuelKind)
                .OrderBy(a => a.AllocationDate)
                .ToListAsync();

            return allocations.Where(a => a.RemainingQuantity > 0).ToList();
        }

        // ═══ كل سجلات الاستهلاك ═══
        public async Task<List<FuelConsumption>> GetAllConsumptionsAsync(
            DateTime? startDate = null, DateTime? endDate = null, int? generatorId = null)
        {
            var query = _db.FuelConsumptions
                .Include(c => c.Generator)
                .Include(c => c.FuelAllocation)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(c => c.ConsumptionDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(c => c.ConsumptionDate <= endDate.Value);
            if (generatorId.HasValue)
                query = query.Where(c => c.GeneratorId == generatorId.Value);

            return await query
                .OrderByDescending(c => c.ConsumptionDate)
                .ToListAsync();
        }

        public async Task<FuelConsumption?> GetConsumptionByIdAsync(int id)
        {
            return await _db.FuelConsumptions
                .Include(c => c.Generator)
                .Include(c => c.FuelAllocation)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<FuelConsumption> CreateConsumptionAsync(FuelConsumption consumption, string createdBy)
        {
            // التحقق من الحصة (إن وُجدت)
            if (consumption.FuelAllocationId.HasValue)
            {
                var allocation = await _db.FuelAllocations
                    .Include(a => a.Consumptions)
                    .FirstOrDefaultAsync(a => a.Id == consumption.FuelAllocationId.Value);

                if (allocation == null)
                    throw new Exception("الحصة غير موجودة");

                var remaining = allocation.Quantity - allocation.Consumptions.Sum(c => c.Quantity);
                if (consumption.Quantity > remaining)
                    throw new Exception(
                        $"الكمية المطلوبة ({consumption.Quantity:N2} لتر) " +
                        $"أكبر من المتبقي في الحصة ({remaining:N2} لتر)");

                // حساب التكلفة تلقائياً
                if (consumption.Cost == 0)
                    consumption.Cost = consumption.Quantity * allocation.PricePerLiter;
            }

            consumption.ConsumptionNumber = await GenerateConsumptionNumberAsync();
            consumption.CreatedAt = DateTime.Now;
            consumption.CreatedBy = createdBy;

            _db.FuelConsumptions.Add(consumption);

            // تحديث مستوى الوقود في المولد
            var generator = await _db.Generators.FindAsync(consumption.GeneratorId);
            if (generator != null && consumption.LevelAfter.HasValue)
            {
                generator.CurrentFuelLevel = consumption.LevelAfter.Value;
                generator.LastDataUpdate = DateTime.Now;
            }

            await _db.SaveChangesAsync();
            return consumption;
        }

        public async Task<bool> DeleteConsumptionAsync(int id)
        {
            var consumption = await _db.FuelConsumptions.FindAsync(id);
            if (consumption == null) return false;

            _db.FuelConsumptions.Remove(consumption);
            await _db.SaveChangesAsync();
            return true;
        }

        // ═══ لوحة تحكم الوقود ═══
        public async Task<FuelDashboard> GetDashboardAsync()
        {
            var allocations = await _db.FuelAllocations
                .Include(a => a.Consumptions)
                .ToListAsync();

            var consumptions = await _db.FuelConsumptions.ToListAsync();

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var totalAllocated = allocations.Sum(a => a.Quantity);
            var totalConsumed = consumptions.Sum(c => c.Quantity);

            return new FuelDashboard
            {
                TotalAllocated = totalAllocated,
                TotalConsumed = totalConsumed,
                TotalRemaining = totalAllocated - totalConsumed,
                ConsumptionPercentage = totalAllocated > 0 ? (totalConsumed / totalAllocated * 100) : 0,

                GovernmentQuota = allocations.Where(a => a.Source == FuelSource.Government).Sum(a => a.Quantity),
                MarketPurchase = allocations.Where(a => a.Source == FuelSource.MarketPurchase).Sum(a => a.Quantity),
                Donations = allocations.Where(a => a.Source == FuelSource.Donation).Sum(a => a.Quantity),

                TotalCost = allocations.Sum(a => a.TotalCost),
                TodayConsumption = consumptions.Where(c => c.ConsumptionDate.Date == today).Sum(c => c.Quantity),
                MonthConsumption = consumptions.Where(c => c.ConsumptionDate.Date >= monthStart).Sum(c => c.Quantity),

                ActiveAllocations = allocations.Count(a => a.RemainingQuantity > 0),
                TotalConsumptionRecords = consumptions.Count,

                ConsumptionByFuelKind = consumptions
                    .GroupBy(c => c.FuelKindText)
                    .ToDictionary(g => g.Key, g => g.Sum(c => c.Quantity)),

                AllocationBySource = allocations
                    .GroupBy(a => a.SourceText)
                    .ToDictionary(g => g.Key, g => g.Sum(a => a.Quantity))
            };
        }

        // ═══ إحصائيات المولدات ═══
        public async Task<List<GeneratorFuelStats>> GetGeneratorFuelStatsAsync()
        {
            var generators = await _db.Generators
                .Include(g => g.FuelConsumptions)
                .ToListAsync();

            return generators.Select(g => new GeneratorFuelStats
            {
                GeneratorId = g.Id,
                GeneratorNumber = g.GeneratorNumber,
                GeneratorName = g.Name,
                TotalConsumed = g.FuelConsumptions?.Sum(c => c.Quantity) ?? 0,
                TotalCost = g.FuelConsumptions?.Sum(c => c.Cost) ?? 0,
                ConsumptionCount = g.FuelConsumptions?.Count ?? 0,
                LastConsumption = g.FuelConsumptions?.OrderByDescending(c => c.ConsumptionDate)
                    .FirstOrDefault()?.ConsumptionDate
            })
            .OrderByDescending(s => s.TotalConsumed)
            .ToList();
        }
    }
}