using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PodcastApp.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Load configuration manually (for migrations)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Use RDS connection string explicitly
            var connStr = configuration.GetConnectionString("RDSConnection")
                ?? "Server=mssqlserver.c2fe4wqk2kwcg.us-east-1.rds.amazonaws.com,1433;Database=PodcastDb;User ID=stella;Password=stellakwate;Encrypt=True;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connStr);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
