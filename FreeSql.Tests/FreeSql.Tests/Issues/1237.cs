using FreeSql.DataAnnotations;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _1237
    {
        [Fact]
        public void WithTempQuery()
        {
            var fsql = g.sqlite;
            var people1 = fsql.Select<people>()
                .GroupBy(x => x.Name.Replace(" ", "").Replace("　", ""))
                .Having(x => x.Count() > 1)
                .WithTempQuery(x => new { xm = x.Key })
                .From<people>()
                .InnerJoin((a, b) => a.xm == b.Name.Replace(" ", "").Replace("　", ""))
                .OrderBy((a, b) => b.Name)
                .OrderBy((a, b) => b.ID)
                .ToSql();
            Assert.Equal(@"SELECT * 
FROM ( 
    SELECT replace(replace(a.""Name"", ' ', ''), '　', '') ""xm"" 
    FROM ""people_issues_1237"" a 
    GROUP BY replace(replace(a.""Name"", ' ', ''), '　', '') 
    HAVING (count(1) > 1) ) a 
INNER JOIN ""people_issues_1237"" b ON a.""xm"" = replace(replace(b.""Name"", ' ', ''), '　', '') 
ORDER BY b.""Name"", b.""ID""", people1);

            var people2 = fsql.Select<people>()
                .GroupBy(x => new { xm_new = x.Name.Replace(" ", "").Replace("　", ""), csny = x.CSNY })
                .Having(x => x.Count() > 1).WithTempQuery(x => new { xm = x.Key.xm_new, csny = x.Key.csny })
                .From<people>()
                .InnerJoin((a, b) => a.xm == b.Name.Replace(" ", "").Replace("　", "") && a.csny == b.CSNY)
                .OrderBy((a, b) => b.Name).OrderBy((a, b) => b.ID)
                .ToSql();
            Assert.Equal(@"SELECT * 
FROM ( 
    SELECT replace(replace(a.""Name"", ' ', ''), '　', ''), a.""CSNY"" 
    FROM ""people_issues_1237"" a 
    GROUP BY replace(replace(a.""Name"", ' ', ''), '　', ''), a.""CSNY"" 
    HAVING (count(1) > 1) ) a 
INNER JOIN ""people_issues_1237"" b ON a.""xm_new"" = replace(replace(b.""Name"", ' ', ''), '　', '') AND a.""csny"" = b.""CSNY"" 
ORDER BY b.""Name"", b.""ID""", people2);

        }

        [Table(Name = "people_issues_1237")]
        public partial class people
        {

            [Column(IsPrimary = true, IsIdentity = true)]
            public int ID { get; set; }

            [Column(DbType = "varchar(255)")]
            public string Name { get; set; }

            [Column(DbType = "varchar(255)")]
            public string CSNY { get; set; }

            [Column(DbType = "varchar(255)")]
            public string Sex { get; set; }
        }
    }

}
