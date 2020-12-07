using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
	public class _595
	{
		[Fact]
		public void BatchInsert()
		{
            var fsql = g.oracle;
			var input = JsonConvert.DeserializeObject<TXL_JXSDH[]>(File.ReadAllText(@"d:/qq/json.txt"));

            using (var uowm = new UnitOfWorkManager(fsql))
            {
				using (var uow = uowm.Begin())
                {
					try
					{
						var _TXL_JXSDHRepository = fsql.GetRepository<TXL_JXSDH>();
						_TXL_JXSDHRepository.UnitOfWork = uow;

						//先删除所有数据再导入
						var cs = _TXL_JXSDHRepository.Orm.Delete<TXL_JXSDH>().Where("1=1").ExecuteAffrows();
						//接着批量插入数据
						var cs1 = _TXL_JXSDHRepository.Insert(input);

						uow.Commit();
					}
                    catch
                    {
						uow.Rollback();
						throw;
                    }
				}
            }
        }

		/// <summary>
		/// 导入单位信息
		/// </summary>
		[Table(Name = "TXL_JXSDH")]
		public partial class TXL_JXSDH
		{

			/// <summary>
			/// 主键
			/// </summary>

			[JsonProperty, Column(StringLength = 50, IsPrimary = true)]
			public string DHID { get; set; }



			/// <summary>
			/// 单位编码
			/// </summary>

			[JsonProperty, Column(StringLength = 50)]
			public string CCODE { get; set; }

			/// <summary>
			/// 单位名称
			/// </summary>

			[JsonProperty, Column(StringLength = 50)]
			public string CNAME { get; set; }

			/// <summary>
			/// 办公电话
			/// </summary>

			[JsonProperty, Column(StringLength = 50)]
			public string BGDH { get; set; }

			/// <summary>
			/// 工作店面
			/// </summary>

			[JsonProperty, Column(StringLength = 50)]
			public string GZDM { get; set; }

			/// <summary>
			/// 员工姓名
			/// </summary>

			[JsonProperty, Column(StringLength = 50)]
			public string YGXM { get; set; }

			/// <summary>
			/// 岗位名称
			/// </summary>

			[JsonProperty, Column(StringLength = 50)]
			public string GWMC { get; set; }

			/// <summary>
			/// 联系电话
			/// </summary>

			[JsonProperty, Column(StringLength = 50)]
			public string SJHM { get; set; }

			/// <summary>
			/// 公司地址
			/// </summary>

			[JsonProperty, Column(DbType = "VARCHAR2(200 BYTE)")]
			public string GSDZ { get; set; }





			/// <summary>
			/// 备注
			/// </summary>

			[JsonProperty, Column(StringLength = 500)]
			public string REMARK { get; set; }

			/// <summary>
			/// 公司所在省份
			/// </summary>
			[JsonProperty, Column(DbType = "VARCHAR2(50 BYTE)")]
			public string SF { get; set; }

			/// <summary>
			/// 是否新能源，0是1否
			/// </summary>
			[JsonProperty, Column(DbType = "NUMBER(1)", CanInsert = false)]
			public decimal? SFXNY { get; set; } = 1M;

			/// <summary>
			/// 公司所在市
			/// </summary>
			[JsonProperty, Column(DbType = "VARCHAR2(50 BYTE)")]
			public string SHI { get; set; }



			/// <summary>
			/// 所属大区
			/// </summary>
			[JsonProperty, Column(DbType = "VARCHAR2(50 BYTE)")]
			public string SSDQ { get; set; }

			/// <summary>
			/// 所属经理部
			/// </summary>
			[JsonProperty, Column(DbType = "VARCHAR2(50 BYTE)")]
			public string SSJLB { get; set; }







			/// <summary>
			/// 区域经理名字
			/// </summary>
			[JsonProperty, Column(DbType = "VARCHAR2(50 BYTE)")]
			public string YXJLMZ { get; set; }

			/// <summary>
			/// 区域经理手机
			/// </summary>
			[JsonProperty, Column(DbType = "VARCHAR2(50 BYTE)")]
			public string YXJLSJ { get; set; }

			/// <summary>
			/// 区域经理账号
			/// </summary>
			[JsonProperty, Column(DbType = "VARCHAR2(50 BYTE)")]
			public string YXJLZH { get; set; }





			/// <summary>
			/// 创建日期
			/// </summary>
			[JsonProperty, Column(DbType = "DATE", InsertValueSql = "sysdate", CanInsert = false)]
			public DateTime? CREATEDATE { get; set; }

			/// <summary>
			/// 创建人账号
			/// </summary>
			[JsonProperty, Column(DbType = "VARCHAR2(30 BYTE)")]
			public string CREATELOGINNAME { get; set; }

			/// <summary>
			/// 创建人
			/// </summary>
			[JsonProperty, Column(DbType = "VARCHAR2(30 BYTE)")]
			public string CREATER { get; set; }


			/// <summary>
			/// 修改日期
			/// </summary>
			[JsonProperty, Column(DbType = "DATE", CanInsert = false, ServerTime = DateTimeKind.Local)]
			public DateTime? UPDATEDATE { get; set; }

			/// <summary>
			/// 修改人账号
			/// </summary>
			[JsonProperty, Column(DbType = "VARCHAR2(30 BYTE)")]
			public string UPDATELOGINNAME { get; set; }

			/// <summary>
			/// 修改人
			/// </summary>
			[JsonProperty, Column(DbType = "VARCHAR2(30 BYTE)")]
			public string UPDATER { get; set; }
		}
	}
}
