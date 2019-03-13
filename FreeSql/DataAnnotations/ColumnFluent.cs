namespace FreeSql.DataAnnotations {
	public class ColumnFluent {

		public ColumnFluent(ColumnAttribute column) {
			_column = column;
		}

		ColumnAttribute _column;
		/// <summary>
		/// 数据库列名
		/// </summary>
		public ColumnFluent Name(string value) {
			_column.Name = value;
			return this;
		}
		/// <summary>
		/// 指定数据库旧的列名，修改实体属性命名时，同时设置此参数为修改之前的值，CodeFirst才可以正确修改数据库字段；否则将视为【新增字段】
		/// </summary>
		public ColumnFluent OldName(string value) {
			_column.OldName = value;
			return this;
		}
		/// <summary>
		/// 数据库类型，如： varchar(255)
		/// </summary>
		public ColumnFluent DbType(string value) {
			_column.DbType = value;
			return this;
		}

		/// <summary>
		/// 主键
		/// </summary>
		public ColumnFluent IsPrimary(bool value) {
			_column.IsPrimary = value;
			return this;
		}
		/// 自增标识
		/// </summary>
		public ColumnFluent IsIdentity(bool value) {
			_column.IsIdentity = value;
			return this;
		}
		/// <summary>
		/// 是否可DBNull
		/// </summary>
		public ColumnFluent IsNullable(bool value) {
			_column.IsNullable = value;
			return this;
		}
		/// <summary>
		/// 忽略此列，不迁移、不插入
		/// </summary>
		public ColumnFluent IsIgnore(bool value) {
			_column.IsIgnore = value;
			return this;
		}
	}
}
