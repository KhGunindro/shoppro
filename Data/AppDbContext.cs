using DotNetEnv;
using shoppro.Models;
using System;
using Microsoft.EntityFrameworkCore;

namespace shoppro.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connStr = Environment.GetEnvironmentVariable("SHOPPRO_DB_CONNECTION");
            optionsBuilder.UseNpgsql(connStr);
        }
    }
}
