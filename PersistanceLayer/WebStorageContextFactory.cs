using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PersistenceLayer
{
    public class WebStorageContextFactory : IDesignTimeDbContextFactory<WebStorageContext>
    {
        public WebStorageContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WebStorageContext>();

            // Connection string for design-time migrations
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=PPAW;Username=postgres;Password=12345");

            return new WebStorageContext(optionsBuilder.Options);
        }
    }
}
