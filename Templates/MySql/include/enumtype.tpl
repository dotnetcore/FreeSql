{%
var dbf = dbfirst as FreeSql.IDbFirst;
var fks = (table.Foreigns as List<DbForeignInfo>);

Func<string, string> UString = stra => stra.Substring(0, 1).ToUpper() + stra.Substring(1);
Func<DbColumnInfo, string> GetCsType = cola3 => {
	if (cola3.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Enum || cola3.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Set) {
		return $"{UString(cola3.Table.Name)}{cola3.Name.ToUpper()}{(cola3.IsNullable ? "?" : "")}";
	}
	return dbf.GetCsType(cola3);
};
Func<DbForeignInfo, string> GetFkObjectName = fkx => {
	var eqfks = fks.Where(fk22a => fk22a.ReferencedTable.Name == fkx.ReferencedTable.Name);
	if (eqfks.Count() == 1) return "Obj_" + fkx.ReferencedTable.Name;
	var fkretname = fkx.Columns[0].Name;
	if (fkretname.EndsWith(fkx.ReferencedColumns[0].Name, StringComparison.CurrentCultureIgnoreCase)) fkretname = fkretname.Substring(0, fkretname.Length - fkx.ReferencedColumns[0].Name.Length).TrimEnd('_');
	if (fkretname.EndsWith(fkx.ReferencedTable.Name, StringComparison.CurrentCultureIgnoreCase)) fkretname = fkretname.Substring(0, fkretname.Length - fkx.ReferencedTable.Name.Length).TrimEnd('_');
	if (fkretname.StartsWith(fkx.ReferencedTable.Name, StringComparison.CurrentCultureIgnoreCase)) fkretname = fkretname.Substring(fkx.ReferencedTable.Name.Length).TrimStart('_');
	return "Obj_" + fkx.ReferencedTable.Name + (string.IsNullOrEmpty(fkretname) ? "" : ("_" + fkretname));
};


	foreach (var col11 in table.Columns) {
		if (col11.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Enum || col11.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Set) {
			if (col11.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Set) print("\r\n\t[Flags]");
			print($"\r\n\tpublic enum {UString(table.Name)}{col11.Name.ToUpper()}");
			if (col11.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Set) print(" : long");
			print (" {\r\n\t\t");

			string slkdgjlksdjg = "";
			int field_idx = 0;
			int unknow_idx = 0;
			string exp2 = string.Concat(col11.DbTypeTextFull);
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
					while(exp2[++quote_pos] == '\'') r_cout++;
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
						if (col11.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Set)
							slkdgjlksdjg += " = " + Math.Pow(2, field_idx++);
						if (col11.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Enum && field_idx++ == 0)
							slkdgjlksdjg += " = 1";
						break;
					}
				}
				if (quote_pos == -1) break;
			}
			print(slkdgjlksdjg.Substring(2).TrimStart('\r', '\n', '\t'));
			print("\r\n\t}");
		}
	}
%}