using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace GCTL.Core.ServiceExtensions
{
    public class EmployeeUploadHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        }

        public async Task NotifyProgress(int current, int total, string message)
        {
            await Clients.All.SendAsync("ReceiveProgress", new
            {
                current,
                total,
                percentage = total > 0 ? (int)((current / (double)total) * 100) : 0,
                message
            });
        }
    }
}
