FreeSql 原本的 FluentApi 方法名与特性名保持一致，所以使用理解成本较低（只需要了解一份）；

这个扩展包目的，为了照顾熟悉 EfCore FluentApi 的开发者，使用习惯 95% 相似；

> dotnet add package FreeSql.Extensions.EfCoreFluentApi

## 以假乱真

```csharp
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
```