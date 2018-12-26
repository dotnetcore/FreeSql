using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql {
	public class FreeSqlBuilder {
		IDistributedCache _cache;
		ILogger _logger;
		DataType _dataType;
		string _masterConnectionString;
		string[] _slaveConnectionString;
		bool _isAutoSyncStructure = false;
		bool _isSyncStructureToLower = false;

		/// <summary>
		/// 使用缓存，不指定默认使用内存
		/// </summary>
		/// <param name="cache">缓存现实</param>
		/// <returns></returns>
		public FreeSqlBuilder UseCache(IDistributedCache cache) {
			_cache = cache;
			return this;
		}

		/// <summary>
		/// 使用日志，不指定默认输出控制台
		/// </summary>
		/// <param name="logger"></param>
		/// <returns></returns>
		public FreeSqlBuilder UseLogger(ILogger logger) {
			_logger = logger;
			return this;
		}
		/// <summary>
		/// 使用连接串
		/// </summary>
		/// <param name="dataType">数据库类型</param>
		/// <param name="connectionString">数据库连接串</param>
		/// <returns></returns>
		public FreeSqlBuilder UseConnectionString(DataType dataType, string connectionString) {
			_dataType = dataType;
			_masterConnectionString = connectionString;
			return this;
		}
		/// <summary>
		/// 使用从数据库，支持多个
		/// </summary>
		/// <param name="slaveConnectionString">从数据库连接串</param>
		/// <returns></returns>
		public FreeSqlBuilder UseSlave(params string[] slaveConnectionString) {
			_slaveConnectionString = slaveConnectionString;
			return this;
		}
		/// <summary>
		/// 【开发环境必备】自动同步实体结构到数据库，程序运行中检查实体表是否存在，然后创建或修改
		/// </summary>
		/// <param name="value">true:运行时检查自动同步结构, false:不同步结构</param>
		/// <returns></returns>
		public FreeSqlBuilder UseAutoSyncStructure(bool value) {
			_isAutoSyncStructure = value;
			return this;
		}
		/// <summary>
		/// 转小写同步结构
		/// </summary>
		/// <param name="value">true:转小写, false:不转</param>
		/// <returns></returns>
		public FreeSqlBuilder UseSyncStructureToLower(bool value) {
			_isSyncStructureToLower = value;
			return this;
		}

		public IFreeSql Build() {
			IFreeSql ret = null;
			switch(_dataType) {
				case DataType.MySql: ret = new MySql.MySqlProvider(_cache, null, _masterConnectionString, _slaveConnectionString, _logger); break;
				case DataType.SqlServer: ret = new SqlServer.SqlServerProvider(_cache, null, _masterConnectionString, _slaveConnectionString, _logger); break;
				case DataType.PostgreSQL: ret = new PostgreSQL.PostgreSQLProvider(_cache, null, _masterConnectionString, _slaveConnectionString, _logger); break;
			}
			if (ret != null) {
				ret.CodeFirst.IsAutoSyncStructure = _isAutoSyncStructure;
				ret.CodeFirst.IsSyncStructureToLower = _isSyncStructureToLower;
			}
			return ret;
		}
	}

	public enum DataType { MySql, SqlServer, PostgreSQL }
}
