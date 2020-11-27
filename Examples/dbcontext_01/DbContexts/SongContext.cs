using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;

namespace dbcontext_01
{
    public class SongContext : DbContext
    {
        public DbSet<Song> Songs { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseFreeSql(Startup.Fsql);
            //这里直接指定一个静态的 IFreeSql 对象即可，切勿重新 Build()
        }

        protected override void OnModelCreating(ICodeFirst codefirst)
        {
            codefirst.Entity<Song>(eb =>
            {
                eb.ToTable("tb_song");
                eb.Ignore(a => a.Field1);
                eb.Property(a => a.Title).HasColumnType("varchar(50)").IsRequired();
                eb.Property(a => a.Url).HasMaxLength(100);

                eb.Property(a => a.RowVersion).IsRowVersion();
                eb.Property(a => a.CreateTime).HasDefaultValueSql("current_timestamp");

                eb.HasKey(a => a.Id);
                eb.HasIndex(a => new { a.Id, a.Title }).IsUnique().HasName("idx_xxx11");

                //一对多、多对一
                eb.HasOne(a => a.Type).HasForeignKey(a => a.TypeId).WithMany(a => a.Songs);

                //多对多
                eb.HasMany(a => a.Tags).WithMany(a => a.Songs, typeof(Song_tag));
            });

            codefirst.Entity<SongType>(eb =>
            {
                eb.HasMany(a => a.Songs).WithOne(a => a.Type).HasForeignKey(a => a.TypeId);

                eb.HasData(new[]
                {
                    new SongType
                    {
                        Id = 1,
                        Name = "流行",
                        Songs = new List<Song>(new[]
                        {
                            new Song{ Title = "真的爱你" },
                            new Song{ Title = "爱你一万年" },
                        })
                    },
                    new SongType
                    {
                        Id = 2,
                        Name = "乡村",
                        Songs = new List<Song>(new[]
                        {
                            new Song{ Title = "乡里乡亲" },
                        })
                    },
                });
            });

            codefirst.SyncStructure<SongType>();
            codefirst.SyncStructure<Song>();
        }
    }

    public class SongType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Song> Songs { get; set; }
    }
    public class Song
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime CreateTime { get; set; }

        public int TypeId { get; set; }
        public SongType Type { get; set; }
        public List<Tag> Tags { get; set; }

        public int Field1 { get; set; }
        public long RowVersion { get; set; }
    }
    public class Song_tag
    {
        public int Song_id { get; set; }
        public Song Song { get; set; }

        public int Tag_id { get; set; }
        public Tag Tag { get; set; }
    }
    public class Tag
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Song> Songs { get; set; }
    }
}
