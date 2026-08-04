using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GeneratorsManagementSystem.Hubs
{
    [Authorize]
    public class GeneratorsHub : Hub
    {
        // المستخدم يطلب الانضمام لمجموعة مولد معين
        public async Task JoinGeneratorGroup(string generatorId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"generator_{generatorId}");
        }

        // مغادرة المجموعة
        public async Task LeaveGeneratorGroup(string generatorId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, $"generator_{generatorId}");
        }

        // الانضمام لمجموعة لوحة التحكم الرئيسية
        public async Task JoinDashboard()
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, "dashboard");
        }
    }
}