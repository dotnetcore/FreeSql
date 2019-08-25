using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace FreeSql.Tests.PostgreSQL
{
    public class PostgreSQLAopTest
    {

        class TestAuditValue
        {
            public Guid id { get; set; }
            [Now]
            public DateTime createtime { get; set; }
        }
        class NowAttribute: Attribute { }

        [Fact]
        public void AuditValue()
        {
            var now = DateTime.Now;
            var item = new TestAuditValue();

            EventHandler<Aop.AuditValueEventArgs> audit = (s, e) =>
             {
                 if (e.Property.GetCustomAttribute<NowAttribute>(false) != null)
                     e.Value = DateTime.Now;
             };
            g.pgsql.Aop.AuditValue += audit;

            g.pgsql.Insert(item).ExecuteAffrows();

            g.pgsql.Aop.AuditValue -= audit;

            Assert.Equal(item.createtime.Date, now.Date);
        }
    }
}
