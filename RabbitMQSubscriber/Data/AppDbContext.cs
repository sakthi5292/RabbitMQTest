namespace RabbitMQSubscriber.Data
{
    using Microsoft.EntityFrameworkCore;
    using RabbitMQSubscriber.Services;
    using System.Collections.Generic;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<DataRecord> Records => Set<DataRecord>();

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //=> optionsBuilder.UseNpgsql(@"Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=RabbitMQDemo;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DataRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).HasMaxLength(5000);
            });
        }
    }
}
