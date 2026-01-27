using ifmisIdentity.Models;
using Microsoft.EntityFrameworkCore;

namespace ifmisIdentity.Data
{
    public class OpenIddictDbContext : DbContext
    {
        public OpenIddictDbContext(DbContextOptions<OpenIddictDbContext> options)
            : base(options)
        {
        }
       
        public DbSet<MyApplication> Applications { get; set; }
        public DbSet<MyAuthorization> Authorizations { get; set; }
        public DbSet<MyScope> Scopes { get; set; }
        public DbSet<MyToken> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
        }
    }
}
