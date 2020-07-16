using FreeSql.Tests.DataContext.SqlServer;
using System;
using System.Data.SqlClient;
using Xunit;

namespace FreeSql.Tests.DataAnnotations
{
    public class EFCoreAttributeTest
    {
        IFreeSql fsql => g.sqlserver;

        [Fact]
        public void TableAttribute()
        {
            fsql.CodeFirst.SyncStructure<eftesttb01>();
            fsql.CodeFirst.SyncStructure<eftesttb02>();
            Assert.Equal("eftesttb_01", fsql.CodeFirst.GetTableByEntity(typeof(eftesttb01)).DbName);
            Assert.Equal("dbo.eftesttb_02", fsql.CodeFirst.GetTableByEntity(typeof(eftesttb02)).DbName);
        }
        [System.ComponentModel.DataAnnotations.Schema.Table("eftesttb_01")]
        class eftesttb01
        {
            public Guid id { get; set; }
        }
        [System.ComponentModel.DataAnnotations.Schema.Table("eftesttb_02", Schema = "dbo")]
        class eftesttb02
        {
            public Guid id { get; set; }
        }

        [Fact]
        public void MaxLengthAttribute()
        {
            fsql.CodeFirst.SyncStructure<eftesttb03>();
            Assert.Equal("NVARCHAR(100)", fsql.CodeFirst.GetTableByEntity(typeof(eftesttb03)).ColumnsByCs["title"].Attribute.DbType);
        }
        class eftesttb03
        {
            public Guid id { get; set; }
            [System.ComponentModel.DataAnnotations.MaxLength(100)]
            public string title { get; set; }
        }

        [Fact]
        public void StringLengthAttribute()
        {
            fsql.CodeFirst.SyncStructure<eftesttb033>();
            Assert.Equal("NVARCHAR(101)", fsql.CodeFirst.GetTableByEntity(typeof(eftesttb033)).ColumnsByCs["title2"].Attribute.DbType);
        }
        class eftesttb033
        {
            public Guid id { get; set; }
            [System.ComponentModel.DataAnnotations.StringLength(101)]
            public string title2 { get; set; }
        }

        [Fact]
        public void RequiredAttribute()
        {
            fsql.CodeFirst.SyncStructure<eftesttb04>();
            Assert.False(fsql.CodeFirst.GetTableByEntity(typeof(eftesttb04)).ColumnsByCs["title"].Attribute.IsNullable);
        }
        class eftesttb04
        {
            public Guid id { get; set; }
            [System.ComponentModel.DataAnnotations.Required]
            public string title { get; set; }
        }

        [Fact]
        public void NotMappedAttribute()
        {
            fsql.CodeFirst.SyncStructure<eftesttb05>();
            Assert.False(fsql.CodeFirst.GetTableByEntity(typeof(eftesttb05)).ColumnsByCsIgnore.ContainsKey("id"));
            Assert.True(fsql.CodeFirst.GetTableByEntity(typeof(eftesttb05)).ColumnsByCsIgnore.ContainsKey("title"));
        }
        class eftesttb05
        {
            public Guid id { get; set; }
            [System.ComponentModel.DataAnnotations.Schema.NotMapped]
            public string title { get; set; }
        }

        [Fact]
        public void ColumnAttribute()
        {
            fsql.CodeFirst.SyncStructure<eftesttb06>();
            Assert.Equal("title_01", fsql.CodeFirst.GetTableByEntity(typeof(eftesttb06)).ColumnsByCs["title1"].Attribute.Name);
            Assert.Equal("title_02", fsql.CodeFirst.GetTableByEntity(typeof(eftesttb06)).ColumnsByCs["title2"].Attribute.Name);
            Assert.Equal("title_03", fsql.CodeFirst.GetTableByEntity(typeof(eftesttb06)).ColumnsByCs["title3"].Attribute.Name);

            Assert.Equal(99, fsql.CodeFirst.GetTableByEntity(typeof(eftesttb06)).ColumnsByCs["title2"].Attribute.Position);
            Assert.Equal(98, fsql.CodeFirst.GetTableByEntity(typeof(eftesttb06)).ColumnsByCs["title3"].Attribute.Position);

            Assert.Equal("NVARCHAR(255)", fsql.CodeFirst.GetTableByEntity(typeof(eftesttb06)).ColumnsByCs["title1"].Attribute.DbType);
            Assert.Equal("NVARCHAR(255)", fsql.CodeFirst.GetTableByEntity(typeof(eftesttb06)).ColumnsByCs["title2"].Attribute.DbType);
            Assert.Equal("VARCHAR(100)", fsql.CodeFirst.GetTableByEntity(typeof(eftesttb06)).ColumnsByCs["title3"].Attribute.DbType);
        }
        class eftesttb06
        {
            public Guid id { get; set; }
            [System.ComponentModel.DataAnnotations.Schema.Column("title_01")]
            public string title1 { get; set; }
            [System.ComponentModel.DataAnnotations.Schema.Column("title_02", Order = 99)]
            public string title2 { get; set; }
            [System.ComponentModel.DataAnnotations.Schema.Column("title_03", Order = 98, TypeName = "varchar(100)")]
            public string title3 { get; set; }
        }

        [Fact]
        public void KeyAttribute()
        {
            fsql.CodeFirst.SyncStructure<eftesttb07>();
            Assert.True(fsql.CodeFirst.GetTableByEntity(typeof(eftesttb07)).ColumnsByCs["title"].Attribute.IsPrimary);
        }
        class eftesttb07
        {
            public Guid id { get; set; }
            [System.ComponentModel.DataAnnotations.Key]
            public string title { get; set; }
        }

        [Fact]
        public void DatabaseGeneratedAttribute()
        {
            fsql.CodeFirst.SyncStructure<eftesttb08>();
            Assert.True(fsql.CodeFirst.GetTableByEntity(typeof(eftesttb08)).ColumnsByCs["id"].Attribute.IsPrimary);
            Assert.True(fsql.CodeFirst.GetTableByEntity(typeof(eftesttb08)).ColumnsByCs["id"].Attribute.IsIdentity);
            Assert.False(fsql.CodeFirst.GetTableByEntity(typeof(eftesttb08)).ColumnsByCs["createtime"].Attribute.CanInsert);
            Assert.False(fsql.CodeFirst.GetTableByEntity(typeof(eftesttb08)).ColumnsByCs["createtime"].Attribute.CanUpdate);
        }
        class eftesttb08
        {
            [System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
            public int id { get; set; }
            public string title { get; set; }
            [System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed)]
            public string createtime { get; set; }
        }
    }
}
