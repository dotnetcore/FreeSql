using FreeSql.DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class RazorModel {
	public RazorModel(IFreeSql fsql, string nameSpace, bool[] NameOptions, List<DbTableInfo> tables, DbTableInfo table) {
		this.fsql = fsql;
        this.NameSpace = nameSpace;
        this.NameOptions = NameOptions;
        this.tables = tables;
		this.table = table;
	}

	public IFreeSql fsql { get; set; }
	public bool[] NameOptions { get; set; }
	public  List<DbTableInfo> tables { get; set; }
	public DbTableInfo table { get; set; }
	public List<DbColumnInfo> columns => this.table.Columns;
	public string NameSpace { get; set; }
    public string FullTableName => $"{(new[] { "public", "dbo" }.Contains(table.Schema) ? "" : table.Schema)}.{table.Name}".TrimStart('.');

	public string GetCsName(string name) {
		name = Regex.Replace(name.TrimStart('@', '.'), @"[^\w]", "_");
		name = char.IsLetter(name, 0) ? name : string.Concat("_", name);
		if (NameOptions[0]) name = UFString(name);
		if (NameOptions[1]) name = UFString(name.ToLower());
		if (NameOptions[2]) name = name.ToLower();
		if (NameOptions[3]) name = string.Join("", name.Split('_').Select(a => UFString(a)));
		return name;
	}
	public string UFString(string text) {
		text = Regex.Replace(text, @"[^\w]", "_");
		if (text.Length <= 1) return text.ToUpper();
		else return text.Substring(0, 1).ToUpper() + text.Substring(1, text.Length - 1);
	}
	public string LFString(string text) {
		text = Regex.Replace(text, @"[^\w]", "_");
		if (text.Length <= 1) return text.ToLower();
		else return text.Substring(0, 1).ToLower() + text.Substring(1, text.Length - 1);
	}

	public string GetCsType(DbColumnInfo col) {
		if (fsql.Ado.DataType == FreeSql.DataType.MySql)
			if (col.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Enum || col.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Set)
				return $"{this.GetCsName(this.FullTableName)}{this.GetCsName(col.Name).ToUpper()}{(col.IsNullable ? "?" : "")}";
		return fsql.DbFirst.GetCsType(col);
	}

	#region 特性
	public string GetTableAttribute() {
		var sb = new List<string>();

		if (GetCsName(this.FullTableName) != this.FullTableName)
		{
			if (this.FullTableName.IndexOf('.') == -1)
				sb.Add("Name = \"" + this.FullTableName + "\"");
			else
				sb.Add("Name = \"" + this.FullTableName + "\""); //Todo: QuoteSqlName
		}

		if (sb.Any() == false) return null;
		return "[Table(" + string.Join(", ", sb) + ")]";
	}
	public string GetColumnAttribute(DbColumnInfo col) {
		var sb = new List<string>();

		if (GetCsName(col.Name) != col.Name)
			sb.Add("Name = \"" + col.Name + "\"");

		if (col.CsType != null)
		{
			var dbinfo = fsql.CodeFirst.GetDbInfo(col.CsType);
			if (dbinfo != null && dbinfo.Value.dbtypeFull.Replace("NOT NULL", "").Trim() != col.DbTypeTextFull)
				sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
			if (col.IsPrimary)
				sb.Add("IsPrimary = true");
			if (col.IsIdentity)
				sb.Add("IsIdentity = true");

			if (dbinfo != null && dbinfo.Value.isnullable != col.IsNullable)
			{
				if (col.IsNullable && fsql.DbFirst.GetCsType(col).Contains("?") == false && col.CsType.IsValueType)
					sb.Add("IsNullable = true");
				if (col.IsNullable == false && fsql.DbFirst.GetCsType(col).Contains("?") == true)
					sb.Add("IsNullable = false");
			}
		}
		if (sb.Any() == false) return null;
		return "[Column(" + string.Join(", ", sb) + ")]";
	}
	#endregion

	#region mysql enum/set
	public string GetMySqlEnumSetDefine() {
		if (fsql.Ado.DataType != FreeSql.DataType.MySql) return null;
		var sb = new StringBuilder();
		foreach (var col in table.Columns) {
			if (col.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Enum || col.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Set) {
				if (col.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Set) sb.Append("\r\n\t[Flags]");
				sb.Append($"\r\n\tpublic enum {this.GetCsName(this.FullTableName)}{this.GetCsName(col.Name).ToUpper()}");
				if (col.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Set) sb.Append(" : long");
				sb.Append(" {\r\n\t\t");

				string slkdgjlksdjg = "";
				int field_idx = 0;
				int unknow_idx = 0;
				string exp2 = string.Concat(col.DbTypeTextFull);
				int quote_pos = -1;
				while (true) {
					int first_pos = quote_pos = exp2.IndexOf('\'', quote_pos + 1);
					if (quote_pos == -1) break;
					while (true) {
						quote_pos = exp2.IndexOf('\'', quote_pos + 1);
						if (quote_pos == -1) break;
						int r_cout = 0;
						//for (int p = 1; true; p++) {
						//	if (exp2[quote_pos - p] == '\\') r_cout++;
						//	else break;
						//}
						while (exp2[++quote_pos] == '\'') r_cout++;
						if (r_cout % 2 == 0/* && quote_pos - first_pos > 2*/) {
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
							if (col.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Set)
								slkdgjlksdjg += " = " + Math.Pow(2, field_idx++);
							if (col.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Enum && field_idx++ == 0)
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


