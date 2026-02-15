using Microsoft.AspNetCore.SignalR;

namespace GridRunner.Server;

public sealed class GameHub : Hub
{
    private readonly RoomManager _rooms;

    public GameHub(RoomManager rooms) => _rooms = rooms;

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _rooms.LeaveAll(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task<string> CreateRoom()
    {
        var code = _rooms.CreateRoom(Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, code);
        return code;
    }

    public async Task<string> JoinRoom(string code)
    {
        var (ok, role, _) = _rooms.JoinRoom(code, Context.ConnectionId);
        if (!ok) return "";

        code = code.Trim().ToUpperInvariant();
        await Groups.AddToGroupAsync(Context.ConnectionId, code);

        await Clients.Group(code).SendAsync("RoomUpdate");
        return role;
    }

    public Task SendSelection(string code, int? selectedVehicleIndex)
    {
        code = code.Trim().ToUpperInvariant();
        return Clients.OthersInGroup(code).SendAsync("SelectionFromBoard", selectedVehicleIndex);
    }

    public Task SendCommand(string code, string command)
    {
        code = code.Trim().ToUpperInvariant();
        return Clients.OthersInGroup(code).SendAsync("CommandFromController", command);
    }

    public Task SendState(string code, string stateJson)
    {
        code = code.Trim().ToUpperInvariant();
        return Clients.OthersInGroup(code).SendAsync("StateFromBoard", stateJson);
    }

    public Task SendBlock(string code, string block)
{
    code = code.Trim().ToUpperInvariant();
    return Clients.OthersInGroup(code).SendAsync("BlockFromController", block);
}

public Task SendStatus(string code, string status)
{
    code = code.Trim().ToUpperInvariant();
    return Clients.OthersInGroup(code).SendAsync("StatusFromBoard", status);
}

public Task SendLevel(string code, string difficulty, string levelLine)
{
    code = code.Trim().ToUpperInvariant();
    return Clients.OthersInGroup(code).SendAsync("LevelFromController", difficulty, levelLine);
}

public Task SendProgram(string code, string programJson)
{
    code = code.Trim().ToUpperInvariant();
    return Clients.OthersInGroup(code).SendAsync("ProgramFromController", programJson);
}
}