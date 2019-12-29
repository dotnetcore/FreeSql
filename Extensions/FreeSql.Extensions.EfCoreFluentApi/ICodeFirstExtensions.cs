using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FreeSql.DataAnnotations;

namespace FreeSql.Extensions.EfCoreFluentApi
{
    public static class ICodeFirstExtensions
    {

        static void Test()
        {
            ICodeFirst cf = null;
            cf.Entity<TestInfo>(eb =>
            {
                eb.Property(b => b.Name).HashColumnType("varchar(50)");
                eb.Property(b => b.FullName).HashColumnType("varchar(60)");

                eb.HasKey(a => a.Id).HasKey(a => new { a.Id, a.Name });
                eb.HasIndex(a => a.Name).IsUnique().HasName("idx_xxx11");
            });
        }
        class TestInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string FullName { get; set; }
            public int DefaultValue { get; set; }
        }

        public static ICodeFirst Entity<T>(this ICodeFirst codeFirst, Action<EfCoreTableFluent<T>> modelBuilder)
        {
            codeFirst.ConfigEntity<T>(tf => modelBuilder(new EfCoreTableFluent<T>(tf)));
            return codeFirst;
        }
    }
}
