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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .ToTable("companies");

            // Birleşik key tanımı
            modelBuilder.Entity<TcmbExchangeRate>()
                .HasKey(e => new { e.Date, e.CurrencyCode, e.Type });

            base.OnModelCreating(modelBuilder);
        }
    }
}
