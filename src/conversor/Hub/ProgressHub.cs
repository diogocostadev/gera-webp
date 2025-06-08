using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GeraWebP.Hub;

public class ProgressHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ILogger<ProgressHub> _logger;

    public ProgressHub(ILogger<ProgressHub> logger)
    {
        _logger = logger;
    }

    public async Task SendProgress(string connectionId, int progress)
    {
        await Clients.Client(connectionId).SendAsync("ReceiveProgress", progress);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Cliente {ConnectionId} entrou no grupo {GroupName}", 
            Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Cliente {ConnectionId} saiu do grupo {GroupName}", 
            Context.ConnectionId, groupName);
    }

    public async Task UpdateProgress(object progressData)
    {
        await Clients.All.SendAsync("ReceiveProgressUpdate", progressData);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Cliente {ConnectionId} conectado", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Cliente {ConnectionId} desconectado", Context.ConnectionId);
        if (exception != null)
        {
            _logger.LogError(exception, "Erro na desconexão do cliente {ConnectionId}", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}