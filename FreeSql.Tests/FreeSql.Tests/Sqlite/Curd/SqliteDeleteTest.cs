using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace FreeSql.Tests.Sqlite
{
    public class SqliteDeleteTest
    {

        IDelete<Topic> delete => g.sqlite.Delete<Topic>(); //��������

        [Table(Name = "tb_topic22211")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int? Clicks { get; set; }
            public TestTypeInfo Type { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void Dywhere()
        {
            Assert.Null(g.sqlite.Delete<Topic>().ToSql());
            var sql = g.sqlite.Delete<Topic>(new[] { 1, 2 }).ToSql();
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" IN (1,2))", sql);

            sql = g.sqlite.Delete<Topic>(new Topic { Id = 1, Title = "test" }).ToSql();
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" = 1)", sql);

            sql = g.sqlite.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).ToSql();
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" IN (1,2))", sql);

            sql = g.sqlite.Delete<Topic>(new { id = 1 }).ToSql();
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" = 1)", sql);

            sql = g.sqlite.Delete<MultiPkTopic>(new[] { new { Id1 = 1, Id2 = 10 }, new { Id1 = 2, Id2 = 20 } }).ToSql();
            Assert.Equal("DELETE FROM \"MultiPkTopic\" WHERE (\"Id1\" = 1 AND \"Id2\" = 10 OR \"Id1\" = 2 AND \"Id2\" = 20)", sql);
        }
        class MultiPkTopic
        {
            [Column(IsPrimary = true)]
            public int Id1 { get; set; }
            [Column(IsPrimary = true)]
            public int Id2 { get; set; }
            public int Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void Where()
        {
            var sql = delete.Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" = 1)", sql);

            sql = delete.Where("id = @id", new { id = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (id = @id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = delete.Where(item).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            sql = delete.Where(items).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void ExecuteAffrows()
        {

            var id = g.sqlite.Insert<Topic>(new Topic { Title = "xxxx", CreateTime = DateTime.Now }).ExecuteIdentity();
            Assert.Equal(1, delete.Where(a => a.Id == id).ExecuteAffrows());
        }
        [Fact]
        public void ExecuteDeleted()
        {

            //var item = g.Sqlite.Delete<Topic>(new Topic { Title = "xxxx", CreateTime = DateTime.Now }).ExecuteInserted();
            //Assert.Equal(item[0].Id, delete.Where(a => a.Id == item[0].Id).ExecuteDeleted()[0].Id);
        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.sqlite.Delete<Topic>().AsTable(a => "TopicAsTable").ToSql());
            var sql = g.sqlite.Delete<Topic>(new[] { 1, 2 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TopicAsTable\" WHERE (\"Id\" IN (1,2))", sql);

            sql = g.sqlite.Delete<Topic>(new Topic { Id = 1, Title = "test" }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TopicAsTable\" WHERE (\"Id\" = 1)", sql);

            sql = g.sqlite.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TopicAsTable\" WHERE (\"Id\" IN (1,2))", sql);

            sql = g.sqlite.Delete<Topic>(new { id = 1 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TopicAsTable\" WHERE (\"Id\" = 1)", sql);
        }
    }
}

public static class DeleteExtensions
{
    public static string ToSqlCascade<T>(this IDelete<T> that)
    {
        var delete = that as DeleteProvider;
        if (delete == null) return null;
        if (delete._whereTimes <= 0 || delete._where.Length == 0) return null;
        if (LocalGetNavigates(delete._table).Any() == false) return that.ToSql();

        var fsql = delete._orm;
        var sb = new StringBuilder();
        Dictionary<string, bool> eachdic = new Dictionary<string, bool>();

        var rootSel = fsql.Select<object>().AsType(delete._table.Type).Where(delete._where.ToString());
        var rootItems = rootSel.ToList();
        LocalEach(delete._table.Type, rootItems, true);
        return sb.ToString();

        List<NativeTuple<TableRef, PropertyInfo>> LocalGetNavigates(TableInfo tb)
        {
            return tb.Properties.Where(a => tb.ColumnsByCs.ContainsKey(a.Key) == false)
                .Select(a => new NativeTuple<TableRef, PropertyInfo>(tb.GetTableRef(a.Key, false), a.Value))
                .Where(a => a.Item1 != null && a.Item1.RefType != TableRefType.ManyToOne)
                .ToList();
        }
        void LocalEach(Type itemType, List<object> items, bool isOneToOne)
        {
            items = items?.Where(item =>
            {
                var itemKeystr = FreeSql.Extensions.EntityUtil.EntityUtilExtensions.GetEntityKeyString(fsql, itemType, item, false);
                var eachdicKey = $"{itemType.FullName},{itemKeystr}";
                if (eachdic.ContainsKey(eachdicKey)) return false;
                eachdic.Add(eachdicKey, true);
                return true;
            }).ToList();
            if (items?.Any() != true) return;

            var tb = fsql.CodeFirst.GetTableByEntity(itemType);
            var navs = LocalGetNavigates(tb);

            var otos = navs.Where(a => a.Item1.RefType == TableRefType.OneToOne).ToList();
            if (otos.Any())
            {
                foreach (var oto in otos)
                {
                    var childsSel = fsql.Select<object>().AsType(oto.Item1.RefEntityType) as Select1Provider<object>;
                    var refitems = items.Select(item =>
                    {
                        var refitem = oto.Item1.RefEntityType.CreateInstanceGetDefaultValue();
                        for (var a = 0; a < oto.Item1.Columns.Count; a++)
                        {
                            var colval = FreeSql.Extensions.EntityUtil.EntityUtilExtensions.GetPropertyValue(tb, item, oto.Item1.Columns[a].CsName);
                            FreeSql.Extensions.EntityUtil.EntityUtilExtensions.SetPropertyValue(childsSel._tables[0].Table, refitem, oto.Item1.RefColumns[a].CsName, colval);
                        }
                        return refitem;
                    }).ToList();

                    childsSel.Where(childsSel._commonUtils.WhereItems(oto.Item1.RefColumns.ToArray(), "a.", refitems));
                    var childs = childsSel.ToList();
                    LocalEach(oto.Item1.RefEntityType, childs, false);
                }
            }

            var otms = navs.Where(a => a.Item1.RefType == TableRefType.OneToMany).ToList();
            if (otms.Any())
            {
                foreach (var otm in otms)
                {
                    var childsSel = fsql.Select<object>().AsType(otm.Item1.RefEntityType) as Select1Provider<object>;
                    var refitems = items.Select(item =>
                    {
                        var refitem = otm.Item1.RefEntityType.CreateInstanceGetDefaultValue();
                        for (var a = 0; a < otm.Item1.Columns.Count; a++)
                        {
                            var colval = FreeSql.Extensions.EntityUtil.EntityUtilExtensions.GetPropertyValue(tb, item, otm.Item1.Columns[a].CsName);
                            FreeSql.Extensions.EntityUtil.EntityUtilExtensions.SetPropertyValue(childsSel._tables[0].Table, refitem, otm.Item1.RefColumns[a].CsName, colval);
                        }
                        return refitem;
                    }).ToList();

                    childsSel.Where(childsSel._commonUtils.WhereItems(otm.Item1.RefColumns.ToArray(), "a.", refitems));
                    var childs = childsSel.ToList();
                    LocalEach(otm.Item1.RefEntityType, childs, true);
                }
            }

            var mtms = navs.Where(a => a.Item1.RefType == TableRefType.ManyToMany).ToList();
            if (mtms.Any())
            {
                foreach (var mtm in mtms)
                {
                    var childsSel = fsql.Select<object>().AsType(mtm.Item1.RefMiddleEntityType) as Select1Provider<object>;
                    var miditems = items.Select(item =>
                    {
                        var refitem = mtm.Item1.RefMiddleEntityType.CreateInstanceGetDefaultValue();
                        for (var a = 0; a < mtm.Item1.Columns.Count; a++)
                        {
                            var colval = FreeSql.Extensions.EntityUtil.EntityUtilExtensions.GetPropertyValue(tb, item, mtm.Item1.Columns[a].CsName);
                            FreeSql.Extensions.EntityUtil.EntityUtilExtensions.SetPropertyValue(childsSel._tables[0].Table, refitem, mtm.Item1.MiddleColumns[a].CsName, colval);
                        }
                        return refitem;
                    }).ToList();

                    childsSel.Where(childsSel._commonUtils.WhereItems(mtm.Item1.MiddleColumns.Take(mtm.Item1.Columns.Count).ToArray(), "a.", miditems));
                    var childs = childsSel.ToList();
                    LocalEach(mtm.Item1.RefEntityType, childs, true);
                }
            }

            var delSql = fsql.Delete<object>().AsType(itemType).WhereDynamic(items).ToSql();
            if (string.IsNullOrWhiteSpace(delSql)) throw new Exception($"ToSqlCascade 失败");
            if (sb.Length > 0) sb.Append("\r\n\r\n;\r\n\r\n");
            sb.Append(delSql);
        }
    }
}
