using efcore_to_freesql.Entitys;
using FreeSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class CodeFirstExtensions
{

    public static void ConfigEntity(this ICodeFirst codeFirst, IModel efmodel)
    {

        foreach (var type in efmodel.GetEntityTypes())
        {

            codeFirst.ConfigEntity(type.ClrType, a =>
            {

                //表名
                var relationalTableName = type.FindAnnotation("Relational:TableName");
                if (relationalTableName != null)
                    a.Name(relationalTableName.Value?.ToString() ?? type.ClrType.Name);

                foreach (var prop in type.GetProperties())
                {

                    var freeProp = a.Property(prop.Name);

                    //列名
                    var relationalColumnName = prop.FindAnnotation("Relational:ColumnName");
                    if (relationalColumnName != null)
                        freeProp.Name(relationalColumnName.Value?.ToString() ?? prop.Name);

                    //主键
                    freeProp.IsPrimary(prop.IsPrimaryKey());

                    //自增
                    freeProp.IsIdentity(
                        prop.ValueGenerated == ValueGenerated.Never ||
                        prop.ValueGenerated == ValueGenerated.OnAdd ||
                        prop.GetAnnotations().Where(z =>
                            z.Name == "SqlServer:ValueGenerationStrategy" && z.Value.ToString().Contains("IdentityColumn") //sqlserver 自增
                            || z.Value.ToString().Contains("IdentityColumn") //其他数据库实现未经测试
                        ).Any()
                    );

                    //可空
                    freeProp.IsNullable(prop.GetAfterSaveBehavior() != PropertySaveBehavior.Throw);

                    //类型
                    var relationalColumnType = prop.FindAnnotation("Relational:ColumnType");
                    if (relationalColumnType != null)
                    {

                        var dbType = relationalColumnType.ToString();
                        if (!string.IsNullOrEmpty(dbType))
                        {

                            var maxLength = prop.FindAnnotation("MaxLength");
                            if (maxLength != null)
                                dbType += $"({maxLength})";

                            freeProp.DbType(dbType);
                        }
                    }
                }
            });
        }
    }

    public static void EfCoreFluentApiTestGeneric(this ICodeFirst cf)
    {
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

    public static void EfCoreFluentApiTestDynamic(this ICodeFirst cf)
    {
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
}