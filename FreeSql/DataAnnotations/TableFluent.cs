using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace FreeSql.DataAnnotations {
	public class TableFluent<T> {

		public TableFluent(TableAttribute table) {
			_table = table;
		}

		TableAttribute _table;
		/// <summary>
		/// 数据库表名
		/// </summary>
		public TableFluent<T> Name(string value) {
			_table.Name = value;
			return this;
		}
		/// <summary>
		/// 指定数据库旧的表名，修改实体命名时，同时设置此参数为修改之前的值，CodeFirst才可以正确修改数据库表；否则将视为【创建新表】
		/// </summary>
		public TableFluent<T> OldName(string value) {
			_table.OldName = value;
			return this;
		}
		/// <summary>
		/// 查询过滤SQL，实现类似 a.IsDeleted = 1 功能
		/// </summary>
		public TableFluent<T> SelectFilter(string value) {
			_table.SelectFilter = value;
			return this;
		}

		public ColumnFluent<TProto> Property<TProto>(Expression<Func<T, TProto>> column) {
			var proto = (column.Body as MemberExpression)?.Member;
			if (proto == null) throw new FormatException($"错误的表达式格式 {column}");
			var col = _table._columns.GetOrAdd(proto.Name, name => new ColumnAttribute { Name = proto.Name });
			return new ColumnFluent<TProto>(col);
		}
	}
}
