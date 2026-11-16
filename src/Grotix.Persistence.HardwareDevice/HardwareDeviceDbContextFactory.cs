using GrotixBackend.BuildingBlocks.Configuration;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC;

public sealed class HardwareDeviceDbContextFactory : IDesignTimeDbContextFactory<HardwareDeviceDbContext>
{
    public HardwareDeviceDbContext CreateDbContext(string[] args)
    {
        DotEnvBootstrap.LoadFromCurrentDirectory();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException("Configure the database connection string in environment variables.");

        var optionsBuilder = new DbContextOptionsBuilder<HardwareDeviceDbContext>();
        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 36)),
            mysql =>
            {
                mysql.MigrationsHistoryTable("__EFMigrationsHistory_HardwareDevice");
                mysql.MigrationsAssembly("Grotix.Persistence.HardwareDevice");
            });

        return new HardwareDeviceDbContext(optionsBuilder.Options);
    }
}
