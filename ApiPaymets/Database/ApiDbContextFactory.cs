using Microsoft.EntityFrameworkCore;
using ApiPaymets.Database.CompiledModels;
using Microsoft.EntityFrameworkCore.Design;

namespace ApiPaymets.Database
{
    public class ApiDbContextFactory: IDesignTimeDbContextFactory<ApiDbContext>
    {
        public ApiDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ApiDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.UseModel(ApiDbContextModel.Instance);

            return new ApiDbContext(optionsBuilder.Options);
        }
    }
}
