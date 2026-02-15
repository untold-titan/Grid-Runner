using GridRunner.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RoomManager>();
builder.Services.AddSignalR();

builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p =>
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .SetIsOriginAllowed(_ => true)
         .AllowCredentials());
});

var app = builder.Build();

app.UseCors();
app.MapHub<GameHub>("/gamehub");

app.Run();