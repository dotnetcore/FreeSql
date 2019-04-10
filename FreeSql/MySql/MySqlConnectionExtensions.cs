using FreeSql.MySql;
using FreeSql.MySql.Curd;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace FreeSql {
	public static class MySqlConnectionExtensions {

		static IFreeSql<int> _curd = new MySqlProvider<int>(null, new LoggerFactory().CreateLogger("MySqlConnectionExtensions"), null, null);

		public static ISelect<T1> Select<T1>(this MySqlConnection that) where T1 : class => _curd.Select<T1>().WithConnection(that);
		public static ISelect<T1> Select<T1>(this MySqlConnection that, object dywhere) where T1 : class => _curd.Select<T1>(dywhere).WithConnection(that);
		public static IInsert<T1> Insert<T1>(this MySqlConnection that) where T1 : class => _curd.Insert<T1>().WithConnection(that);
		public static IInsert<T1> Insert<T1>(this MySqlConnection that, T1 source) where T1 : class => _curd.Insert<T1>().WithConnection(that).AppendData(source);
		public static IInsert<T1> Insert<T1>(this MySqlConnection that, T1[] source) where T1 : class => _curd.Insert<T1>().WithConnection(that).AppendData(source);
		public static IInsert<T1> Insert<T1>(this MySqlConnection that, IEnumerable<T1> source) where T1 : class => _curd.Insert<T1>().WithConnection(that).AppendData(source);
		public static IUpdate<T1> Update<T1>(this MySqlConnection that) where T1 : class => _curd.Update<T1>().WithConnection(that);
		public static IUpdate<T1> Update<T1>(this MySqlConnection that, object dywhere) where T1 : class => _curd.Update<T1>(dywhere).WithConnection(that);
		public static IDelete<T1> Delete<T1>(this MySqlConnection that) where T1 : class => _curd.Delete<T1>().WithConnection(that);
		public static IDelete<T1> Delete<T1>(this MySqlConnection that, object dywhere) where T1 : class => _curd.Delete<T1>(dywhere).WithConnection(that);

		public static List<T> Query<T>(this MySqlConnection that, string cmdText, object parms = null) => _curd.Ado.Query<T>(that, cmdText, parms);
		public static List<T> Query<T>(this MySqlConnection that, CommandType cmdType, string cmdText, params MySqlParameter[] cmdParms) => _curd.Ado.Query<T>(that, cmdText, cmdParms);
	}
}
