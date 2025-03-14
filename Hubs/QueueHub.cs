// Hubs/QueueHub.cs
using Microsoft.AspNetCore.SignalR;

namespace VirtualQueueApi.Hubs
{
    public class QueueHub : Hub
    {
        private readonly ILogger<QueueHub> _logger;

        public QueueHub(ILogger<QueueHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            if (exception != null)
            {
                _logger.LogError(exception, "Disconnection error for {ConnectionId}", Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinQueue(string queueId)
        {
            _logger.LogInformation("Client joined Queue: {ConnectionId}", Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, queueId);
        }

        public async Task LeaveQueue(string queueId)
        {
            _logger.LogInformation("Client leaved Queue: {ConnectionId}", Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, queueId);
        }
    }
}
