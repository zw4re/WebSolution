using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities.DbModels;

namespace DatabaseService.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<TcmbExchangeRate> TcmbExchangeRates { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Company tablosu için yapılandırma
            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("companies");

                // Primary key tanımı
                entity.HasKey(c => c.StockCode);

                // Uyumlu kolon ayarları
                entity.Property(c => c.StockCode)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(c => c.MkkMemberOid).IsRequired();
                entity.Property(c => c.KapMemberTitle).IsRequired();
                entity.Property(c => c.RelatedMemberTitle).IsRequired();
                entity.Property(c => c.CityName).IsRequired();
                entity.Property(c => c.RelatedMemberOid).IsRequired();
                entity.Property(c => c.KapMemberType).IsRequired();
            });

            // Birleşik key tanımı
            modelBuilder.Entity<TcmbExchangeRate>()
                .HasKey(e => new { e.Date, e.CurrencyCode, e.Type });

            base.OnModelCreating(modelBuilder);
        }
    }
}
