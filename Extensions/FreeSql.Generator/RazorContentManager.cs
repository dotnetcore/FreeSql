using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Generator
{
    class RazorContentManager
    {
        public static string 实体类_特性_cshtml =
        #region 长内容
            @"@using FreeSql.DatabaseModel;@{
var gen = Model as RazorModel;

Func<string, string> GetAttributeString = attr => {
	if (string.IsNullOrEmpty(attr)) return null;
	return string.Concat("", "", attr.Trim('[', ']'));
};
Func<DbColumnInfo, string> GetDefaultValue = col => {
    if (col.CsType == typeof(string)) return "" = string.Empty;"";
    return """";
};
}@{
switch (gen.fsql.Ado.DataType) {
	case FreeSql.DataType.PostgreSQL:
@:using System;
@:using System.Collections;
@:using System.Collections.Generic;
@:using System.Linq;
@:using System.Reflection;
@:using System.Threading.Tasks;
@:using Newtonsoft.Json;
@:using FreeSql.DataAnnotations;
@:using System.Net;
@:using Newtonsoft.Json.Linq;
@:using System.Net.NetworkInformation;
@:using NpgsqlTypes;
@:using Npgsql.LegacyPostgis;
		break;
	case FreeSql.DataType.SqlServer:
	case FreeSql.DataType.MySql:
	default:
@:using System;
@:using System.Collections;
@:using System.Collections.Generic;
@:using System.Linq;
@:using System.Reflection;
@:using System.Threading.Tasks;
@:using Newtonsoft.Json;
@:using FreeSql.DataAnnotations;
		break;
}
}
namespace @gen.NameSpace {

@if (string.IsNullOrEmpty(gen.table.Comment) == false) {
	@:/// <summary>
	@:/// @gen.table.Comment.Replace(""\r\n"", ""\n"").Replace(""\n"", ""\r\n		/// "")
	@:/// </summary>
}
	[JsonObject(MemberSerialization.OptIn)@GetAttributeString(gen.GetTableAttribute())]
	public partial class @gen.GetCsName(gen.FullTableName) {

	@foreach (var col in gen.columns) {

		if (string.IsNullOrEmpty(col.Coment) == false) {
		@:/// <summary>
		@:/// @col.Coment.Replace(""\r\n"", ""\n"").Replace(""\n"", ""\r\n		/// "")
		@:/// </summary>
		}
		@:@(""[JsonProperty"" + GetAttributeString(gen.GetColumnAttribute(col)) + ""]"")
		@:public @gen.GetCsType(col) @gen.GetCsName(col.Name) { get; set; }@GetDefaultValue(col)
@:
	}
	}
@gen.GetMySqlEnumSetDefine()
}";
        #endregion

        public static string 实体类_特性_导航属性_cshtml =
		#region 长内容 
			@"@using FreeSql.DatabaseModel;@{

var isLazying = true; //延时加载
var isOneToMany = true; //一对多，集合属性
var isManyToMany = true; //多对多，集合属性

var gen = Model as RazorModel;
var fks = gen.table.Foreigns;

Func<string, string> GetAttributeString = attr => {
	if (string.IsNullOrEmpty(attr)) return null;
	return string.Concat("", "", attr.Trim('[', ']'));
};

Func<DbForeignInfo, string> GetFkObjectName = fkx => {
	var eqfks = fks.Where(fk22a => fk22a.ReferencedTable.Name == fkx.ReferencedTable.Name);
	if (eqfks.Count() == 1) return fkx.ReferencedTable.Name;
	var fkretname = fkx.Columns[0].Name;
	if (fkretname.EndsWith(fkx.ReferencedColumns[0].Name, StringComparison.CurrentCultureIgnoreCase)) fkretname = fkretname.Substring(0, fkretname.Length - fkx.ReferencedColumns[0].Name.Length).TrimEnd('_');
	if (fkretname.EndsWith(fkx.ReferencedTable.Name, StringComparison.CurrentCultureIgnoreCase)) fkretname = fkretname.Substring(0, fkretname.Length - fkx.ReferencedTable.Name.Length).TrimEnd('_');
	if (fkretname.StartsWith(fkx.ReferencedTable.Name, StringComparison.CurrentCultureIgnoreCase)) fkretname = fkretname.Substring(fkx.ReferencedTable.Name.Length).TrimStart('_');
	return fkx.ReferencedTable.Name + (string.IsNullOrEmpty(fkretname) ? """" : (""_"" + fkretname));
};
Func<DbForeignInfo, string> GetFkObjectNameOutside = fkx => {
	var eqfks = fkx.Table.Foreigns.Where(fk22a => fk22a.ReferencedTable.Name == fkx.ReferencedTable.Name);
	if (eqfks.Count() == 1) return fkx.Table.Name;
	var fkretname = fkx.Columns[0].Name;
	if (fkretname.EndsWith(fkx.ReferencedColumns[0].Name, StringComparison.CurrentCultureIgnoreCase)) fkretname = fkretname.Substring(0, fkretname.Length - fkx.ReferencedColumns[0].Name.Length).TrimEnd('_');
	if (fkretname.EndsWith(fkx.ReferencedTable.Name, StringComparison.CurrentCultureIgnoreCase)) fkretname = fkretname.Substring(0, fkretname.Length - fkx.ReferencedTable.Name.Length).TrimEnd('_');
	if (fkretname.StartsWith(fkx.ReferencedTable.Name, StringComparison.CurrentCultureIgnoreCase)) fkretname = fkretname.Substring(fkx.ReferencedTable.Name.Length).TrimStart('_');
	return fkx.Table.Name + (string.IsNullOrEmpty(fkretname) ? """" : (""_"" + fkretname));
};
}@{
switch (gen.fsql.Ado.DataType) {
	case FreeSql.DataType.PostgreSQL:
@:using System;
@:using System.Collections.Generic;
@:using Newtonsoft.Json;
@:using FreeSql.DataAnnotations;
@:using System.Net;
@:using Newtonsoft.Json.Linq;
@:using System.Net.NetworkInformation;
@:using NpgsqlTypes;
@:using Npgsql.LegacyPostgis;
		break;
	case FreeSql.DataType.SqlServer:
	case FreeSql.DataType.MySql:
	default:
@:using System;
@:using System.Collections.Generic;
@:using Newtonsoft.Json;
@:using FreeSql.DataAnnotations;
		break;
}
}
namespace @gen.NameSpace {

@if (string.IsNullOrEmpty(gen.table.Comment) == false) {
	@:/// <summary>
	@:/// @gen.table.Comment.Replace(""\r\n"", ""\n"").Replace(""\n"", ""\r\n		/// "")
	@:/// </summary>
}
	[JsonObject(MemberSerialization.OptIn)@GetAttributeString(gen.GetTableAttribute())]
	public partial class @gen.GetCsName(gen.FullTableName) {

	@foreach (var col in gen.columns) {

		var findfks = fks.Where(fkaa => fkaa.Columns.Where(fkaac1 => fkaac1.Name == col.Name).Any());
		var csname = gen.GetCsName(col.Name);

		if (string.IsNullOrEmpty(col.Coment) == false) {
		@:/// <summary>
		@:/// @col.Coment.Replace(""\r\n"", ""\n"").Replace(""\n"", ""\r\n		/// "")
		@:/// </summary>
		}
		@:@(""[JsonProperty"" + GetAttributeString(gen.GetColumnAttribute(col)) + ""]"")
		if (findfks.Any() == false) {
		@:public @gen.GetCsType(col) @csname { get; set; }
		} else {
		@:public @gen.GetCsType(col) @csname { get => _@csname; set {
			@:if (_@csname == value) return;
			@:_@csname = value;
			foreach (var fkcok2 in findfks) {
			@:@gen.GetCsName(GetFkObjectName(fkcok2)) = null;
			}
		@:} }
		@:private @gen.GetCsType(col) _@csname;
		}
@:
	}
@if (fks.Any()) {
@:
		@:#region 外键 => 导航属性，ManyToOne/OneToOne
		foreach (var fk in fks) {
			var fkTableName = (fk.ReferencedTable.Schema + ""."" + fk.ReferencedTable.Name).Trim('.');
            if (fk.ReferencedTable.Schema == ""public"" || fk.ReferencedTable.Schema == ""dbo"")
            {
                fkTableName = fkTableName.Replace(fk.ReferencedTable.Schema + ""."", """");
            }
@:
		@:[Navigate(""@string.Join("", "", fk.Columns.Select(a => gen.GetCsName(a.Name)))"")]
		@:public@(isLazying ? "" virtual"" : """") @gen.GetCsName(fkTableName) @gen.GetCsName(GetFkObjectName(fk)) { get; set; }
		}
@:
		@:#endregion
}
@if (isOneToMany && gen.tables.Where(tmpft => tmpft.Foreigns.Where(tmpftfk => tmpftfk.ReferencedTable.Schema == gen.table.Schema && tmpftfk.ReferencedTable.Name == gen.table.Name && tmpftfk.Columns.Where(tmpcol => tmpcol.IsPrimary).Count() != tmpftfk.Columns.Count).Any()).Any()) {
@:
		@:#region 外键 => 导航属性，OneToMany
	foreach (var ft in gen.tables) {
        var ftfks = ft.Foreigns.Where(ftfk => ftfk.ReferencedTable.Schema == gen.table.Schema && ftfk.ReferencedTable.Name == gen.table.Name && ftfk.Columns.Where(tmpcol => tmpcol.IsPrimary).Count() != ftfk.Columns.Count).ToArray();
        foreach (var fk in ftfks) {
            var fkTableName = (ft.Schema + ""."" + ft.Name).Trim('.');
            if (ft.Schema == ""public"" || ft.Schema == ""dbo"")
            {
                fkTableName = fkTableName.Replace(ft.Schema + ""."", """");
            }
@:
		@:[Navigate(""@string.Join("", "", fk.Columns.Select(a => gen.GetCsName(a.Name)))"")]
		@:public@(isLazying ? "" virtual"" : """") List<@gen.GetCsName(fkTableName)> @gen.GetCsName(GetFkObjectNameOutside(fk))s { get; set; }
		}
	}
@:
		@:#endregion
}
@if (isManyToMany) {
@:
		@:#region 外键 => 导航属性，ManyToMany
	foreach (var ft in gen.tables) {
        if (ft != gen.table) {
            var ftfks = ft.Foreigns.Where(ftfk => ftfk.Columns.Where(ftfkcol => ftfkcol.IsPrimary == false).Any() == false).ToArray();
            if (ftfks.Length == 2) {
                var fk1 = ftfks.Where(ftfk => (ftfk.ReferencedTable.Schema + ""."" + ftfk.ReferencedTable.Name).Trim('.') == gen.FullTableName).ToArray();
                if (fk1.Length == 1) {
                    var fk2 = ftfks.Where(ftfk => fk1.Contains(ftfk) == false).ToArray();

                    var midft = ft;
                    var leftft = gen.table;
                    DbTableInfo rightft = null;
                    if (fk2.Any()) {
                        rightft = fk2[0].ReferencedTable;
                    } else {
                        rightft = fk1[1].ReferencedTable;
                    }

                    var fkTableName = (rightft.Schema + ""."" + rightft.Name).Trim('.');
                    if (rightft.Schema == ""public"" || rightft.Schema == ""dbo"")
                    {
                        fkTableName = fkTableName.Replace(rightft.Schema + ""."", """");
                    }
                    var csname = rightft.Name;
@:
		@:public@(isLazying ? "" virtual"" : """") List<@gen.GetCsName(fkTableName)> @gen.GetCsName(csname)s { get; set; }
				}
			}
		}
	}
@:
		@:#endregion
}
	}
@gen.GetMySqlEnumSetDefine()
}"; 
        #endregion
    }
}
