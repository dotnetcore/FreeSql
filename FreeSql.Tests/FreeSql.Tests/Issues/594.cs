using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
	public class _594
	{
		[Fact]
		public void Decimal()
		{
            var fsql = g.sqlserver;

            fsql.Delete<ts_decimal_18_4>().Where("1=1").ExecuteAffrows();
            var id = Guid.NewGuid();

            Assert.Equal(1, fsql.Insert(new ts_decimal_18_4 { id = id }).ExecuteAffrows());
            Assert.Equal(1, fsql.Update<ts_decimal_18_4>(id)
                .Set(a => a.price, 698830024.6700m)
                .ExecuteAffrows());

            Assert.Equal(1, fsql.Update<ts_decimal_18_4>()
                .SetSource(new ts_decimal_18_4 { id = id, price = 698830024.6700m })
                .ExecuteAffrows());

            Assert.Equal(1, fsql.Update<ts_decimal_18_4>(id).NoneParameter()
                .Set(a => a.price, 698830024.6700m)
                .ExecuteAffrows());

            Assert.Equal(1, fsql.Update<ts_decimal_18_4>().NoneParameter()
                .SetSource(new ts_decimal_18_4 { id = id, price = 698830024.6700m })
                .ExecuteAffrows());
        }

        class ts_decimal_18_4
        {
            public Guid id { get; set; }
            [Column(Precision = 18, Scale = 4)]
            public decimal price { get; set; }
        }
    }
}
