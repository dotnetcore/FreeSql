using FreeSql.DataAnnotations;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;

namespace FreeSql.Extensions.ZeroEntity
{
	public class TableDescriptor
	{
		public string Name { get; set; }
		public string DbName { get; set; }
		public string AsTable { get; set; }
		public bool DisableSyncStructure { get; set; }
		public string Comment { get; set; }
		public List<ColumnDescriptor> Columns { get; } = new List<ColumnDescriptor>();
		public List<NavigateDescriptor> Navigates { get; } = new List<NavigateDescriptor>();
		public List<IndexDescriptor> Indexes { get; } = new List<IndexDescriptor>();

		public class ColumnDescriptor
		{
			public string Name { get; set; }
			public string DbType { get; set; }
			bool? _IsPrimary, _IsIdentity, _IsNullable, _IsVersion;
			public bool IsPrimary { get => _IsPrimary ?? false; set => _IsPrimary = value; }
			public bool IsIdentity { get => _IsIdentity ?? false; set => _IsIdentity = value; }
			public bool IsNullable { get => _IsNullable ?? false; set => _IsNullable = value; }
			public bool IsVersion { get => _IsVersion ?? false; set => _IsVersion = value; }
			public Type MapType { get; set; }
			public DateTimeKind ServerTime { get; set; }
			public string InsertValueSql { get; set; }
			int? _StringLength;
			public int StringLength { get => _StringLength ?? 0; set => _StringLength = value; }
			int? _Precision;
			public int Precision { get => _Precision ?? 0; set => _Precision = value; }
			int? _Scale;
			public int Scale { get => _Scale ?? 0; set => _Scale = value; }
			public string Comment { get; set; }

			public ColumnAttribute ToAttribute()
			{
				var attr = new ColumnAttribute
				{
					Name = Name,
					DbType = DbType,
					MapType = MapType,
					ServerTime = ServerTime,
					InsertValueSql = InsertValueSql,
				};
				if (_IsPrimary != null) attr.IsPrimary = IsPrimary;
				if (_IsIdentity != null) attr.IsIdentity = IsIdentity;
				if (_IsNullable != null) attr.IsNullable = IsNullable;
				if (_IsVersion != null) attr.IsVersion = IsVersion;
				if (_StringLength != null) attr.StringLength = StringLength;
				if (_Precision != null) attr.Precision = Precision;
				if (_Scale != null) attr.Scale = Scale;
				return attr;
			}
		}
		public class IndexDescriptor
		{
			public string Name { get; set; }
			public string Fields { get; set; }
			public bool IsUnique { get; set; }
			public IndexMethod IndexMethod { get; set; }
		}
		public class NavigateDescriptor
		{
			public string Name { get; set; }
			public NavigateType Type { get; set; }
			public string RelTable { get; set; }
			public string Bind { get; set; }
			public string ManyToMany { get; set; }
		}
		public enum NavigateType
		{
			OneToOne, ManyToOne, OneToMany, ManyToMany
		}
	}


	class ZeroTableRef
	{
		internal string NavigateKey { get; set; }
		public TableRefType RefType { get; set; }
		internal ZeroTableInfo Table { get; set; }
		internal ZeroTableInfo RefTable { get; set; }
		internal ZeroTableInfo RefMiddleTable { get; set; }

		public List<string> Columns { get; set; } = new List<string>();
		public List<string> MiddleColumns { get; set; } = new List<string>();
		public List<string> RefColumns { get; set; } = new List<string>();
	}
	class ZeroTableInfo : TableInfo
	{
		public Dictionary<string, ZeroTableRef> Navigates { get; set; } = new Dictionary<string, ZeroTableRef>(StringComparer.OrdinalIgnoreCase);
	}
}
