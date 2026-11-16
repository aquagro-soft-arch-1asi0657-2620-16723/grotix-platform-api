using GrotixBackend.BuildingBlocks.Auth;
using GrotixBackend.BuildingBlocks.Configuration;

DotEnvBootstrap.LoadFromCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrotixJwt(builder.Configuration);

builder.Services.AddAuthorization();
// CORS solo para los orígenes del frontend configurados (CORS__ALLOWED_ORIGIN en compose).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
builder.Services.AddCors(options =>
    options.AddPolicy("ConfiguredOrigins", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader()));

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("ConfiguredOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/live", () => Results.Ok(new { status = "healthy", service = "gateway" }));
app.MapReverseProxy();

app.Run();
