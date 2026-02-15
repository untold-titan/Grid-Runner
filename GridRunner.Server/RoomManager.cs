using System.Collections.Concurrent;

namespace GridRunner.Server;

public sealed class RoomManager
{
    private sealed class Room
    {
        public string Code { get; init; } = "";
        public string? BoardConnId { get; set; }
        public string? ControllerConnId { get; set; }
    }

    private readonly ConcurrentDictionary<string, Room> _rooms = new();

    public string CreateRoom(string boardConnId)
    {
        while (true)
        {
            var code = GenerateCode();
            var room = new Room { Code = code, BoardConnId = boardConnId };
            if (_rooms.TryAdd(code, room)) return code;
        }
    }

    public (bool ok, string role, string? boardConnId) JoinRoom(string code, string connId)
    {
        code = code.Trim().ToUpperInvariant();
        if (!_rooms.TryGetValue(code, out var room)) return (false, "", null);

        lock (room)
        {
            if (room.BoardConnId == null)
            {
                room.BoardConnId = connId;
                return (true, "Board", room.BoardConnId);
            }

            if (room.ControllerConnId == null && room.BoardConnId != connId)
            {
                room.ControllerConnId = connId;
                return (true, "Controller", room.BoardConnId);
            }

            return (false, "", room.BoardConnId);
        }
    }

    public (string? board, string? controller) GetPeers(string code)
    {
        code = code.Trim().ToUpperInvariant();
        if (!_rooms.TryGetValue(code, out var room)) return (null, null);
        return (room.BoardConnId, room.ControllerConnId);
    }

    public void LeaveAll(string connId)
    {
        foreach (var kvp in _rooms)
        {
            var room = kvp.Value;
            bool removeRoom = false;

            lock (room)
            {
                if (room.BoardConnId == connId) room.BoardConnId = null;
                if (room.ControllerConnId == connId) room.ControllerConnId = null;
                if (room.BoardConnId == null && room.ControllerConnId == null) removeRoom = true;
            }

            if (removeRoom) _rooms.TryRemove(kvp.Key, out _);
        }
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<char> buf = stackalloc char[5];
        var r = Random.Shared;
        for (int i = 0; i < buf.Length; i++) buf[i] = chars[r.Next(chars.Length)];
        return new string(buf);
    }
}