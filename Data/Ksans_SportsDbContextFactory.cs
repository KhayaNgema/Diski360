using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using MyField.Data;

public class Ksans_SportsDbContextFactory : IDesignTimeDbContextFactory<Ksans_SportsDbContext>
{
    public Ksans_SportsDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<Ksans_SportsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new Ksans_SportsDbContext(optionsBuilder.Options);
    }
}
