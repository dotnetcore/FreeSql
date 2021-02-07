using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.DatabaseModel;
using FreeSql.Internal.CommonProvider;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class RazorModel
{
	public RazorModel(IFreeSql fsql, string nameSpace, bool[] NameOptions, List<DbTableInfo> tables, DbTableInfo table)
	{
		this.fsql = fsql;
		this.NameSpace = nameSpace;
		this.NameOptions = NameOptions;
		this.tables = tables;
		this.table = table;
	}

	public IFreeSql fsql { get; set; }
	public bool[] NameOptions { get; set; }
	public List<DbTableInfo> tables { get; set; }
	public DbTableInfo table { get; set; }
	public List<DbColumnInfo> columns => this.table.Columns;
	public string NameSpace { get; set; }
	public string FullTableName => $"{(new[] { "public", "dbo" }.Contains(table.Schema) ? "" : table.Schema)}.{table.Name}".TrimStart('.');

	public string GetCsName(string name)
	{
		name = Regex.Replace(name.TrimStart('@', '.'), @"[^\w]", "_");
		name = char.IsLetter(name, 0) ? name : string.Concat("_", name);
		if (NameOptions[0]) name = UFString(name);
		if (NameOptions[1]) name = UFString(name.ToLower());
		if (NameOptions[2]) name = name.ToLower();
		if (NameOptions[3]) name = string.Join("", name.Split('_').Select(a => UFString(a)));
		return name;
	}
	public string UFString(string text)
	{
		text = Regex.Replace(text, @"[^\w]", "_");
		if (text.Length <= 1) return text.ToUpper();
		else return text.Substring(0, 1).ToUpper() + text.Substring(1, text.Length - 1);
	}
	public string LFString(string text)
	{
		text = Regex.Replace(text, @"[^\w]", "_");
		if (text.Length <= 1) return text.ToLower();
		else return text.Substring(0, 1).ToLower() + text.Substring(1, text.Length - 1);
	}

	public string GetCsType(DbColumnInfo col)
	{
		if (fsql.Ado.DataType == FreeSql.DataType.MySql)
			if (col.DbType == (int)MySqlDbType.Enum || col.DbType == (int)MySqlDbType.Set)
				return $"{this.GetCsName(this.FullTableName)}{this.GetCsName(col.Name).ToUpper()}{(col.IsNullable ? "?" : "")}";
		return fsql.DbFirst.GetCsType(col);
	}

	#region 特性
	public string GetTableAttribute()
	{
		var sb = new List<string>();

		if (GetCsName(this.FullTableName) != this.FullTableName)
		{
			if (this.FullTableName.IndexOf('.') == -1)
				sb.Add("Name = \"" + this.FullTableName + "\"");
			else
				sb.Add("Name = \"" + this.FullTableName + "\""); //Todo: QuoteSqlName
		}

		sb.Add("DisableSyncStructure = true");
		if (sb.Any() == false) return null;
		return "[Table(" + string.Join(", ", sb) + ")]";
	}
	public string GetColumnAttribute(DbColumnInfo col, bool isInsertValueSql = false)
	{
		var sb = new List<string>();

		if (GetCsName(col.Name) != col.Name)
			sb.Add("Name = \"" + col.Name + "\"");

		if (col.CsType != null)
		{
			var dbinfo = fsql.CodeFirst.GetDbInfo(col.CsType);
			if (dbinfo != null && string.Compare(dbinfo.dbtypeFull.Replace("NOT NULL", "").Trim(), col.DbTypeTextFull, true) != 0)
			{
				#region StringLength 反向
				switch (fsql.Ado.DataType)
				{
					case DataType.MySql:
					case DataType.OdbcMySql:
						switch (col.DbTypeTextFull.ToLower())
						{
							case "longtext": sb.Add("StringLength = -2"); break;
							case "text": sb.Add("StringLength = -1"); break;
							default:
								var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^varchar\s*\((\w+)\)$", RegexOptions.IgnoreCase);
								if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
								else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
								break;
						}
						break;
					case DataType.SqlServer:
					case DataType.OdbcSqlServer:
						switch (col.DbTypeTextFull.ToLower())
						{
							case "nvarchar(max)": sb.Add("StringLength = -2"); break;
							default:
								var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^nvarchar\s*\((\w+)\)$", RegexOptions.IgnoreCase);
								if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
								else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
								break;
						}
						break;
					case DataType.PostgreSQL:
					case DataType.OdbcPostgreSQL:
					case DataType.KingbaseES:
					case DataType.OdbcKingbaseES:
					case DataType.ShenTong:
						switch (col.DbTypeTextFull.ToLower())
						{
							case "text": sb.Add("StringLength = -2"); break;
							default:
								var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^varchar\s*\((\w+)\)$", RegexOptions.IgnoreCase);
								if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
								else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
								break;
						}
						break;
					case DataType.Oracle:
					case DataType.OdbcOracle:
						switch (col.DbTypeTextFull.ToLower())
						{
							case "nclob": sb.Add("StringLength = -2"); break;
							default:
								var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^nvarchar2\s*\((\w+)\)$", RegexOptions.IgnoreCase);
								if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
								else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
								break;
						}
						break;
					case DataType.Dameng:
					case DataType.OdbcDameng:
						switch (col.DbTypeTextFull.ToLower())
						{
							case "text": sb.Add("StringLength = -2"); break;
							default:
								var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^nvarchar2\s*\((\w+)\)$", RegexOptions.IgnoreCase);
								if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
								else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
								break;
						}
						break;
					case DataType.Sqlite:
						switch (col.DbTypeTextFull.ToLower())
						{
							case "text": sb.Add("StringLength = -2"); break;
							default:
								var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^nvarchar\s*\((\w+)\)$", RegexOptions.IgnoreCase);
								if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
								else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
								break;
						}
						break;
					case DataType.MsAccess:
						switch (col.DbTypeTextFull.ToLower())
						{
							case "longtext": sb.Add("StringLength = -2"); break;
							default:
								var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^varchar\s*\((\w+)\)$", RegexOptions.IgnoreCase);
								if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
								else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
								break;
						}
						break;
				}
				#endregion
			}
			if (col.IsPrimary)
				sb.Add("IsPrimary = true");
			if (col.IsIdentity)
				sb.Add("IsIdentity = true");

			if (dbinfo != null && dbinfo.isnullable != col.IsNullable)
			{
				var cstype = fsql.DbFirst.GetCsType(col);
				if (col.IsNullable && cstype.Contains("?") == false && col.CsType.IsValueType)
					sb.Add("IsNullable = true");
				if (col.IsNullable == false && (cstype.Contains("?") == true || cstype == "string"))
					sb.Add("IsNullable = false");
			}

			if (isInsertValueSql)
			{
				var defval = GetColumnDefaultValue(col, false);
				if (defval == null) //c#默认属性值，就不需要设置 InsertValueSql 了
				{
					defval = GetColumnDefaultValue(col, true);
					if (defval != null)
					{
						sb.Add("InsertValueSql = \"" + defval.Replace("\"", "\\\"") + "\"");
						//sb.Add("CanInsert = false");
					}
				}
				//else
				//sb.Add("CanInsert = false");
			}
		}
		if (sb.Any() == false) return null;
		return "[Column(" + string.Join(", ", sb) + ")]";
	}
	public string GetColumnDefaultValue(DbColumnInfo col, bool isInsertValueSql)
	{
		var defval = col.DefaultValue?.Trim();
		if (string.IsNullOrEmpty(defval)) return null;
		var cstype = col.CsType.NullableTypeOrThis();
		if (fsql.Ado.DataType == DataType.SqlServer || fsql.Ado.DataType == DataType.OdbcSqlServer)
		{
			if (defval.StartsWith("((") && defval.EndsWith("))")) defval = defval.Substring(2, defval.Length - 4);
			else if (defval.StartsWith("('") && defval.EndsWith("')")) defval = defval.Substring(2, defval.Length - 4).Replace("''", "'");
			else if (defval.StartsWith("(") && defval.EndsWith(")")) defval = defval.Substring(1, defval.Length - 2);
			else return null;
			if (defval.StartsWith("N'") && defval.EndsWith("'")) defval = defval.Substring(1);
			if (cstype == typeof(Guid) && string.Compare(defval, "newid()", true) == 0) return $"Guid.NewGuid()";
			if (cstype == typeof(string) && string.Compare(defval, "newid()", true) == 0) return $"Guid.NewGuid().ToString().ToUpper()";
			if (defval == "NULL") return null;
		}
		if ((cstype == typeof(string) && defval.StartsWith("'") && defval.EndsWith("'::character varying") ||
			cstype == typeof(Guid) && defval.StartsWith("'") && defval.EndsWith("'::uuid")
			) && (fsql.Ado.DataType == DataType.PostgreSQL || fsql.Ado.DataType == DataType.OdbcPostgreSQL ||
				fsql.Ado.DataType == DataType.KingbaseES || fsql.Ado.DataType == DataType.OdbcKingbaseES ||
				fsql.Ado.DataType == DataType.ShenTong))
		{
			defval = defval.Substring(1, defval.LastIndexOf("'::") - 1).Replace("''", "'");
		}
		else if (defval.StartsWith("'") && defval.EndsWith("'"))
		{
			defval = defval.Substring(1, defval.Length - 2).Replace("''", "'");
			if (fsql.Ado.DataType == DataType.MySql || fsql.Ado.DataType == DataType.OdbcMySql) defval = defval.Replace("\\\\", "\\");
		}
		if (cstype.IsNumberType() && decimal.TryParse(defval, out var trydec))
		{
			if (isInsertValueSql) return defval;
			if (cstype == typeof(float)) return defval + "f";
			if (cstype == typeof(double)) return defval + "d";
			if (cstype == typeof(decimal)) return defval + "M";
			return defval;
		}
		if (cstype == typeof(Guid) && Guid.TryParse(defval, out var tryguid)) return isInsertValueSql ? (fsql.Select<TestTb>() as Select0Provider)._commonUtils.FormatSql("{0}", defval) : $"Guid.Parse(\"{defval.Replace("\r\n", "\\r\\n").Replace("\"", "\\\"")}\")";
		if (cstype == typeof(DateTime) && DateTime.TryParse(defval, out var trydt)) return isInsertValueSql ? (fsql.Select<TestTb>() as Select0Provider)._commonUtils.FormatSql("{0}", defval) : $"DateTime.Parse(\"{defval.Replace("\r\n", "\\r\\n").Replace("\"", "\\\"")}\")";
		if (cstype == typeof(TimeSpan) && TimeSpan.TryParse(defval, out var tryts)) return isInsertValueSql ? (fsql.Select<TestTb>() as Select0Provider)._commonUtils.FormatSql("{0}", defval) : $"TimeSpan.Parse(\"{defval.Replace("\r\n", "\\r\\n").Replace("\"", "\\\"")}\")";
		if (cstype == typeof(string)) return isInsertValueSql ? (fsql.Select<TestTb>() as Select0Provider)._commonUtils.FormatSql("{0}", defval) : $"\"{defval.Replace("\r\n", "\\r\\n").Replace("\"", "\\\"")}\"";
		if (cstype == typeof(bool)) return isInsertValueSql ? defval : (defval == "1" || defval == "t" ? "true" : "false");
		if (fsql.Ado.DataType == DataType.MySql || fsql.Ado.DataType == DataType.OdbcMySql)
			if (col.DbType == (int)MySqlDbType.Enum || col.DbType == (int)MySqlDbType.Set)
				if (isInsertValueSql) return (fsql.Select<TestTb>() as Select0Provider)._commonUtils.FormatSql("{0}", defval);
		return isInsertValueSql ? defval : null; //sql function or exp
	}
	#endregion

	#region mysql enum/set
	public string GetMySqlEnumSetDefine()
	{
		if (fsql.Ado.DataType != FreeSql.DataType.MySql && fsql.Ado.DataType != FreeSql.DataType.OdbcMySql) return null;
		var sb = new StringBuilder();
		foreach (var col in table.Columns)
		{
			if (col.DbType == (int)MySqlDbType.Enum || col.DbType == (int)MySqlDbType.Set)
			{
				if (col.DbType == (int)MySqlDbType.Set) sb.Append("\r\n\t[Flags]");
				sb.Append($"\r\n\tpublic enum {this.GetCsName(this.FullTableName)}{this.GetCsName(col.Name).ToUpper()}");
				if (col.DbType == (int)MySqlDbType.Set) sb.Append(" : long");
				sb.Append(" {\r\n\t\t");

				string slkdgjlksdjg = "";
				int field_idx = 0;
				int unknow_idx = 0;
				string exp2 = string.Concat(col.DbTypeTextFull);
				int quote_pos = -1;
				while (true)
				{
					int first_pos = quote_pos = exp2.IndexOf('\'', quote_pos + 1);
					if (quote_pos == -1) break;
					while (true)
					{
						quote_pos = exp2.IndexOf('\'', quote_pos + 1);
						if (quote_pos == -1) break;
						int r_cout = 0;
						//for (int p = 1; true; p++) {
						//	if (exp2[quote_pos - p] == '\\') r_cout++;
						//	else break;
						//}
						while (exp2[++quote_pos] == '\'') r_cout++;
						if (r_cout % 2 == 0/* && quote_pos - first_pos > 2*/)
						{
							string str2 = exp2.Substring(first_pos + 1, quote_pos - first_pos - 2).Replace("''", "'");
							if (Regex.IsMatch(str2, @"^[\u0391-\uFFE5a-zA-Z_\$][\u0391-\uFFE5a-zA-Z_\$\d]*$"))
								slkdgjlksdjg += ", " + str2;
							else
								slkdgjlksdjg += string.Format(@", 
/// <summary>
/// {0}
/// </summary>
[Description(""{0}"")]
Unknow{1}", str2.Replace("\"", "\\\""), ++unknow_idx);
							if (col.DbType == (int)MySqlDbType.Set)
								slkdgjlksdjg += " = " + Math.Pow(2, field_idx++);
							if (col.DbType == (int)MySqlDbType.Enum && field_idx++ == 0)
								slkdgjlksdjg += " = 1";
							break;
						}
					}
					if (quote_pos == -1) break;
				}
				sb.Append(slkdgjlksdjg.Substring(2).TrimStart('\r', '\n', '\t'));
				sb.Append("\r\n\t}");
			}
		}
		return sb.ToString();
	}
	#endregion
}

[Table(DisableSyncStructure = true)]
class TestTb { public Guid id { get; set; } }


