using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Extensions.EfCoreFluentApi;
using FreeSql.Internal.CommonProvider;

partial class FreeSqlDbContextExtensions
{
    /// <summary>
    /// EFCore 95% 相似的 FluentApi 扩展方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="codeFirst"></param>
    /// <param name="modelBuilder"></param>
    /// <returns></returns>
    public static ICodeFirst Entity<T>(this ICodeFirst codeFirst, Action<EfCoreTableFluent<T>> modelBuilder)
    {
        var cf = codeFirst as CodeFirstProvider;
        codeFirst.ConfigEntity<T>(tf => modelBuilder(new EfCoreTableFluent<T>(cf._orm, tf)));
        return codeFirst;
    }
    /// <summary>
    /// EFCore 95% 相似的 FluentApi 扩展方法
    /// </summary>
    /// <param name="codeFirst"></param>
    /// <param name="entityType">实体类型</param>
    /// <param name="modelBuilder"></param>
    /// <returns></returns>
    public static ICodeFirst Entity(this ICodeFirst codeFirst, Type entityType, Action<EfCoreTableFluent> modelBuilder)
    {
        var cf = codeFirst as CodeFirstProvider;
        codeFirst.ConfigEntity(entityType, tf => modelBuilder(new EfCoreTableFluent(cf._orm, tf, entityType)));
        return codeFirst;
    }

    public static void EfCoreFluentApiTestGeneric(IFreeSql fsql)
    {
        var cf = fsql.CodeFirst;
        cf.Entity<Song>(eb =>
        {
            eb.ToTable("tb_song1");
            eb.Ignore(a => a.Field1);
            eb.Property(a => a.Title).HasColumnType("varchar(50)").IsRequired();
            eb.Property(a => a.Url).HasMaxLength(100);

            eb.Property(a => a.RowVersion).IsRowVersion();
            eb.Property(a => a.CreateTime).HasDefaultValueSql("current_timestamp");

            eb.HasKey(a => a.Id);
            eb.HasIndex(a => a.Title).IsUnique().HasName("idx_tb_song1111");

            //一对多、多对一
            eb.HasOne(a => a.Type).HasForeignKey(a => a.TypeId).WithMany(a => a.Songs);

            //多对多
            eb.HasMany(a => a.Tags).WithMany(a => a.Songs, typeof(Song_tag));
        });
        cf.Entity<SongType>(eb =>
        {
            eb.ToTable("tb_songtype1");
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

        cf.SyncStructure<SongType>();
        cf.SyncStructure<Song>();
    }

    public static void EfCoreFluentApiTestDynamic(IFreeSql fsql)
    {
        var cf = fsql.CodeFirst;
        cf.Entity(typeof(Song), eb =>
        {
            eb.ToTable("tb_song2");
            eb.Ignore("Field1");
            eb.Property("Title").HasColumnType("varchar(50)").IsRequired();
            eb.Property("Url").HasMaxLength(100);

            eb.Property("RowVersion").IsRowVersion();
            eb.Property("CreateTime").HasDefaultValueSql("current_timestamp");

            eb.HasKey("Id");
            eb.HasIndex("Title").IsUnique().HasName("idx_tb_song2222");

            //一对多、多对一
            eb.HasOne("Type").HasForeignKey("TypeId").WithMany("Songs");

            //多对多
            eb.HasMany("Tags").WithMany("Songs", typeof(Song_tag));
        });
        cf.Entity(typeof(SongType), eb =>
        {
            eb.ToTable("tb_songtype2");
            eb.HasMany("Songs").WithOne("Type").HasForeignKey("TypeId");

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

        cf.SyncStructure<SongType>();
        cf.SyncStructure<Song>();
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
