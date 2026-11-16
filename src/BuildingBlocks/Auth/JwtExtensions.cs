using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GrotixBackend.BuildingBlocks.Auth;

public static class JwtExtensions
{
    public static IServiceCollection AddGrotixJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration["TokenSettings:Secret"] ?? configuration["JWT_SECRET"];
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32 || secret.Contains("REPLACE_ME", StringComparison.OrdinalIgnoreCase) ||
            secret.Contains("EstaEsUnaClaveSuperSecreta", StringComparison.OrdinalIgnoreCase) || secret.Any(c => c > 127))
            throw new InvalidOperationException(
                "TokenSettings:Secret debe configurarse con al menos 32 caracteres ASCII seguros.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}
