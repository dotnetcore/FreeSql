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
	[ExpressionCall]
	public static class _576Extensions
	{
		public static ThreadLocal<ExpressionCallContext> expContext = new ThreadLocal<ExpressionCallContext>();

		/// <summary>
		/// 自定义表达式树函数解析
		/// </summary>
		/// <param name="that"></param>
		/// <param name="withinCode"></param>
		/// <returns></returns>
		public static string ToNormalWithinCodeGuid([RawValue] this Guid that, string withinCode)
		{
			expContext.Value.Result = $"{expContext.Value.ParsedContent["withinCode"]} || '{that.ToString("N")}'";
			return null;
		}
	}

	public class _576
	{
		[Fact]
		public void InsertInto()
		{
			IFreeSql fsql = g.oracle;


			fsql.Delete<SysRole>().Where("1=1").ExecuteAffrows();
			var id = Guid.NewGuid().ToString();
			fsql.Insert(new SysRole { Guid = id, RoleName = "role1", Sort = 1 }).ExecuteAffrows();

			Assert.Equal(1, fsql.Select<SysRole>().Where(a => a.Guid == id).InsertInto("", a => new SysRole
			{
				Guid = "'x123123dasfafd'",
				RoleName = Guid.NewGuid().ToNormalWithinCodeGuid(a.RoleName),
				Sort = a.Sort
			}));

			var item = fsql.Select<SysRole>().Where(a => a.Guid == "x123123dasfafd").First();
			Assert.NotNull(item);
			Assert.True(item.RoleName.StartsWith("role1") && item.RoleName.Length == 37);
			Assert.Equal(item.Sort, 1);
		}

		[Table(Name = "issues_576_SysRole")]
		public partial class SysRole
		{

			/// <summary>
			/// GUID
			/// </summary>
			[JsonProperty, Column(Name = "GUID", DbType = "VARCHAR2(60 BYTE)", IsPrimary = true)]
			public string Guid { get; set; }

			/// <summary>
			/// 角色名称
			/// </summary>
			[JsonProperty, Column(Name = "ROLE_NAME", DbType = "NVARCHAR2(40)")]
			public string RoleName { get; set; }

			/// <summary>
			/// 角色排序
			/// </summary>
			[JsonProperty, Column(Name = "SORT")]
			public int Sort { get; set; }
		}
	}
}
