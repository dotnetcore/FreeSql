
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using FreeSql;
//using FreeSql.DatabaseModel;

////namespace TplDynamicCodeGenerate {
//public class TplDynamicCodeGenerate_view1 : FreeSql.Generator.TemplateEngin.ITemplateOutput {
//	public FreeSql.Generator.TemplateEngin.TemplateReturnInfo OuTpUt(StringBuilder tOuTpUt, IDictionary oPtIoNs, string rEfErErFiLeNaMe, FreeSql.Generator.TemplateEngin tEmPlAtEsEnDeR) {
//		FreeSql.Generator.TemplateEngin.TemplateReturnInfo rTn = tOuTpUt == null ?
//			new FreeSql.Generator.TemplateEngin.TemplateReturnInfo { Sb = (tOuTpUt = new StringBuilder()), Blocks = new Dictionary<string, int[]>() } :
//			new FreeSql.Generator.TemplateEngin.TemplateReturnInfo { Sb = tOuTpUt, Blocks = new Dictionary<string, int[]>() };
//		Dictionary<string, int[]> TPL__blocks = rTn.Blocks;
//		Stack<int[]> TPL__blocks_stack = new Stack<int[]>();
//		int[] TPL__blocks_stack_peek;
//		List<IDictionary> TPL__forc = new List<IDictionary>();
//		Func<IDictionary> pRoCeSsOpTiOnS = new Func<IDictionary>(delegate () {
//			IDictionary nEwoPtIoNs = new Hashtable();
//			foreach (DictionaryEntry oPtIoNs_dE in oPtIoNs)
//				nEwoPtIoNs[oPtIoNs_dE.Key] = oPtIoNs_dE.Value;
//			foreach (IDictionary TPL__forc_dIc in TPL__forc)
//				foreach (DictionaryEntry TPL__forc_dIc_dE in TPL__forc_dIc)
//					nEwoPtIoNs[TPL__forc_dIc_dE.Key] = TPL__forc_dIc_dE.Value;
//			return nEwoPtIoNs;
//		});
//		FreeSql.Generator.TemplateEngin.TemplateIf tPlIf = delegate (object exp) {
//			if (exp is bool) return (bool)exp;
//			if (exp == null) return false;
//			if (exp is int && (int)exp == 0) return false;
//			if (exp is string && (string)exp == string.Empty) return false;
//			if (exp is long && (long)exp == 0) return false;
//			if (exp is short && (short)exp == 0) return false;
//			if (exp is byte && (byte)exp == 0) return false;
//			if (exp is double && (double)exp == 0) return false;
//			if (exp is float && (float)exp == 0) return false;
//			if (exp is decimal && (decimal)exp == 0) return false;
//			return true;
//		};
//		FreeSql.Generator.TemplateEngin.TemplatePrint print = delegate (object[] pArMs) {
//			if (pArMs == null || pArMs.Length == 0) return;
//			foreach (object pArMs_A in pArMs) if (pArMs_A != null) tOuTpUt.Append(pArMs_A);
//		};
//		FreeSql.Generator.TemplateEngin.TemplatePrint Print = prin
//			dynamic index = oPtIoNs["index"];
//		dynamic col = oPtIoNs["col"];
//		dynamic table = oPtIoNs["table"];
//		dynamic dbfirst = oPtIoNs["dbfirst"]; t;
//		tOuTpUt.Append("using System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing System.Reflection;\r\nusing System.Threading.Tasks;\r\nusing Newtonsoft.Json;\r\nusing FreeSql.DataAnnotations;\r\n");

//		var dbf = dbfirst as FreeSql.IDbFirst;
//		var cols = (table.Columns as List<DbColumnInfo>);

//		Func<string, string> UString = stra => stra.Substring(0, 1).ToUpper() + stra.Substring(1);
//		Func<DbColumnInfo, string> GetCsType = cola3 => {
//			if (cola3.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Enum || cola3.DbType == (int)MySql.Data.MySqlClient.MySqlDbType.Set) {
//				return $"{UString(cola3.Table.Name)}{cola3.Name.ToUpper()}{(cola3.IsNullable ? "?" : "")}";
//			}
//			return dbf.GetCsType(cola3);
//		};

//		tOuTpUt.Append("\r\nnamespace test.Model {\r\n\r\n	[JsonObject(MemberSerialization.OptIn), Table(Name = \"");
//		Print(!string.IsNullOrEmpty(table.Schema) ? table.Schema + "." : "");
//		tOuTpUt.Append("");
//		Print(table.Name);
//		tOuTpUt.Append("\"");
//		if (tPlIf(cols.Where(cola003 => cola003.Name.ToLower() == "is_deleted" || cola003.Name.ToLower() == "isdeleted").Any())) {
//			tOuTpUt.Append(", SelectFilter = \"a.IsDeleted = 1\"");
//		}
//		tOuTpUt.Append(")]\r\n	public partial class ");
//		Print(UString(table.Name));
//		tOuTpUt.Append(" {");
//		//new Action(delegate () {
//		IDictionary TPL__tmp1 = new Hashtable();
//		TPL__forc.Add(TPL__tmp1);
//		var TPL__tmp2 = table.Columns;
//		var TPL__tmp3 = col;
//		var TPL__tmp4 = index;
//		index = 0;
//		if (TPL__tmp2 != null)
//			foreach (var TPL__tmp5 in TPL__tmp2) {
//				TPL__tmp1["index"] = ++index;
//				TPL__tmp1["col"] = TPL__tmp5;
//				col = TPL__tmp5;
//				tOuTpUt.Append("\r\n		");
//				if (tPlIf(string.IsNullOrEmpty(col.Coment) == false)) {
//					tOuTpUt.Append("/// <summary>\r\n		/// ");
//					Print(col.Coment.Replace("\r\n", "\n").Replace("\n", "\r\n		/// "));
//					tOuTpUt.Append("\r\n		/// </summary>");
//				}
//				tOuTpUt.Append("\r\n		[JsonProperty, Column(Name = \"");
//				Print(col.Name);
//				tOuTpUt.Append("\", DbType = \"");
//				Print(col.DbTypeTextFull);
//				tOuTpUt.Append("\"");
//				if (tPlIf(col.IsPrimary == true)) {
//					tOuTpUt.Append(", IsPrimary = true");
//				}
//				tOuTpUt.Append("");
//				if (tPlIf(col.IsIdentity == true)) {
//					tOuTpUt.Append(", IsIdentity = true");
//				}
//				tOuTpUt.Append("");
//				if (tPlIf(col.IsNullable == true)) {
//					tOuTpUt.Append(", IsNullable = true");
//				}
//				tOuTpUt.Append(")]\r\n		public ");
//				Print(GetCsType(col));
//				tOuTpUt.Append(" ");
//				Print(UString(col.Name));
//				tOuTpUt.Append(" { get; set; }\r\n		");
//			}
//		col = TPL__tmp3;
//		index = TPL__tmp4;
//		TPL__forc.RemoveAt(TPL__forc.Count - 1);
//		//})();
//		tOuTpUt.Append("\r\n	}\r\n");
//		tEmPlAtEsEnDeR.RenderFile2(tOuTpUt, pRoCeSsOpTiOnS(), "../../include/enumtype.tpl", rEfErErFiLeNaMe);
//		tOuTpUt.Append("\r\n}");
//		return rTn;
//	}
//}
////}
