using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AuthApplication.DataModels;

namespace AuthApplication.Database
{

    public class ApplicationDbContext:IdentityDbContext<ApplicationUser>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure RefreshToken Entity
            modelBuilder.Entity<RefreshToken>()
                .HasKey(r => r.Id);  

            modelBuilder.Entity<RefreshToken>()
                .Property(r => r.Token)
                .IsRequired()  
                .HasMaxLength(512);  

            modelBuilder.Entity<RefreshToken>()
                .Property(r => r.ExpirationDate)
                .IsRequired(); 

            modelBuilder.Entity<RefreshToken>()
                .Property(r => r.RevokedAt)
                .IsRequired(false);   

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.Token)
                .IsUnique(); 

            modelBuilder.Entity<RefreshToken>()
                .HasOne<ApplicationUser>()
                .WithMany()  
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //seeding Data: 
            
        }
    }

}
