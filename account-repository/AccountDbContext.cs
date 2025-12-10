using AccountEntities;
using Microsoft.EntityFrameworkCore;

namespace AccountRepository
{
    public class AccountDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Account entity
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.AccountId);

                // Ensure AccountId is treated as a database-generated identity
                entity.Property(e => e.AccountId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.BirthDate)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.City)
                    .HasMaxLength(100);

                entity.Property(e => e.EmailAddress)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.PetCount)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                // Add index on EmailAddress for faster lookups
                entity.HasIndex(e => e.EmailAddress)
                    .IsUnique();
            });
        }
    }
}
