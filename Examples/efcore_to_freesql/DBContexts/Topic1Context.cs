using efcore_to_freesql.Entitys;
using Microsoft.EntityFrameworkCore;

namespace efcore_to_freesql.DBContexts
{

    public class Topic1Context : BaseDBContext
    {

        public DbSet<Topic1> Topic1s { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Topic1>().ToTable("topic1_sss").HasKey(a => a.Id);
            modelBuilder.Entity<Topic1>().Property(a => a.Id).HasColumnName("topic1_id").ValueGeneratedOnAdd();

            base.OnModelCreating(modelBuilder);
        }
    }
}