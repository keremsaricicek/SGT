using Sgt.BuildingBlocks;

// Tek host: modülleri kaydeder, kendi iş kodu yoktur (docs/kod-kurallari.md 3.2).
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClock, SystemClock>();

var app = builder.Build();

app.MapGet("/health", (IClock clock) => Results.Ok(new { Status = "ok", ServerTimeUtc = clock.UtcNow }));

app.Run();
