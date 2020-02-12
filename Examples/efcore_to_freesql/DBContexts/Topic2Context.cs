using efcore_to_freesql.Entitys;
using Microsoft.EntityFrameworkCore;

namespace efcore_to_freesql.DBContexts
{

    public class Topic2Context : BaseDBContext
    {

        public DbSet<Topic2> Topic2s { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Topic2>().ToTable("topic2_sss");
            modelBuilder.Entity<Topic2>().Property(a => a.Id).HasColumnName("topic2_id");

            base.OnModelCreating(modelBuilder);
        }
    }
}