using ifmisIdentity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ifmisIdentity.Data
{
    public class IdentityDbContext : IdentityDbContext<User, Role, int>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OldPassword> oldPasswords { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OldPassword>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.ChangedDate)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(ur => ur.UserId)
                      .IsRequired();

                entity.HasOne(ur => ur.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(ur => ur.RoleId)
                      .IsRequired();
            });

            
                

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.GlobalUserId)
                      .HasDefaultValueSql("gen_random_uuid()") 
                      .IsRequired();

                entity.HasIndex(u => u.GlobalUserId)
                      .IsUnique();
                entity.HasOne(u => u.Organization)
                      .WithMany()
                      .HasForeignKey(u => u.OrganizationId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(o => o.Id); 
                entity.Property(o => o.Name).IsRequired().HasMaxLength(200);
                entity.Property(o => o.DatabaseName).IsRequired().HasMaxLength(100);
                entity.Property(o => o.Description).HasMaxLength(500);
                entity.Property(o=>o.OrgUrl).IsRequired().HasMaxLength(500);
                entity.HasIndex(o => o.DatabaseName).IsUnique();
            });
        }
    }
}
