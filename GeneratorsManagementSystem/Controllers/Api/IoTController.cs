using GeneratorsManagementSystem.Hubs;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GeneratorsManagementSystem.Controllers.Api
{
    [Route("api/iot")]
    [ApiController]
    public class IoTController : ControllerBase
    {
        private readonly IGeneratorService _service;
        private readonly IHubContext<GeneratorsHub> _hub;
        private readonly IConfiguration _config;

        public IoTController(
            IGeneratorService service,
            IHubContext<GeneratorsHub> hub,
            IConfiguration config)
        {
            _service = service;
            _hub = hub;
            _config = config;
        }

        // ══════════════════════════════════════
        // استقبال بيانات من الحساسات
        // POST /api/iot/data
        // ══════════════════════════════════════
        [HttpPost("data")]
        public async Task<IActionResult> ReceiveData([FromBody] IoTDataDto dto)
        {
            // التحقق من API Key
            var apiKey = Request.Headers["X-API-Key"].ToString();
            var validKey = _config["IoT:ApiKey"] ?? "GMS-IOT-2025";

            if (apiKey != validKey)
                return Unauthorized(new { message = "مفتاح API غير صحيح" });

            if (dto == null || dto.GeneratorId <= 0)
                return BadRequest(new { message = "بيانات غير صحيحة" });

            // تحديث بيانات المولد
            var realtimeData = new RealtimeData
            {
                CurrentLoad = dto.CurrentLoad,
                Temperature = dto.Temperature,
                OilPressure = dto.OilPressure,
                FuelLevel = dto.FuelLevel,
                Voltage = dto.Voltage,
                RunningMinutes = dto.RunningMinutes
            };

            await _service.UpdateRealtimeDataAsync(dto.GeneratorId, realtimeData);

            // إرسال عبر SignalR لجميع المتصلين
            await _hub.Clients.Group($"generator_{dto.GeneratorId}")
                .SendAsync("RealtimeUpdate", new
                {
                    GeneratorId = dto.GeneratorId,
                    dto.CurrentLoad,
                    dto.Temperature,
                    dto.OilPressure,
                    dto.FuelLevel,
                    dto.Voltage,
                    Timestamp = DateTime.Now.ToString("HH:mm:ss")
                });

            // إرسال للوحة التحكم
            await _hub.Clients.Group("dashboard")
                .SendAsync("GeneratorDataUpdate", new
                {
                    GeneratorId = dto.GeneratorId,
                    CurrentLoad = dto.CurrentLoad,
                    FuelLevel = dto.FuelLevel,
                    Temperature = dto.Temperature,
                    Timestamp = DateTime.Now.ToString("HH:mm:ss")
                });

            // تحذيرات تلقائية
            await CheckAndSendAlerts(dto);

            return Ok(new
            {
                success = true,
                message = "تم استقبال البيانات بنجاح",
                timestamp = DateTime.Now
            });
        }

        // ══════════════════════════════════════
        // تحذيرات تلقائية
        // ══════════════════════════════════════
        private async Task CheckAndSendAlerts(IoTDataDto dto)
        {
            var alerts = new List<object>();

            if (dto.FuelLevel.HasValue && dto.FuelLevel < 20)
            {
                alerts.Add(new
                {
                    Type = "fuel",
                    Level = "warning",
                    Message = $"⚠️ مستوى الوقود منخفض: {dto.FuelLevel}%",
                    GeneratorId = dto.GeneratorId
                });
            }

            if (dto.Temperature.HasValue && dto.Temperature > 90)
            {
                alerts.Add(new
                {
                    Type = "temperature",
                    Level = "danger",
                    Message = $"🔴 درجة حرارة مرتفعة: {dto.Temperature}°C",
                    GeneratorId = dto.GeneratorId
                });
            }

            if (dto.OilPressure.HasValue && dto.OilPressure < 2)
            {
                alerts.Add(new
                {
                    Type = "oil",
                    Level = "danger",
                    Message = $"🔴 ضغط زيت منخفض: {dto.OilPressure} bar",
                    GeneratorId = dto.GeneratorId
                });
            }

            foreach (var alert in alerts)
            {
                await _hub.Clients.Group("dashboard")
                    .SendAsync("GeneratorAlert", alert);
            }
        }

        // ══════════════════════════════════════
        // اختبار الاتصال
        // GET /api/iot/ping
        // ══════════════════════════════════════
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                status = "online",
                message = "GMS IoT API يعمل",
                timestamp = DateTime.Now
            });
        }
    }

    public class IoTDataDto
    {
        public int GeneratorId { get; set; }
        public decimal? CurrentLoad { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? OilPressure { get; set; }
        public decimal? FuelLevel { get; set; }
        public decimal? Voltage { get; set; }
        public decimal RunningMinutes { get; set; } = 0;
    }
}