using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace efcore_to_freesql.DBContexts
{

    public class BaseDBContext : DbContext
    {

        public static IFreeSql Fsql { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            Fsql.CodeFirst.ConfigEntity(modelBuilder.Model); //Õ¨≤Ω≈‰÷√
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=|DataDirectory|\document.db;Pooling=true;Max Pool Size=10");
        }
    }
}