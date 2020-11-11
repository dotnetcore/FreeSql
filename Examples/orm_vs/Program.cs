using FreeSql.Internal;
using Microsoft.EntityFrameworkCore;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace orm_vs
{
    class Program
    {
        static IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                //.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=20")
                .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=20")
                //.UseConnectionString(FreeSql.DataType.PostgreSQL, "Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=tedb;Pooling=true;Maximum Pool Size=20")
                .UseAutoSyncStructure(false)
                .UseNoneCommandParameter(true)
                //.UseConfigEntityFromDbFirst(true)
                .Build();

        static SqlSugarClient sugar
        {
            get => new SqlSugarClient(new ConnectionConfig()
            {
                //ConnectionString = "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=20;Max Pool Size=20",
                //DbType = DbType.SqlServer,
                ConnectionString = "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Min Pool Size=20;Max Pool Size=20",
                DbType = DbType.MySql,
                //ConnectionString = "Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=tedb;Pooling=true;Maximum Pool Size=21",
                //DbType = DbType.PostgreSQL,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        class SongContext : DbContext
        {
            public DbSet<Song> Songs { get; set; }
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                //optionsBuilder.UseSqlServer(@"Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Min Pool Size=21;Max Pool Size=21");
                optionsBuilder.UseMySql("Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Min Pool Size=21;Max Pool Size=21");
                //optionsBuilder.UseNpgsql("Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=tedb;Pooling=true;Maximum Pool Size=21");
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Song>()
                    .Property(a => a.create_time)
                    .HasConversion(a => int.Parse(a.ToString()), a => new DateTime(a));
            }
        }

        static void Main(string[] args)
        {
            //fsql.CodeFirst.SyncStructure(typeof(Song), typeof(Song_tag), typeof(Tag));
            //sugar.CodeFirst.InitTables(typeof(Song), typeof(Song_tag), typeof(Tag));
            //sugar创建表失败：SqlSugar.SqlSugarException: Sequence contains no elements
            fsql.CodeFirst.SyncStructure(typeof(Song), "freesql_song");
            fsql.CodeFirst.SyncStructure(typeof(Song), "sugar_song");
            fsql.CodeFirst.SyncStructure(typeof(Song), "efcore_song");

            fsql.CodeFirst.SyncStructure(typeof(Song_tag), "freesql_song_tag");
            fsql.CodeFirst.SyncStructure(typeof(Song_tag), "sugar_song_tag");
            fsql.CodeFirst.SyncStructure(typeof(Song_tag), "efcore_song_tag");

            fsql.CodeFirst.SyncStructure(typeof(Tag), "freesql_tag");
            fsql.CodeFirst.SyncStructure(typeof(Tag), "sugar_tag");
            fsql.CodeFirst.SyncStructure(typeof(Tag), "efcore_tag");

            var sb = new StringBuilder();
            var time = new Stopwatch();

            var sql222 = fsql.Select<Song>().Where(a => DateTime.Now.Subtract(a.create_time.Value).TotalHours > 0).ToSql();

            #region ET test
            ////var t31 = fsql.Select<xxx>().ToList();
            //fsql.Select<Song>().First();

            //time.Restart();
            //var t3 = fsql.Select<Song>().ToList();
            //time.Stop();
            //sb.AppendLine($"Elapsed: {time.Elapsed}; ToList Entity Counts: {t3.Count}; ORM: FreeSql*");

            //time.Restart();
            //var adoarr1 = fsql.Ado.ExecuteArray("select * from freesql_song");
            //time.Stop();
            //sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteArray Entity Counts: {adoarr1.Length}; ORM: FreeSql ExecuteArray*");

            //time.Restart();
            //var adolist1 = new List<Song>();
            //fsql.Ado.ExecuteReader(dr =>
            //{
            //    var xim = new Song();
            //    dr.GetValue(0)?.GetType();
            //    dr.GetValue(1)?.GetType();
            //    dr.GetValue(2)?.GetType();
            //    dr.GetValue(3)?.GetType();
            //    dr.GetValue(4)?.GetType();
            //    adolist1.Add(xim);
            //}, "select * from freesql_song");
            //time.Stop();
            //sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReader Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReader*");


            //time.Restart();
            //adolist1 = new List<Song>();
            //fsql.Ado.ExecuteReader(dr =>
            //{
            //    var xim = new Song();
            //    var v1 = dr.GetValue(0);
            //    var locvalue = (object)v1;
            //    if (locvalue == null || locvalue == DBNull.Value) xim.Id = default;
            //    else
            //    {
            //        if (locvalue is int iv) xim.Id = iv;
            //        else
            //        {
            //            if (locvalue is string)
            //            {

            //            }
            //        }
            //    }
            //    v1 = dr.GetValue(1);
            //    locvalue = (object)v1;
            //    if (locvalue == null || locvalue == DBNull.Value) xim.Create_time = default;
            //    else
            //    {
            //        if (locvalue is DateTime dt) xim.Create_time = dt;
            //        else
            //        {
            //            if (locvalue is string)
            //            {

            //            }
            //        }
            //    }
            //    v1 = dr.GetValue(2);
            //    locvalue = (object)v1;
            //    if (locvalue == null || locvalue == DBNull.Value) xim.Is_deleted = default;
            //    else
            //    {
            //        if (locvalue is bool bl) xim.Is_deleted = bl;
            //        else
            //        {
            //            if (locvalue is string)
            //            {

            //            }
            //        }
            //    }
            //    v1 = dr.GetValue(3);
            //    locvalue = (object)v1;
            //    if (locvalue == null || locvalue == DBNull.Value) xim.Title = default;
            //    else
            //    {
            //        if (locvalue is string str) xim.Title = str;
            //        else
            //        {
            //            if (locvalue is string)
            //            {

            //            }
            //        }
            //    }
            //    v1 = dr.GetValue(4);
            //    locvalue = (object)v1;
            //    if (locvalue == null || locvalue == DBNull.Value) xim.Url = default;
            //    else
            //    {
            //        if (locvalue is string str) xim.Url = str;
            //        else
            //        {
            //            if (locvalue is string)
            //            {

            //            }
            //        }
            //    }
            //    adolist1.Add(xim);
            //}, "select * from freesql_song");
            //time.Stop();
            //sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReaderObject Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReaderObject*");

            ////var type = typeof(Song);
            ////var myfuncParam1 = Expression.Parameter(typeof(object[]), "values");
            ////var retExp = Expression.Variable(type, "ret");
            ////var objExp = Expression.Variable(typeof(object), "obj");
            ////var returnTarget = Expression.Label(type);
            ////var myfuncBody = Expression.Block(
            ////    new[] { retExp, objExp },
            ////    Expression.Assign(retExp, type.InternalNewExpression()),
            ////    Expression.Assign(objExp, Expression.ArrayIndex(myfuncParam1, Expression.Constant(0))),
            ////    Utils.GetConvertExpression(type.GetProperty("Id").PropertyType, objExp),
            ////    Expression.Assign(Expression.MakeMemberAccess(retExp, type.GetProperty("Id")), Expression.Convert(objExp, type.GetProperty("Id").PropertyType)),
            ////    Expression.Assign(objExp, Expression.ArrayIndex(myfuncParam1, Expression.Constant(1))),
            ////    Utils.GetConvertExpression(type.GetProperty("Create_time").PropertyType, objExp),
            ////    Expression.Assign(Expression.MakeMemberAccess(retExp, type.GetProperty("Create_time")), Expression.Convert(objExp, type.GetProperty("Create_time").PropertyType)),
            ////    Expression.Assign(objExp, Expression.ArrayIndex(myfuncParam1, Expression.Constant(2))),
            ////    Utils.GetConvertExpression(type.GetProperty("Is_deleted").PropertyType, objExp),
            ////    Expression.Assign(Expression.MakeMemberAccess(retExp, type.GetProperty("Is_deleted")), Expression.Convert(objExp, type.GetProperty("Is_deleted").PropertyType)),
            ////    Expression.Assign(objExp, Expression.ArrayIndex(myfuncParam1, Expression.Constant(3))),
            ////    Utils.GetConvertExpression(type.GetProperty("Title").PropertyType, objExp),
            ////    Expression.Assign(Expression.MakeMemberAccess(retExp, type.GetProperty("Title")), Expression.Convert(objExp, type.GetProperty("Title").PropertyType)),
            ////    Expression.Assign(objExp, Expression.ArrayIndex(myfuncParam1, Expression.Constant(4))),
            ////    Utils.GetConvertExpression(type.GetProperty("Url").PropertyType, objExp),
            ////    Expression.Assign(Expression.MakeMemberAccess(retExp, type.GetProperty("Url")), Expression.Convert(objExp, type.GetProperty("Url").PropertyType)),
            ////    Expression.Return(returnTarget, retExp),
            ////    Expression.Label(returnTarget, Expression.Default(type))
            ////);
            ////var myfunc = Expression.Lambda<Func<object[], Song>>(myfuncBody, myfuncParam1).Compile();
            ////time.Restart();
            ////adolist1 = new List<Song>();
            ////fsql.Ado.ExecuteReader(dr =>
            ////{
            ////    var values = new object[dr.FieldCount];
            ////    dr.GetValues(values);
            ////    var xim = myfunc(values);
            ////    adolist1.Add(xim);
            ////}, "select * from freesql_song");
            ////time.Stop();
            ////sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReaderMyFunc Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReaderMyFunc*");


            ////var methodDrgv = typeof(DbDataReader).GetMethod("GetValue");
            ////var myfunc2Param1 = Expression.Parameter(typeof(DbDataReader), "dr");
            ////var myfunc2Body = Expression.Block(
            ////    new[] { retExp, objExp },
            ////    Expression.Assign(retExp, type.InternalNewExpression()),
            ////    Expression.Assign(objExp, Expression.Call(myfunc2Param1, methodDrgv, Expression.Constant(0))),
            ////    Utils.GetConvertExpression(type.GetProperty("Id").PropertyType, objExp),
            ////    Expression.Assign(Expression.MakeMemberAccess(retExp, type.GetProperty("Id")), Expression.Convert(objExp, type.GetProperty("Id").PropertyType)),
            ////    Expression.Assign(objExp, Expression.Call(myfunc2Param1, methodDrgv, Expression.Constant(1))),
            ////    Utils.GetConvertExpression(type.GetProperty("Create_time").PropertyType, objExp),
            ////    Expression.Assign(Expression.MakeMemberAccess(retExp, type.GetProperty("Create_time")), Expression.Convert(objExp, type.GetProperty("Create_time").PropertyType)),
            ////    Expression.Assign(objExp, Expression.Call(myfunc2Param1, methodDrgv, Expression.Constant(2))),
            ////    Utils.GetConvertExpression(type.GetProperty("Is_deleted").PropertyType, objExp),
            ////    Expression.Assign(Expression.MakeMemberAccess(retExp, type.GetProperty("Is_deleted")), Expression.Convert(objExp, type.GetProperty("Is_deleted").PropertyType)),
            ////    Expression.Assign(objExp, Expression.Call(myfunc2Param1, methodDrgv, Expression.Constant(3))),
            ////    Utils.GetConvertExpression(type.GetProperty("Title").PropertyType, objExp),
            ////    Expression.Assign(Expression.MakeMemberAccess(retExp, type.GetProperty("Title")), Expression.Convert(objExp, type.GetProperty("Title").PropertyType)),
            ////    Expression.Assign(objExp, Expression.Call(myfunc2Param1, methodDrgv, Expression.Constant(4))),
            ////    Utils.GetConvertExpression(type.GetProperty("Url").PropertyType, objExp),
            ////    Expression.Assign(Expression.MakeMemberAccess(retExp, type.GetProperty("Url")), Expression.Convert(objExp, type.GetProperty("Url").PropertyType)),
            ////    Expression.Return(returnTarget, retExp),
            ////    Expression.Label(returnTarget, Expression.Default(type))
            ////);
            ////var myfunc2 = Expression.Lambda<Func<DbDataReader, Song>>(myfunc2Body, myfunc2Param1).Compile();
            ////time.Restart();
            ////adolist1 = new List<Song>();
            ////fsql.Ado.ExecuteReader(dr =>
            ////{
            ////    var xim = myfunc2(dr);
            ////    adolist1.Add(xim);
            ////}, "select * from freesql_song");
            ////time.Stop();
            ////sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReaderMyFunc22 Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReaderMyFunc22*");


            //time.Restart();
            //adolist1 = new List<Song>();
            //fsql.Ado.ExecuteReader(dr =>
            //{
            //    var xim = new Song();
            //    dr.GetFieldValue<int>(0);
            //    dr.GetFieldValue<DateTime>(1);
            //    dr.GetFieldValue<bool>(2);
            //    dr.GetFieldValue<string>(3);
            //    dr.GetFieldValue<string>(4);
            //    adolist1.Add(xim);
            //}, "select * from freesql_song");
            //time.Stop();
            //sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReader0000 Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReader0000*");

            //time.Restart();
            //adolist1 = new List<Song>();
            //fsql.Ado.ExecuteReader(dr =>
            //{
            //    var xim = new Song();
            //    Utils.GetDataReaderValue(typeof(int), dr.GetValue(0));
            //    Utils.GetDataReaderValue(typeof(DateTime), dr.GetValue(1));
            //    Utils.GetDataReaderValue(typeof(bool), dr.GetValue(2));
            //    Utils.GetDataReaderValue(typeof(string), dr.GetValue(3));
            //    Utils.GetDataReaderValue(typeof(string), dr.GetValue(4));
            //    adolist1.Add(xim);
            //}, "select * from freesql_song");
            //time.Stop();
            //sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReader1111 Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReader1111*");


            ////time.Restart();
            ////adolist1 = new List<Song>();
            ////fsql.Ado.ExecuteReader(dr =>
            ////{
            ////    var xim = new Song();
            ////    Utils.GetConvertValue(typeof(int), dr.GetValue(0));
            ////    Utils.GetConvertValue(typeof(DateTime), dr.GetValue(1));
            ////    Utils.GetConvertValue(typeof(bool), dr.GetValue(2));
            ////    Utils.GetConvertValue(typeof(string), dr.GetValue(3));
            ////    Utils.GetConvertValue(typeof(string), dr.GetValue(4));
            ////    adolist1.Add(xim);
            ////}, "select * from freesql_song");
            ////time.Stop();
            ////sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReader11112222 Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReader11112222*");


            //time.Restart();
            //adolist1 = new List<Song>();
            //fsql.Ado.ExecuteReader(dr =>
            //{
            //    var values = new object[dr.FieldCount];
            //    dr.GetValues(values);

            //    var xim = new Song();
            //    xim.Id = (int)Utils.GetDataReaderValue(typeof(int), values[0]);
            //    xim.Create_time = (DateTime)Utils.GetDataReaderValue(typeof(DateTime), values[1]);
            //    xim.Is_deleted = (bool)Utils.GetDataReaderValue(typeof(bool), values[2]);
            //    xim.Title = (string)Utils.GetDataReaderValue(typeof(string), values[3]);
            //    xim.Url = (string)Utils.GetDataReaderValue(typeof(string), values[4]);
            //    adolist1.Add(xim);
            //}, "select * from freesql_song");
            //time.Stop();
            //sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReader1111 Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReader1111*");


            ////time.Restart();
            ////adolist1 = new List<Song>();
            ////fsql.Ado.ExecuteReader(dr =>
            ////{
            ////    var values = new object[dr.FieldCount];
            ////    dr.GetValues(values);

            ////    var xim = new Song();
            ////    xim.Id = (int)Utils.GetConvertValue(typeof(int), values[0]);
            ////    xim.Create_time = (DateTime)Utils.GetConvertValue(typeof(DateTime), values[1]);
            ////    xim.Is_deleted = (bool)Utils.GetConvertValue(typeof(bool), values[2]);
            ////    xim.Title = (string)Utils.GetConvertValue(typeof(string), values[3]);
            ////    xim.Url = (string)Utils.GetConvertValue(typeof(string), values[4]);
            ////    adolist1.Add(xim);
            ////}, "select * from freesql_song");
            ////time.Stop();
            ////sb.AppendLine($"Elapsed: {time.Elapsed}; ExecuteReader11112222 Entity Counts: {adolist1.Count}; ORM: FreeSql ExecuteReader11112222*");


            //time.Restart();
            //List<Song> dplist1 = null;
            //using (var conn = fsql.Ado.MasterPool.Get())
            //{
            //    dplist1 = Dapper.SqlMapper.Query<Song>(conn.Value, "select * from freesql_song").ToList();
            //}
            //time.Stop();
            //sb.AppendLine($"Elapsed: {time.Elapsed}; Query Entity Counts: {dplist1.Count}; ORM: Dapper");

            //time.Restart();
            //t3 = fsql.Select<Song>().ToList();
            //time.Stop();
            //sb.AppendLine($"Elapsed: {time.Elapsed}; ToList Entity Counts: {t3.Count}; ORM: FreeSql*");

            //Console.WriteLine(sb.ToString());
            //Console.ReadKey();

            #endregion

            var testlist1 = fsql.Select<Song>().OrderBy(a => a.id).ToList();
            var testlist2 = new List<Song>();
            fsql.Select<Song>().OrderBy(a => a.id).ToChunk(2, fetch =>
            {
                testlist2.AddRange(fetch.Object);
            });

            var testlist22 = new List<object>();
            fsql.Select<Song, Song_tag>().LeftJoin((a, b) => a.id == b.song_id).ToChunk((a, b) => new { a.title, a.create_time, b.tag_id }, 2, fetch =>
            {
                testlist22.AddRange(fetch.Object);
            });

            //sugar.Aop.OnLogExecuted = (s, e) =>
            //{
            //    Trace.WriteLine(s);
            //};
            //测试前清空数据
            fsql.Delete<Song>().Where(a => a.id > 0).ExecuteAffrows();
            sugar.Deleteable<Song>().Where(a => a.id > 0).ExecuteCommand();
            fsql.Ado.ExecuteNonQuery("delete from efcore_song");

            Console.WriteLine("插入性能：");
            Insert(sb, 100, 1);
            Console.Write(sb.ToString());
            sb.Clear();
            Insert(sb, 100, 10);
            Console.Write(sb.ToString());
            sb.Clear();
            
            Insert(sb, 1, 1000);
            Console.Write(sb.ToString());
            sb.Clear();
            Insert(sb, 1, 10000);
            Console.Write(sb.ToString());
            sb.Clear();
            Insert(sb, 1, 50000);
            Console.Write(sb.ToString());
            sb.Clear();
            Insert(sb, 1, 100000);
            Console.Write(sb.ToString());
            sb.Clear();

            Console.WriteLine("查询性能：");
            Select(sb, 100, 1);
            Console.Write(sb.ToString());
            sb.Clear();
            Select(sb, 100, 10);
            Console.Write(sb.ToString());
            sb.Clear();

            Select(sb, 1, 1000);
            Console.Write(sb.ToString());
            sb.Clear();
            Select(sb, 1, 10000);
            Console.Write(sb.ToString());
            sb.Clear();
            Select(sb, 1, 50000);
            Console.Write(sb.ToString());
            sb.Clear();
            Select(sb, 1, 100000);
            Console.Write(sb.ToString());
            sb.Clear();

            Console.WriteLine("更新：");
            Update(sb, 100, 1);
            Console.Write(sb.ToString());
            sb.Clear();
            Update(sb, 100, 10);
            Console.Write(sb.ToString());
            sb.Clear();

            Update(sb, 1, 1000);
            Console.Write(sb.ToString());
            sb.Clear();
            Update(sb, 1, 10000);
            Console.Write(sb.ToString());
            sb.Clear();
            Update(sb, 1, 50000);
            Console.Write(sb.ToString());
            sb.Clear();
            Update(sb, 1, 100000);
            Console.Write(sb.ToString());
            sb.Clear();

            Console.WriteLine("测试结束，按任意键退出...");
            Console.ReadKey();
        }

        static void Select(StringBuilder sb, int forTime, int size)
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            for (var a = 0; a < forTime; a++)
                fsql.Select<Song>().Limit(size).ToList();
            sw.Stop();
            sb.AppendLine($"FreeSql Select {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms");

            sw.Restart();
            for (var a = 0; a < forTime; a++)
                sugar.Queryable<Song>().Take(size).ToList();
            sw.Stop();
            sb.AppendLine($"SqlSugar Select {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms");

            sw.Restart();
            for (var a = 0; a < forTime; a++)
            {
                using (var db = new SongContext())
                {
                    //db.Songs.Take(size).AsNoTracking().ToList();
                }
            }
            sw.Stop();
            sb.AppendLine($"EFCore Select {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms .net5.0无效");

            sw.Restart();
            using (var conn = fsql.Ado.MasterPool.Get())
            {
                for (var a = 0; a < forTime; a++)
                    Dapper.SqlMapper.Query<Song>(conn.Value, $"select top {size} * from freesql_song").ToList();
            }
            sw.Stop();
            sb.AppendLine($"Dapper Select {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms\r\n");
        }

        static void Insert(StringBuilder sb, int forTime, int size)
        {
            var songs = Enumerable.Range(0, size).Select(a => new Song
            {
                create_time = DateTime.Now,
                is_deleted = false,
                title = $"Insert_{a}",
                url = $"Url_{a}"
            });

            //预热
            fsql.Insert(songs.First()).ExecuteAffrows();
            sugar.Insertable(songs.First()).ExecuteCommand();
            using (var db = new SongContext())
            {
                //db.Configuration.AutoDetectChangesEnabled = false;
                //db.Songs.AddRange(songs.First());
                //db.SaveChanges(); //.net5.0 throw Microsoft.EntityFrameworkCore.DbUpdateException
            }
            Stopwatch sw = new Stopwatch();

            sw.Restart();
            for (var a = 0; a < forTime; a++)
            {
                fsql.Insert(songs).ExecuteAffrows();
                //using (var db = new FreeSongContext()) {
                //	//db.Configuration.AutoDetectChangesEnabled = false;
                //	db.Songs.AddRange(songs.ToArray());
                //	db.SaveChanges();
                //}
            }
            sw.Stop();
            sb.AppendLine($"FreeSql Insert {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms");

            sw.Restart();
            Exception sugarEx = null;
            try
            {
                for (var a = 0; a < forTime; a++)
                    sugar.Insertable(songs.ToArray()).ExecuteCommand();
            }
            catch (Exception ex)
            {
                sugarEx = ex;
            }
            sw.Stop();
            sb.AppendLine($"SqlSugar Insert {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms" + (sugarEx != null ? $"成绩无效，错误：{sugarEx.Message}" : ""));

            sw.Restart();
            for (var a = 0; a < forTime; a++)
            {

                using (var db = new SongContext())
                {
                    //db.Configuration.AutoDetectChangesEnabled = false;
                    //db.Songs.AddRange(songs.ToArray());
                    //db.SaveChanges(); //.net5.0 throw Microsoft.EntityFrameworkCore.DbUpdateException
                }
            }
            sw.Stop();
            sb.AppendLine($"EFCore Insert {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms .net5.0无效\r\n");
        }

        static void Update(StringBuilder sb, int forTime, int size)
        {
            Stopwatch sw = new Stopwatch();

            var songs = fsql.Select<Song>().Limit(size).ToList();
            sw.Restart();
            for (var a = 0; a < forTime; a++)
            {
                fsql.Update<Song>().SetSource(songs).ExecuteAffrows();
            }
            sw.Stop();
            sb.AppendLine($"FreeSql Update {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms");

            songs = sugar.Queryable<Song>().Take(size).ToList();
            sw.Restart();
            Exception sugarEx = null;
            try
            {
                for (var a = 0; a < forTime; a++)
                    sugar.Updateable(songs).ExecuteCommand();
            }
            catch (Exception ex)
            {
                sugarEx = ex;
            }
            sw.Stop();
            sb.AppendLine($"SqlSugar Update {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms" + (sugarEx != null ? $"成绩无效，错误：{sugarEx.Message}" : ""));

            using (var db = new SongContext())
            {
                songs = db.Songs.Take(size).AsNoTracking().ToList();
            }
            sw.Restart();
            for (var a = 0; a < forTime; a++)
            {

                using (var db = new SongContext())
                {
                    //db.Configuration.AutoDetectChangesEnabled = false;
                    //db.Songs.UpdateRange(songs.ToArray());
                    //db.SaveChanges();
                }
            }
            sw.Stop();
            sb.AppendLine($"EFCore Update {size}条数据，循环{forTime}次，耗时{sw.ElapsedMilliseconds}ms .net5.0无效\r\n");
        }
    }

    [FreeSql.DataAnnotations.Table(Name = "freesql_song")]
    [SugarTable("sugar_song")]
    [Table("efcore_song")]
    public class Song
    {
        [FreeSql.DataAnnotations.Column(IsIdentity = true)]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public DateTime? create_time { get; set; }
        public bool? is_deleted { get; set; }
        public string title { get; set; }
        public string url { get; set; }

        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual ICollection<Tag> Tags { get; set; }
    }
    [FreeSql.DataAnnotations.Table(Name = "freesql_song_tag")]
    [SugarTable("sugar_song_tag")]
    [Table("efcore_song_tag")]
    public class Song_tag
    {
        public int song_id { get; set; }
        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual Song Song { get; set; }

        public int tag_id { get; set; }
        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual Tag Tag { get; set; }
    }
    [FreeSql.DataAnnotations.Table(Name = "freesql_tag")]
    [SugarTable("sugar_tag")]
    [Table("efcore_tag")]
    public class Tag
    {
        [FreeSql.DataAnnotations.Column(IsIdentity = true)]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public int? parent_id { get; set; }
        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual Tag Parent { get; set; }

        public decimal? ddd { get; set; }
        public string name { get; set; }

        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual ICollection<Song> Songs { get; set; }
        [SugarColumn(IsIgnore = true)]
        [NotMapped]
        public virtual ICollection<Tag> Tags { get; set; }
    }
}
