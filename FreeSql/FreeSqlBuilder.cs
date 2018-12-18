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

		public IFreeSql Build() {
			switch(_dataType) {
				case DataType.MySql: return new MySql.MySqlProvider(_cache, null, _masterConnectionString, _slaveConnectionString, _logger);
				case DataType.SqlServer: return new SqlServer.SqlServerProvider(_cache, null, _masterConnectionString, _slaveConnectionString, _logger);
				case DataType.PostgreSQL: return new MySql.MySqlProvider(_cache, null, _masterConnectionString, _slaveConnectionString, _logger);
			}
			return null;
		}
	}

	public enum DataType { MySql, SqlServer, PostgreSQL }
}
