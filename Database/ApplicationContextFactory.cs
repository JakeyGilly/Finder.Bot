using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace Finder.Bot.Database {
    public class BloggingContextFactory : IDesignTimeDbContextFactory<ApplicationContext> {
        public ApplicationContext CreateDbContext(string[] args) {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            IConfiguration configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", false, true).Build();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("Default")!, builder => builder.MigrationsAssembly("Finder.Database"));
            return new ApplicationContext(optionsBuilder.Options);
        }
    }
}