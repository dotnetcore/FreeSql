using FreeSql.DataAnnotations;
using System;
using System.Diagnostics;
using Xunit;

namespace FreeSql.Tests.SqlServer
{
    public class SqlServerSelectWithTempQueryTest
    {
        [Fact]
        public void SingleTablePartitionBy()
        {
            var fsql = g.sqlserver;

            fsql.Delete<SingleTablePartitionBy_User>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new[] {
                new SingleTablePartitionBy_User { Id = 1, Nickname = "name01" },
                new SingleTablePartitionBy_User { Id = 2, Nickname = "name01" },
                new SingleTablePartitionBy_User { Id = 3, Nickname = "name01" },
                new SingleTablePartitionBy_User { Id = 4, Nickname = "name02" },
                new SingleTablePartitionBy_User { Id = 5, Nickname = "name03" },
                new SingleTablePartitionBy_User { Id = 6, Nickname = "name03" },
            }).ExecuteAffrows();

            var sql01 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql();
            var assertSql01 = @"SELECT * 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [SingleTablePartitionBy_User] a ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql01, sql01);

            var sel01 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql01, sel01.ToSql());

            var list01 = sel01.ToList();
            Assert.Equal(3, list01.Count);
            Assert.Equal(list01[0].rownum, 1);
            Assert.Equal(list01[0].item.Id, 1);
            Assert.Equal(list01[0].item.Nickname, "name01");
            Assert.Equal(list01[1].rownum, 1);
            Assert.Equal(list01[1].item.Id, 4);
            Assert.Equal(list01[1].item.Nickname, "name02");
            Assert.Equal(list01[2].rownum, 1);
            Assert.Equal(list01[2].item.Id, 5);
            Assert.Equal(list01[2].item.Nickname, "name03");


            var sql02 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => a.item);
            var assertSql02 = @"SELECT a.[Id] as1, a.[Nickname] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [SingleTablePartitionBy_User] a ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql02, sql02);

            var sel02 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql02, sel02.ToSql(a => a.item));

            var list02 = sel02.ToList(a => a.item);
            Assert.Equal(3, list02.Count);
            Assert.Equal(list02[0].Id, 1);
            Assert.Equal(list02[0].Nickname, "name01");
            Assert.Equal(list02[1].Id, 4);
            Assert.Equal(list02[1].Nickname, "name02");
            Assert.Equal(list02[2].Id, 5);
            Assert.Equal(list02[2].Nickname, "name03");


            var sql03 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => new
                {
                    a.item.Id,
                    a.rownum
                });
            var assertSql03 = @"SELECT a.[Id] as1, a.[rownum] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [SingleTablePartitionBy_User] a ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql03, sql03);

            var sel03 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql03, sel03.ToSql(a => new
            {
                a.item.Id,
                a.rownum
            }));

            var list03 = sel03.ToList(a => new
            {
                a.item.Id,
                a.rownum
            });
            Assert.Equal(3, list03.Count);
            Assert.Equal(list03[0].rownum, 1);
            Assert.Equal(list03[0].Id, 1);
            Assert.Equal(list03[1].rownum, 1);
            Assert.Equal(list03[1].Id, 4);
            Assert.Equal(list03[2].rownum, 1);
            Assert.Equal(list03[2].Id, 5);



            var sql04 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    a.Id,
                    a.Nickname,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => new SingleTablePartitionBy_UserDto());
            var assertSql04 = @"SELECT a.[Id] as1, a.[rownum] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [SingleTablePartitionBy_User] a ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql04, sql04);

            var sel04 = fsql.Select<SingleTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    a.Id,
                    a.Nickname,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql04, sel04.ToSql(a => new SingleTablePartitionBy_UserDto()));

            var list04 = sel04.ToList<SingleTablePartitionBy_UserDto>();
            Assert.Equal(3, list04.Count);
            Assert.Equal(list04[0].rownum, 1);
            Assert.Equal(list04[0].Id, 1);
            Assert.Equal(list04[1].rownum, 1);
            Assert.Equal(list04[1].Id, 4);
            Assert.Equal(list04[2].rownum, 1);
            Assert.Equal(list04[2].Id, 5);


            var sql05 = fsql.Select<TwoTablePartitionBy_User>()
                 .Where(a => a.Id > 0)
                 .WithTempQuery(a => new
                 {
                     a.Id,
                     a.Nickname
                 })
                 .GroupBy(a => new { a.Nickname })
                 .WithTempQuery(a => new
                 {
                     a.Key,
                     sum1 = a.Sum(a.Value.Id),
                     cou1 = a.Count()
                 })
                 .ToSql();
            var assertSql05 = @"SELECT * 
FROM ( 
    SELECT a.[Nickname], sum(a.[Id]) [sum1], count(1) [cou1] 
    FROM ( 
        SELECT a.[Id], a.[Nickname] 
        FROM [TwoTablePartitionBy_User] a 
        WHERE (a.[Id] > 0) ) a 
    GROUP BY a.[Nickname] ) a";
            Assert.Equal(assertSql05, sql05);
            var list05 = fsql.Select<TwoTablePartitionBy_User>()
                 .Where(a => a.Id > 0)
                 .WithTempQuery(a => new
                 {
                     a.Id,
                     a.Nickname
                 })
                 .GroupBy(a => new { a.Nickname })
                 .WithTempQuery(a => new
                 {
                     a.Key,
                     sum1 = a.Sum(a.Value.Id),
                     cou1 = a.Count()
                 })
                 .ToList();
            Assert.Equal(3, list05.Count);
            Assert.Equal("name01", list05[0].Key.Nickname);
            Assert.Equal(6, list05[0].sum1);
            Assert.Equal(3, list05[0].cou1);
            Assert.Equal("name02", list05[1].Key.Nickname);
            Assert.Equal(4, list05[1].sum1);
            Assert.Equal(1, list05[1].cou1);
            Assert.Equal("name03", list05[2].Key.Nickname);
            Assert.Equal(11, list05[2].sum1);
            Assert.Equal(2, list05[2].cou1);
        }
        class SingleTablePartitionBy_User
        {
            public int Id { get; set; }
            public string Nickname { get; set; }
        }
        class SingleTablePartitionBy_UserDto
        {
            public int Id { get; set; }
            public int rownum { get; set; }
        }


        [Fact]
        public void TwoTablePartitionBy()
        {
            var fsql = g.sqlserver;

            fsql.Delete<TwoTablePartitionBy_User>().Where("1=1").ExecuteAffrows();
            fsql.Delete<TwoTablePartitionBy_UserExt>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new[] {
                new TwoTablePartitionBy_User { Id = 1, Nickname = "name01" },
                new TwoTablePartitionBy_User { Id = 2, Nickname = "name01" },
                new TwoTablePartitionBy_User { Id = 3, Nickname = "name01" },
                new TwoTablePartitionBy_User { Id = 4, Nickname = "name02" },
                new TwoTablePartitionBy_User { Id = 5, Nickname = "name03" },
                new TwoTablePartitionBy_User { Id = 6, Nickname = "name03" },
            }).ExecuteAffrows();
            fsql.Insert(new[] {
                new TwoTablePartitionBy_UserExt { UserId = 1, Remark = "remark01" },
                new TwoTablePartitionBy_UserExt { UserId = 2, Remark = "remark02" },
                new TwoTablePartitionBy_UserExt { UserId = 3, Remark = "remark03" },
                new TwoTablePartitionBy_UserExt { UserId = 4, Remark = "remark04" },
                new TwoTablePartitionBy_UserExt { UserId = 5, Remark = "remark05" },
                new TwoTablePartitionBy_UserExt { UserId = 6, Remark = "remark06" },
            }).ExecuteAffrows();

            var sql01 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql();
            var assertSql01 = @"SELECT * 
FROM ( 
    SELECT a.[Id], a.[Nickname], b.[UserId], b.[Remark], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql01, sql01);

            var sel01 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql01, sel01.ToSql());

            var list01 = sel01.ToList();
            Assert.Equal(3, list01.Count);
            Assert.Equal(list01[0].rownum, 1);
            Assert.Equal(list01[0].user.Id, 1);
            Assert.Equal(list01[0].user.Nickname, "name01");
            Assert.Equal(list01[0].userext.Remark, "remark01");
            Assert.Equal(list01[1].rownum, 1);
            Assert.Equal(list01[1].user.Id, 4);
            Assert.Equal(list01[1].user.Nickname, "name02");
            Assert.Equal(list01[1].userext.Remark, "remark04");
            Assert.Equal(list01[2].rownum, 1);
            Assert.Equal(list01[2].user.Id, 5);
            Assert.Equal(list01[2].user.Nickname, "name03");
            Assert.Equal(list01[2].userext.Remark, "remark05");


            var sql02 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => a.user);
            var assertSql02 = @"SELECT a.[Id] as1, a.[Nickname] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], b.[UserId], b.[Remark], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql02, sql02);

            var sel02 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql02, sel02.ToSql(a => a.user));

            var list02 = sel02.ToList(a => a.user);
            Assert.Equal(3, list02.Count);
            Assert.Equal(list02[0].Id, 1);
            Assert.Equal(list02[0].Nickname, "name01");
            Assert.Equal(list02[1].Id, 4);
            Assert.Equal(list02[1].Nickname, "name02");
            Assert.Equal(list02[2].Id, 5);
            Assert.Equal(list02[2].Nickname, "name03");


            var sql022 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => a.userext);
            var assertSql022 = @"SELECT a.[UserId] as1, a.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], b.[UserId], b.[Remark], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql022, sql022);

            var sel022 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql022, sel022.ToSql(a => a.userext));

            var list022 = sel022.ToList(a => a.userext);
            Assert.Equal(3, list022.Count);
            Assert.Equal(list022[0].UserId, 1);
            Assert.Equal(list022[0].Remark, "remark01");
            Assert.Equal(list022[1].UserId, 4);
            Assert.Equal(list022[1].Remark, "remark04");
            Assert.Equal(list022[2].UserId, 5);
            Assert.Equal(list022[2].Remark, "remark05");


            var sql03 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => new
                {
                    a.user.Id,
                    a.rownum
                });
            var assertSql03 = @"SELECT a.[Id] as1, a.[rownum] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], b.[UserId], b.[Remark], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql03, sql03);

            var sel03 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    user = a,
                    userext = b,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql03, sel03.ToSql(a => new
            {
                a.user.Id,
                a.rownum
            }));

            var list03 = sel03.ToList(a => new
            {
                a.user.Id,
                a.rownum
            });
            Assert.Equal(3, list03.Count);
            Assert.Equal(list03[0].rownum, 1);
            Assert.Equal(list03[0].Id, 1);
            Assert.Equal(list03[1].rownum, 1);
            Assert.Equal(list03[1].Id, 4);
            Assert.Equal(list03[2].rownum, 1);
            Assert.Equal(list03[2].Id, 5);



            var sql04 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    a.Id,
                    a.Nickname,
                    b.Remark,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .ToSql(a => new TwoTablePartitionBy_UserDto());
            var assertSql04 = @"SELECT a.[Id] as1, a.[rownum] as2, a.[Remark] as3 
FROM ( 
    SELECT a.[Id], a.[Nickname], b.[Remark], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] ) a 
WHERE (a.[rownum] = 1)";
            Assert.Equal(assertSql04, sql04);

            var sel04 = fsql.Select<TwoTablePartitionBy_User, TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .WithTempQuery((a, b) => new
                {
                    a.Id,
                    a.Nickname,
                    b.Remark,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1);
            Assert.Equal(assertSql04, sel04.ToSql(a => new TwoTablePartitionBy_UserDto()));

            var list04 = sel04.ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(3, list04.Count);
            Assert.Equal(list04[0].rownum, 1);
            Assert.Equal(list04[0].Id, 1);
            Assert.Equal(list04[0].remark, "remark01");
            Assert.Equal(list04[1].rownum, 1);
            Assert.Equal(list04[1].Id, 4);
            Assert.Equal(list04[1].remark, "remark04");
            Assert.Equal(list04[2].rownum, 1);
            Assert.Equal(list04[2].Id, 5);
            Assert.Equal(list04[2].remark, "remark05");


            var sql05 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .Where(a => a.Nickname == "name03")
                .ToSql(a => new TwoTablePartitionBy_UserDto());
            var assertSql05 = @"SELECT a.[Id] as1 
FROM ( 
    SELECT a.[Id], a.[Nickname] 
    FROM ( 
        SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
        FROM [TwoTablePartitionBy_User] a ) a 
    WHERE (a.[rownum] = 1) ) a 
WHERE (a.[Nickname] = N'name03')";
            Assert.Equal(sql05, assertSql05);
            var list05 = fsql.Select<TwoTablePartitionBy_User>()
                 .WithTempQuery(a => new
                 {
                     user = a,
                     rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                 })
                 .Where(a => a.rownum == 1)
                 .WithTempQuery(a => a.user)
                 .Where(a => a.Nickname == "name03")
                 .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list05.Count, 1);
            Assert.Equal(5, list05[0].Id);
            Assert.Equal(0, list05[0].rownum);
            Assert.Null(list05[0].remark);


            var sql06 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .From<TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Nickname == "name03" || a.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql06 = @"SELECT a.[Id] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname] 
    FROM ( 
        SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
        FROM [TwoTablePartitionBy_User] a ) a 
    WHERE (a.[rownum] = 1) ) a 
INNER JOIN [TwoTablePartitionBy_UserExt] b ON a.[Id] = b.[UserId] 
WHERE ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql06, assertSql06);
            var list06 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .From<TwoTablePartitionBy_UserExt>()
                .InnerJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Nickname == "name03" || a.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list06.Count, 2);
            Assert.Equal(list06[0].rownum, 0);
            Assert.Equal(list06[0].Id, 4);
            Assert.Equal(list06[0].remark, "remark04");
            Assert.Equal(list06[1].rownum, 0);
            Assert.Equal(list06[1].Id, 5);
            Assert.Equal(list06[1].remark, "remark05");


            var sql061 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .From<TwoTablePartitionBy_UserExt>()
                .AsTable((type, old) => type == typeof(TwoTablePartitionBy_UserExt) ? old.Replace("TwoTablePartitionBy_", "") : old)
                .InnerJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Nickname == "name03" || a.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql061 = @"SELECT a.[Id] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname] 
    FROM ( 
        SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
        FROM [TwoTablePartitionBy_User] a ) a 
    WHERE (a.[rownum] = 1) ) a 
INNER JOIN [UserExt] b ON a.[Id] = b.[UserId] 
WHERE ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql061, assertSql061);


            var sql07 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>())
                .InnerJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Nickname == "name03" || a.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql07 = @"SELECT a.[Id] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname] 
    FROM ( 
        SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
        FROM [TwoTablePartitionBy_User] a ) a 
    WHERE (a.[rownum] = 1) ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [TwoTablePartitionBy_UserExt] a) b ON a.[Id] = b.[UserId] 
WHERE ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql07, assertSql07);
            var list07 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .WithTempQuery(a => a.user)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>())
                .InnerJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Nickname == "name03" || a.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list07.Count, 2);
            Assert.Equal(list07[0].rownum, 0);
            Assert.Equal(list07[0].Id, 4);
            Assert.Equal(list07[0].remark, "remark04");
            Assert.Equal(list07[1].rownum, 0);
            Assert.Equal(list07[1].Id, 5);
            Assert.Equal(list07[1].remark, "remark05");


            var sql08 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql08 = @"SELECT a.[rownum] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0)) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql08, assertSql08);
            var list08 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list08.Count, 2);
            Assert.Equal(list08[0].rownum, 1);
            Assert.Equal(list08[0].Id, 0);
            Assert.Equal(list08[0].remark, "remark04");
            Assert.Equal(list08[1].rownum, 1);
            Assert.Equal(list08[1].Id, 0);
            Assert.Equal(list08[1].remark, "remark05");


            var sql09 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).WithTempQuery(b => new { b.UserId, b.Remark }))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql09 = @"SELECT a.[rownum] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) ) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql09, assertSql09);
            var list09 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).WithTempQuery(b => new { b.UserId, b.Remark }))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list09.Count, 2);
            Assert.Equal(list09[0].rownum, 1);
            Assert.Equal(list09[0].Id, 0);
            Assert.Equal(list09[0].remark, "remark04");
            Assert.Equal(list09[1].rownum, 1);
            Assert.Equal(list09[1].Id, 0);
            Assert.Equal(list09[1].remark, "remark05");


            var sql091 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => new { b.UserId, b.Remark }).WithTempQuery(b => b.Key))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql091 = @"SELECT a.[rownum] as1, b.[Remark] as2 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) 
    GROUP BY a.[UserId], a.[Remark] ) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql091, assertSql091);
            var list091 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => new { b.UserId, b.Remark }).WithTempQuery(b => b.Key))
                .InnerJoin((a, b) => a.user.Id == b.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list091.Count, 2);
            Assert.Equal(list091[0].rownum, 1);
            Assert.Equal(list091[0].Id, 0);
            Assert.Equal(list091[0].remark, "remark04");
            Assert.Equal(list091[1].rownum, 1);
            Assert.Equal(list091[1].Id, 0);
            Assert.Equal(list091[1].remark, "remark05");


            var sql10 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => new { b.UserId, b.Remark }).WithTempQuery(b => new { b.Key, rownum = b.Sum(b.Value.UserId) }))
                .InnerJoin((a, b) => a.user.Id == b.Key.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql10 = @"SELECT a.[rownum] as1 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark], sum(a.[UserId]) [rownum] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) 
    GROUP BY a.[UserId], a.[Remark] ) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql10, assertSql10);
            var list10 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => new { b.UserId, b.Remark }).WithTempQuery(b => new { b.Key, rownum = b.Sum(b.Value.UserId) }))
                .InnerJoin((a, b) => a.user.Id == b.Key.UserId)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list10.Count, 2);
            Assert.Equal(list10[0].rownum, 1);
            Assert.Equal(list10[0].Id, 0);
            Assert.Null(list10[0].remark);
            Assert.Equal(list10[1].rownum, 1);
            Assert.Equal(list10[1].Id, 0);
            Assert.Null(list10[1].remark);


            var sql11 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => b.UserId).WithTempQuery(b => new { uid = b.Key, rownum = b.Sum(b.Value.UserId) }))
                .InnerJoin((a, b) => a.user.Id == b.uid)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToSql((a, b) => new TwoTablePartitionBy_UserDto());
            var assertSql11 = @"SELECT a.[rownum] as1 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId] [uid], sum(a.[UserId]) [rownum] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) 
    GROUP BY a.[UserId] ) b ON a.[Id] = b.[uid] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name03' OR a.[Nickname] = N'name02'))";
            Assert.Equal(sql11, assertSql11);
            var list11 = fsql.Select<TwoTablePartitionBy_User>()
                .WithTempQuery(a => new
                {
                    user = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0).GroupBy(b => b.UserId).WithTempQuery(b => new { uid = b.Key, rownum = b.Sum(b.Value.UserId) }))
                .InnerJoin((a, b) => a.user.Id == b.uid)
                .Where((a, b) => a.user.Nickname == "name03" || a.user.Nickname == "name02")
                .ToList<TwoTablePartitionBy_UserDto>();
            Assert.Equal(list11.Count, 2);
            Assert.Equal(list11[0].rownum, 1);
            Assert.Equal(list11[0].Id, 0);
            Assert.Null(list11[0].remark);
            Assert.Equal(list11[1].rownum, 1);
            Assert.Equal(list11[1].Id, 0);
            Assert.Null(list11[1].remark);


            var sql12 = fsql.Select<TwoTablePartitionBy_User>()
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>()
                    .Where(b => b.UserId > 0))
                .LeftJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Id > 0 && b.UserId > 0)
                .GroupBy((a, b) => new { a.Nickname })
                .ToSql(g => new
                {
                    g.Key,
                    sum1 = g.Sum(g.Value.Item1.Id),
                    sum2 = g.Sum(g.Value.Item2.UserId),
                });
            var assertSql12 = @"SELECT a.[Nickname], sum(a.[Id]) as1, sum(b.[UserId]) as2 
FROM [TwoTablePartitionBy_User] a 
LEFT JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0)) b ON a.[Id] = b.[UserId] 
WHERE (a.[Id] > 0 AND b.[UserId] > 0) 
GROUP BY a.[Nickname]";
            Assert.Equal(sql12, assertSql12);
            var list12 = fsql.Select<TwoTablePartitionBy_User>()
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>()
                    .Where(b => b.UserId > 0))
                .LeftJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Id > 0 && b.UserId > 0)
                .GroupBy((a, b) => new { a.Nickname })
                .ToList(g => new
                {
                    g.Key,
                    sum1 = g.Sum(g.Value.Item1.Id),
                    sum2 = g.Sum(g.Value.Item2.UserId),
                });
            Assert.Equal(list12.Count, 3);
            Assert.Equal("name01", list12[0].Key.Nickname);
            Assert.Equal(6, list12[0].sum1);
            Assert.Equal(6, list12[0].sum2);
            Assert.Equal("name02", list12[1].Key.Nickname);
            Assert.Equal(4, list12[1].sum1);
            Assert.Equal(4, list12[1].sum2);
            Assert.Equal("name03", list12[2].Key.Nickname);
            Assert.Equal(11, list12[2].sum1);
            Assert.Equal(11, list12[2].sum2);


            var sql13 = fsql.Select<TwoTablePartitionBy_User>().AsTable((_, old) => old.Replace("TwoTablePartitionBy_", ""))
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().AsTable((_, old) => old.Replace("TwoTablePartitionBy_", ""))
                    .Where(b => b.UserId > 0))
                .LeftJoin((a, b) => a.Id == b.UserId)
                .Where((a, b) => a.Id > 0 && b.UserId > 0)
                .GroupBy((a, b) => new { a.Nickname })
                .ToSql(g => new
                {
                    g.Key,
                    sum1 = g.Sum(g.Value.Item1.Id),
                    sum2 = g.Sum(g.Value.Item2.UserId),
                });
            var assertSql13 = @"SELECT a.[Nickname], sum(a.[Id]) as1, sum(b.[UserId]) as2 
FROM [User] a 
LEFT JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [UserExt] a 
    WHERE (a.[UserId] > 0)) b ON a.[Id] = b.[UserId] 
WHERE (a.[Id] > 0 AND b.[UserId] > 0) 
GROUP BY a.[Nickname]";
            Assert.Equal(sql13, assertSql13);


            var sql14 = fsql.Select<TwoTablePartitionBy_User>()
                .Where(a => a.Id > 0)
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0))
                .InnerJoin((a, b) => a.item.Id == b.UserId)
                .ToSql((a, b) => new
                {
                    user = a.item,
                    rownum = a.rownum,
                    userext = b
                });
            var assertSql14 = @"SELECT a.[Id] as1, a.[Nickname] as2, a.[rownum] as3, b.[UserId] as4, b.[Remark] as5 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a 
    WHERE (a.[Id] > 0) ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0)) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1)";
            Assert.Equal(sql14, assertSql14);
            var list14 = fsql.Select<TwoTablePartitionBy_User>()
                .Where(a => a.Id > 0)
                .WithTempQuery(a => new
                {
                    item = a,
                    rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                })
                .Where(a => a.rownum == 1)
                .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0))
                .InnerJoin((a, b) => a.item.Id == b.UserId)
                .ToList((a, b) => new
                {
                    user = a.item,
                    rownum = a.rownum,
                    userext = b
                });
            Assert.Equal(list14.Count, 3);
            Assert.Equal(list14[0].rownum, 1);
            Assert.Equal(list14[0].user.Id, 1);
            Assert.Equal(list14[0].user.Nickname, "name01");
            Assert.Equal(list14[0].userext.UserId, 1);
            Assert.Equal(list14[0].userext.Remark, "remark01");
            Assert.Equal(list14[1].rownum, 1);
            Assert.Equal(list14[1].user.Id, 4);
            Assert.Equal(list14[1].user.Nickname, "name02");
            Assert.Equal(list14[1].userext.UserId, 4);
            Assert.Equal(list14[1].userext.Remark, "remark04");
            Assert.Equal(list14[2].rownum, 1);
            Assert.Equal(list14[2].user.Id, 5);
            Assert.Equal(list14[2].user.Nickname, "name03");
            Assert.Equal(list14[2].userext.UserId, 5);
            Assert.Equal(list14[2].userext.Remark, "remark05");


            var sql15 = fsql.Select<TwoTablePartitionBy_User>()
                 .WithTempQuery(a => new
                 {
                     user = a,
                     rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                 })
                 .Where(a => a.rownum == 1)
                 .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0)
                     .GroupBy(b => new { b.UserId, b.Remark })
                     .WithTempQuery(b => new { b.Key, sum1 = b.Sum(b.Value.UserId) }))
                 .InnerJoin((a, b) => a.user.Id == b.Key.UserId)
                 .Where((a, b) => a.user.Nickname == "name02" || a.user.Nickname == "name03")
                 .ToSql((a, b) => new
                 {
                     user = a.user,
                     rownum = a.rownum,
                     groupby = b
                 }, FieldAliasOptions.AsProperty);
            var assertSql15 = @"SELECT a.[Id], a.[Nickname], a.[rownum], b.[UserId], b.[Remark], b.[sum1] 
FROM ( 
    SELECT a.[Id], a.[Nickname], row_number() over( partition by a.[Nickname] order by a.[Id]) [rownum] 
    FROM [TwoTablePartitionBy_User] a ) a 
INNER JOIN ( 
    SELECT a.[UserId], a.[Remark], sum(a.[UserId]) [sum1] 
    FROM [TwoTablePartitionBy_UserExt] a 
    WHERE (a.[UserId] > 0) 
    GROUP BY a.[UserId], a.[Remark] ) b ON a.[Id] = b.[UserId] 
WHERE (a.[rownum] = 1) AND ((a.[Nickname] = N'name02' OR a.[Nickname] = N'name03'))";
            Assert.Equal(sql15, assertSql15);
            var list15 = fsql.Select<TwoTablePartitionBy_User>()
                 .WithTempQuery(a => new
                 {
                     user = a,
                     rownum = SqlExt.RowNumber().Over().PartitionBy(a.Nickname).OrderBy(a.Id).ToValue()
                 })
                 .Where(a => a.rownum == 1)
                 .FromQuery(fsql.Select<TwoTablePartitionBy_UserExt>().Where(b => b.UserId > 0)
                     .GroupBy(b => new { b.UserId, b.Remark })
                     .WithTempQuery(b => new { b.Key, sum1 = b.Sum(b.Value.UserId) }))
                 .InnerJoin((a, b) => a.user.Id == b.Key.UserId)
                 .Where((a, b) => a.user.Nickname == "name02" || a.user.Nickname == "name03")
                 .ToList((a, b) => new
                 {
                     user = a.user,
                     rownum = a.rownum,
                     groupby = b
                 });
            Assert.Equal(list15.Count, 2);
            Assert.Equal("remark04", list15[0].groupby.Key.Remark);
            Assert.Equal(4, list15[0].groupby.Key.UserId);
            Assert.Equal(4, list15[0].groupby.sum1);
            Assert.Equal(1, list15[0].rownum);
            Assert.Equal(4, list15[0].user.Id);
            Assert.Equal("name02", list15[0].user.Nickname);
            Assert.Equal("remark05", list15[1].groupby.Key.Remark);
            Assert.Equal(5, list15[1].groupby.Key.UserId);
            Assert.Equal(5, list15[1].groupby.sum1);
            Assert.Equal(1, list15[1].rownum);
            Assert.Equal(5, list15[1].user.Id);
            Assert.Equal("name03", list15[1].user.Nickname);
        }
        class TwoTablePartitionBy_User
        {
            public int Id { get; set; }
            public string Nickname { get; set; }
        }
        class TwoTablePartitionBy_UserExt
        {
            public int UserId { get; set; }
            public string Remark { get; set; }
        }
        class TwoTablePartitionBy_UserDto
        {
            public int Id { get; set; }
            public int rownum { get; set; }
            public string remark { get; set; }
        }
    }
}
