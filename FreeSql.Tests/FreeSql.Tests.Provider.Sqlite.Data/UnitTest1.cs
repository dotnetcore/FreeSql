using FreeSql.DataAnnotations;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

[Table(Name = "TestTypeInfoT1")]
class TestTypeInfo
{
    [Column(IsIdentity = true)]
    public int Guid { get; set; }
    public int ParentId { get; set; }
    public TestTypeParentInfo Parent { get; set; }
    public string Name { get; set; }
}

[Table(Name = "TestTypeParentInfoT1")]
class TestTypeParentInfo
{
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public string Name { get; set; }

    public int ParentId { get; set; }
    public TestTypeParentInfo Parent { get; set; }
    public ICollection<TestTypeParentInfo> Childs { get; set; }

    public List<TestTypeInfo> Types { get; set; }
}

[Table(Name = "TestInfoT1")]
class TestInfo
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    public int TypeGuid { get; set; }
    public TestTypeInfo Type { get; set; }
    public string Title { get; set; }
    public DateTime CreateTime { get; set; }
}

public partial class Song
{
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public DateTime? Create_time { get; set; }
    public bool? Is_deleted { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }

    public virtual ICollection<Tag> Tags { get; set; }
}
public partial class Song_tag
{
    public int Song_id { get; set; }
    public virtual Song Song { get; set; }

    public int Tag_id { get; set; }
    public virtual Tag Tag { get; set; }
}
public partial class Tag
{
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public int? Parent_id { get; set; }
    public virtual Tag Parent { get; set; }

    public decimal? Ddd { get; set; }
    public string Name { get; set; }

    public virtual ICollection<Song> Songs { get; set; }
    public virtual ICollection<Tag> Tags { get; set; }
}

public class UnitTest1
{
    [Fact]
    public void Test()
    {
        string dataSubDirectory = Path.Combine(AppContext.BaseDirectory);

        if (!Directory.Exists(dataSubDirectory))
            Directory.CreateDirectory(dataSubDirectory);

        AppDomain.CurrentDomain.SetData("DataDirectory", dataSubDirectory);
        using (var connection = new SqliteConnection("Data Source=|DataDirectory|local.db"))
        {
            connection.Open();

        }
    }
}