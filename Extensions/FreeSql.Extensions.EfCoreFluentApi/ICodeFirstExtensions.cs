using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FreeSql.DataAnnotations;

namespace FreeSql.Extensions.EfCoreFluentApi
{
    public static class ICodeFirstExtensions
    {

        public static ICodeFirst Entity<T>(this ICodeFirst codeFirst, Action<EfCoreTableFluent<T>> modelBuilder)
        {
            codeFirst.ConfigEntity<T>(tf => modelBuilder(new EfCoreTableFluent<T>(tf)));
            return codeFirst;
        }

        static void Test()
        {
            ICodeFirst cf = null;
            cf.Entity<Song>(eb =>
            {
                eb.ToTable("tb_song");
                eb.Ignore(a => a.Field1);
                eb.Property(a => a.Title).HasColumnType("varchar(50)").IsRequired();
                eb.Property(a => a.Url).HasMaxLength(100);

                eb.Property(a => a.RowVersion).IsRowVersion();
                eb.Property(a => a.CreateTime).HasDefaultValueSql("getdate()");

                eb.HasKey(a => a.Id);
                eb.HasIndex(a => a.Title).IsUnique().HasName("idx_xxx11");

                //一对多、多对一
                eb.HasOne(a => a.Type).HasForeignKey(a => a.TypeId).WithMany(a => a.Songs);

                //多对多
                eb.HasMany(a => a.Tags).WithMany(a => a.Songs, typeof(Song_tag));
            });
        }

        public class SongType
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Song> Songs { get; set; }
        }

        public class Song
        {
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
}
