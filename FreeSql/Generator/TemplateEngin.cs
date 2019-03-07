using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FreeSql.Generator {
	public class TemplateEngin : IDisposable {
		public interface ITemplateOutput {
			/// <summary>
			/// 
			/// </summary>
			/// <param name="tOuTpUt">返回内容</param>
			/// <param name="oPtIoNs">渲染对象</param>
			/// <param name="rEfErErFiLeNaMe">当前文件路径</param>
			/// <param name="tEmPlAtEsEnDeR"></param>
			/// <returns></returns>
			TemplateReturnInfo OuTpUt(StringBuilder tOuTpUt, IDictionary oPtIoNs, string rEfErErFiLeNaMe, TemplateEngin tEmPlAtEsEnDeR);
		}
		public class TemplateReturnInfo {
			public Dictionary<string, int[]> Blocks;
			public StringBuilder Sb;
		}
		public delegate bool TemplateIf(object exp);
		public delegate void TemplatePrint(params object[] parms);

		private static int _view = 0;
		private static Regex _reg = new Regex(@"\{(\$TEMPLATE__CODE|\/\$TEMPLATE__CODE|import\s+|module\s+|extends\s+|block\s+|include\s+|for\s+|if\s+|#|\/for|elseif|else|\/if|\/block|\/module)([^\}]*)\}", RegexOptions.Compiled);
		private static Regex _reg_forin = new Regex(@"^([\w_]+)\s*,?\s*([\w_]+)?\s+in\s+(.+)", RegexOptions.Compiled);
		private static Regex _reg_foron = new Regex(@"^([\w_]+)\s*,?\s*([\w_]+)?,?\s*([\w_]+)?\s+on\s+(.+)", RegexOptions.Compiled);
		private static Regex _reg_forab = new Regex(@"^([\w_]+)\s+([^,]+)\s*,\s*(.+)", RegexOptions.Compiled);
		private static Regex _reg_miss = new Regex(@"\{\/?miss\}", RegexOptions.Compiled);
		private static Regex _reg_code = new Regex(@"(\{%|%\})", RegexOptions.Compiled);
		private static Regex _reg_syntax = new Regex(@"<(\w+)\s+@(if|for|else)\s*=""([^""]*)""", RegexOptions.Compiled);
		private static Regex _reg_htmltag = new Regex(@"<\/?\w+[^>]*>", RegexOptions.Compiled);
		private static Regex _reg_blank = new Regex(@"\s+", RegexOptions.Compiled);
		private static Regex _reg_complie_undefined = new Regex(@"(当前上下文中不存在名称)?“(\w+)”", RegexOptions.Compiled);

		private Dictionary<string, ITemplateOutput> _cache = new Dictionary<string, ITemplateOutput>();
		private object _cache_lock = new object();
		private string _viewDir;
		private string[] _usings;
		private FileSystemWatcher _fsw = new FileSystemWatcher();

		public TemplateEngin(string viewDir, params string[] usings) {
			_viewDir = Utils.TranslateUrl(viewDir);
			_usings = usings;
			_fsw = new FileSystemWatcher(_viewDir);
			_fsw.IncludeSubdirectories = true;
			_fsw.Changed += ViewDirChange;
			_fsw.Renamed += ViewDirChange;
			_fsw.EnableRaisingEvents = true;
		}
		public void Dispose() {
			_fsw.Dispose();
		}
		void ViewDirChange(object sender, FileSystemEventArgs e) {
			string filename = e.FullPath.ToLower();
			lock (_cache_lock) {
				_cache.Remove(filename);
			}
		}
		public TemplateReturnInfo RenderFile2(StringBuilder sb, IDictionary options, string filename, string refererFilename) {
			if (filename[0] == '/' || string.IsNullOrEmpty(refererFilename)) refererFilename = _viewDir;
			//else refererFilename = Path.GetDirectoryName(refererFilename);
			string filename2 = Utils.TranslateUrl(filename, refererFilename);
			ITemplateOutput tpl;
			if (_cache.TryGetValue(filename2, out tpl) == false) {
				string tplcode = File.Exists(filename2) == false ? string.Concat("文件不存在 ", filename) : Utils.ReadTextFile(filename2);
				tpl = Parser(tplcode, _usings, options);
				lock (_cache_lock) {
					if (_cache.ContainsKey(filename2) == false) {
						_cache.Add(filename2, tpl);
					}
				}
			}
			try {
				return tpl.OuTpUt(sb, options, filename2, this);
			} catch (Exception ex) {
				TemplateReturnInfo ret = sb == null ?
					new TemplateReturnInfo { Sb = new StringBuilder(), Blocks = new Dictionary<string, int[]>() } :
					new TemplateReturnInfo { Sb = sb, Blocks = new Dictionary<string, int[]>() };
				ret.Sb.Append(refererFilename);
				ret.Sb.Append(" -> ");
				ret.Sb.Append(filename);
				ret.Sb.Append("\r\n");
				ret.Sb.Append(ex.Message);
				ret.Sb.Append("\r\n");
				ret.Sb.Append(ex.StackTrace);
				return ret;
			}
		}
		public string RenderFile(string filename, IDictionary options) {
			TemplateReturnInfo ret = this.RenderFile2(null, options, filename, null);
			return ret.Sb.ToString();
		}
		private static ITemplateOutput Parser(string tplcode, string[] usings, IDictionary options) {
			int view = Interlocked.Increment(ref _view);
			StringBuilder sb = new StringBuilder();
			IDictionary options_copy = new Hashtable();
			foreach (DictionaryEntry options_de in options) options_copy[options_de.Key] = options_de.Value;
			sb.AppendFormat(@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;{1}

//namespace TplDynamicCodeGenerate {{
	public class TplDynamicCodeGenerate_view{0} : FreeSql.Generator.TemplateEngin.ITemplateOutput {{
		public FreeSql.Generator.TemplateEngin.TemplateReturnInfo OuTpUt(StringBuilder tOuTpUt, IDictionary oPtIoNs, string rEfErErFiLeNaMe, FreeSql.Generator.TemplateEngin tEmPlAtEsEnDeR) {{
			FreeSql.Generator.TemplateEngin.TemplateReturnInfo rTn = tOuTpUt == null ? 
				new FreeSql.Generator.TemplateEngin.TemplateReturnInfo {{ Sb = (tOuTpUt = new StringBuilder()), Blocks = new Dictionary<string, int[]>() }} :
				new FreeSql.Generator.TemplateEngin.TemplateReturnInfo {{ Sb = tOuTpUt, Blocks = new Dictionary<string, int[]>() }};
			Dictionary<string, int[]> TPL__blocks = rTn.Blocks;
			Stack<int[]> TPL__blocks_stack = new Stack<int[]>();
			int[] TPL__blocks_stack_peek;
			List<IDictionary> TPL__forc = new List<IDictionary>();
			Func<IDictionary> pRoCeSsOpTiOnS = new Func<IDictionary>(delegate () {{
				IDictionary nEwoPtIoNs = new Hashtable();
				foreach (DictionaryEntry oPtIoNs_dE in oPtIoNs)
					nEwoPtIoNs[oPtIoNs_dE.Key] = oPtIoNs_dE.Value;
				foreach (IDictionary TPL__forc_dIc in TPL__forc)
					foreach (DictionaryEntry TPL__forc_dIc_dE in TPL__forc_dIc)
						nEwoPtIoNs[TPL__forc_dIc_dE.Key] = TPL__forc_dIc_dE.Value;
				return nEwoPtIoNs;
			}});
			FreeSql.Generator.TemplateEngin.TemplateIf tPlIf = delegate(object exp) {{
				if (exp is bool) return (bool)exp;
				if (exp == null) return false;
				if (exp is int && (int)exp == 0) return false;
				if (exp is string && (string)exp == string.Empty) return false;
				if (exp is long && (long)exp == 0) return false;
				if (exp is short && (short)exp == 0) return false;
				if (exp is byte && (byte)exp == 0) return false;
				if (exp is double && (double)exp == 0) return false;
				if (exp is float && (float)exp == 0) return false;
				if (exp is decimal && (decimal)exp == 0) return false;
				return true;
			}};
			FreeSql.Generator.TemplateEngin.TemplatePrint print = delegate(object[] pArMs) {{
				if (pArMs == null || pArMs.Length == 0) return;
				foreach (object pArMs_A in pArMs) if (pArMs_A != null) tOuTpUt.Append(pArMs_A);
			}};
			FreeSql.Generator.TemplateEngin.TemplatePrint Print = print;", view, usings?.Any() == true ? $"\r\nusing {string.Join(";\r\nusing ", usings)};" : "");

			#region {miss}...{/miss}块内容将不被解析
			string[] tmp_content_arr = _reg_miss.Split(tplcode);
			if (tmp_content_arr.Length > 1) {
				sb.AppendFormat(@"
			string[] TPL__MISS = new string[{0}];", Math.Ceiling(1.0 * (tmp_content_arr.Length - 1) / 2));
				int miss_len = -1;
				for (int a = 1; a < tmp_content_arr.Length; a += 2) {
					sb.Append(string.Concat(@"
			TPL__MISS[", ++miss_len, @"] = """, Utils.GetConstString(tmp_content_arr[a]), @""";"));
					tmp_content_arr[a] = string.Concat("{#TPL__MISS[", miss_len, "]}");
				}
				tplcode = string.Join("", tmp_content_arr);
			}
			#endregion
			#region 扩展语法如 <div @if="表达式"></div>
			tplcode = htmlSyntax(tplcode, 3); //<div @if="c#表达式" @for="index 1,100"></div>
											  //处理 {% %} 块 c#代码
			tmp_content_arr = _reg_code.Split(tplcode);
			if (tmp_content_arr.Length == 1) {
				tplcode = Utils.GetConstString(tplcode)
					.Replace("{%", "{$TEMPLATE__CODE}")
					.Replace("%}", "{/$TEMPLATE__CODE}");
			} else {
				tmp_content_arr[0] = Utils.GetConstString(tmp_content_arr[0]);
				for (int a = 1; a < tmp_content_arr.Length; a += 4) {
					tmp_content_arr[a] = "{$TEMPLATE__CODE}";
					tmp_content_arr[a + 2] = "{/$TEMPLATE__CODE}";
					tmp_content_arr[a + 3] = Utils.GetConstString(tmp_content_arr[a + 3]);
				}
				tplcode = string.Join("", tmp_content_arr);
			}
			#endregion
			sb.Append(@"
			tOuTpUt.Append(""");

			string error = null;
			int tpl_tmpid = 0;
			int forc_i = 0;
			string extends = null;
			Stack<string> codeTree = new Stack<string>();
			Stack<string> forEndRepl = new Stack<string>();
			sb.Append(_reg.Replace(tplcode, delegate (Match m) {
				string _0 = m.Groups[0].Value;
				if (!string.IsNullOrEmpty(error)) return _0;

				string _1 = m.Groups[1].Value.Trim(' ', '\t');
				string _2 = m.Groups[2].Value
					.Replace("\\\\", "\\")
					.Replace("\\\"", "\"");
				_2 = Utils.ReplaceSingleQuote(_2);

				switch (_1) {
					#region $TEMPLATE__CODE--------------------------------------------------
					case "$TEMPLATE__CODE":
						codeTree.Push(_1);
						return @""");
";
					case "/$TEMPLATE__CODE":
						string pop = codeTree.Pop();
						if (pop != "$TEMPLATE__CODE") {
							codeTree.Push(pop);
							error = "编译出错，{% 与 %} 并没有配对";
							return _0;
						}
						return @"
			tOuTpUt.Append(""";
					#endregion
					case "include":
						return string.Format(@""");
tEmPlAtEsEnDeR.RenderFile2(tOuTpUt, pRoCeSsOpTiOnS(), ""{0}"", rEfErErFiLeNaMe);
			tOuTpUt.Append(""", _2);
					case "import":
						return _0;
					case "module":
						return _0;
					case "/module":
						return _0;
					case "extends":
						//{extends ../inc/layout.html}
						if (string.IsNullOrEmpty(extends) == false) return _0;
						extends = _2;
						return string.Empty;
					case "block":
						codeTree.Push("block");
						return string.Format(@""");
TPL__blocks_stack_peek = new int[] {{ tOuTpUt.Length, 0 }};
TPL__blocks_stack.Push(TPL__blocks_stack_peek);
TPL__blocks.Add(""{0}"", TPL__blocks_stack_peek);
tOuTpUt.Append(""", _2.Trim(' ', '\t'));
					case "/block":
						codeTreeEnd(codeTree, "block");
						return @""");
TPL__blocks_stack_peek = TPL__blocks_stack.Pop();
TPL__blocks_stack_peek[1] = tOuTpUt.Length - TPL__blocks_stack_peek[0];
tOuTpUt.Append(""";

					#region ##---------------------------------------------------------
					case "#":
						if (_2[0] == '#')
							return string.Format(@""");
			try {{ Print({0}); }} catch {{ }}
			tOuTpUt.Append(""", _2.Substring(1));
						return string.Format(@""");
			Print({0});
			tOuTpUt.Append(""", _2);
					#endregion
					#region for--------------------------------------------------------
					case "for":
						forc_i++;
						int cur_tpl_tmpid = tpl_tmpid;
						string sb_endRepl = string.Empty;
						StringBuilder sbfor = new StringBuilder();
						sbfor.Append(@""");");
						Match mfor = _reg_forin.Match(_2);
						if (mfor.Success) {
							string mfor1 = mfor.Groups[1].Value.Trim(' ', '\t');
							string mfor2 = mfor.Groups[2].Value.Trim(' ', '\t');
							sbfor.AppendFormat(@"
//new Action(delegate () {{
	IDictionary TPL__tmp{0} = new Hashtable();
	TPL__forc.Add(TPL__tmp{0});
	var TPL__tmp{1} = {3};
	var TPL__tmp{2} = {4};", ++tpl_tmpid, ++tpl_tmpid, ++tpl_tmpid, mfor.Groups[3].Value, mfor1);
							sb_endRepl = string.Concat(sb_endRepl, string.Format(@"
	{0} = TPL__tmp{1};", mfor1, cur_tpl_tmpid + 3));
							if (options_copy.Contains(mfor1) == false) options_copy[mfor1] = null;
							if (!string.IsNullOrEmpty(mfor2)) {
								sbfor.AppendFormat(@"
	var TPL__tmp{1} = {0};
	{0} = 0;", mfor2, ++tpl_tmpid);
								sb_endRepl = string.Concat(sb_endRepl, string.Format(@"
	{0} = TPL__tmp{1};", mfor2, tpl_tmpid));
								if (options_copy.Contains(mfor2) == false) options_copy[mfor2] = null;
							}
							sbfor.AppendFormat(@"
	if (TPL__tmp{1} != null)
	foreach (var TPL__tmp{0} in TPL__tmp{1}) {{", ++tpl_tmpid, cur_tpl_tmpid + 2);
							if (!string.IsNullOrEmpty(mfor2))
								sbfor.AppendFormat(@"
		TPL__tmp{1}[""{0}""] = ++ {0};", mfor2, cur_tpl_tmpid + 1);
							sbfor.AppendFormat(@"
		TPL__tmp{1}[""{0}""] = TPL__tmp{2};
		{0} = TPL__tmp{2};
		tOuTpUt.Append(""", mfor1, cur_tpl_tmpid + 1, tpl_tmpid);
							codeTree.Push("for");
							forEndRepl.Push(sb_endRepl);
							return sbfor.ToString();
						}
						mfor = _reg_foron.Match(_2);
						if (mfor.Success) {
							string mfor1 = mfor.Groups[1].Value.Trim(' ', '\t');
							string mfor2 = mfor.Groups[2].Value.Trim(' ', '\t');
							string mfor3 = mfor.Groups[3].Value.Trim(' ', '\t');
							sbfor.AppendFormat(@"
//new Action(delegate () {{
	IDictionary TPL__tmp{0} = new Hashtable();
	TPL__forc.Add(TPL__tmp{0});
	var TPL__tmp{1} = {3};
	var TPL__tmp{2} = {4};", ++tpl_tmpid, ++tpl_tmpid, ++tpl_tmpid, mfor.Groups[4].Value, mfor1);
							sb_endRepl = string.Concat(sb_endRepl, string.Format(@"
	{0} = TPL__tmp{1};", mfor1, cur_tpl_tmpid + 3));
							if (options_copy.Contains(mfor1) == false) options_copy[mfor1] = null;
							if (!string.IsNullOrEmpty(mfor2)) {
								sbfor.AppendFormat(@"
	var TPL__tmp{1} = {0};", mfor2, ++tpl_tmpid);
								sb_endRepl = string.Concat(sb_endRepl, string.Format(@"
	{0} = TPL__tmp{1};", mfor2, tpl_tmpid));
								if (options_copy.Contains(mfor2) == false) options_copy[mfor2] = null;
							}
							if (!string.IsNullOrEmpty(mfor3)) {
								sbfor.AppendFormat(@"
	var TPL__tmp{1} = {0};
	{0} = 0;", mfor3, ++tpl_tmpid);
								sb_endRepl = string.Concat(sb_endRepl, string.Format(@"
	{0} = TPL__tmp{1};", mfor3, tpl_tmpid));
								if (options_copy.Contains(mfor3) == false) options_copy[mfor3] = null;
							}
							sbfor.AppendFormat(@"
	if (TPL__tmp{2} != null)
	foreach (DictionaryEntry TPL__tmp{1} in TPL__tmp{2}) {{
		{0} = TPL__tmp{1}.Key;
		TPL__tmp{3}[""{0}""] = {0};", mfor1, ++tpl_tmpid, cur_tpl_tmpid + 2, cur_tpl_tmpid + 1);
							if (!string.IsNullOrEmpty(mfor2))
								sbfor.AppendFormat(@"
		{0} = TPL__tmp{1}.Value;
		TPL__tmp{2}[""{0}""] = {0};", mfor2, tpl_tmpid, cur_tpl_tmpid + 1);
							if (!string.IsNullOrEmpty(mfor3))
								sbfor.AppendFormat(@"
		TPL__tmp{1}[""{0}""] = ++ {0};", mfor3, cur_tpl_tmpid + 1);
							sbfor.AppendFormat(@"
		tOuTpUt.Append(""");
							codeTree.Push("for");
							forEndRepl.Push(sb_endRepl);
							return sbfor.ToString();
						}
						mfor = _reg_forab.Match(_2);
						if (mfor.Success) {
							string mfor1 = mfor.Groups[1].Value.Trim(' ', '\t');
							sbfor.AppendFormat(@"
//new Action(delegate () {{
	IDictionary TPL__tmp{0} = new Hashtable();
	TPL__forc.Add(TPL__tmp{0});
	var TPL__tmp{1} = {5};
	{5} = {3} - 1;
	if ({5} == null) {5} = 0;
	var TPL__tmp{2} = {4} + 1;
	while (++{5} < TPL__tmp{2}) {{
		TPL__tmp{0}[""{5}""] = {5};
		tOuTpUt.Append(""", ++tpl_tmpid, ++tpl_tmpid, ++tpl_tmpid, mfor.Groups[2].Value, mfor.Groups[3].Value, mfor1);
							sb_endRepl = string.Concat(sb_endRepl, string.Format(@"
	{0} = TPL__tmp{1};", mfor1, cur_tpl_tmpid + 1));
							if (options_copy.Contains(mfor1) == false) options_copy[mfor1] = null;
							codeTree.Push("for");
							forEndRepl.Push(sb_endRepl);
							return sbfor.ToString();
						}
						return _0;
					case "/for":
						if (--forc_i < 0) return _0;
						codeTreeEnd(codeTree, "for");
						return string.Format(@""");
	}}{0}
	TPL__forc.RemoveAt(TPL__forc.Count - 1);
//}})();
			tOuTpUt.Append(""", forEndRepl.Pop());
					#endregion
					#region if---------------------------------------------------------
					case "if":
						codeTree.Push("if");
						return string.Format(@""");
			if ({1}tPlIf({0})) {{
				tOuTpUt.Append(""", _2[0] == '!' ? _2.Substring(1) : _2, _2[0] == '!' ? '!' : ' ');
					case "elseif":
						codeTreeEnd(codeTree, "if");
						codeTree.Push("if");
						return string.Format(@""");
			}} else if ({1}tPlIf({0})) {{
				tOuTpUt.Append(""", _2[0] == '!' ? _2.Substring(1) : _2, _2[0] == '!' ? '!' : ' ');
					case "else":
						codeTreeEnd(codeTree, "if");
						codeTree.Push("if");
						return @""");
			} else {
			tOuTpUt.Append(""";
					case "/if":
						codeTreeEnd(codeTree, "if");
						return @""");
			}
			tOuTpUt.Append(""";
						#endregion
				}
				return _0;
			}));

			sb.Append(@""");");
			if (string.IsNullOrEmpty(extends) == false) {
				sb.AppendFormat(@"
FreeSql.Generator.TemplateEngin.TemplateReturnInfo eXtEnDs_ReT = tEmPlAtEsEnDeR.RenderFile2(null, pRoCeSsOpTiOnS(), ""{0}"", rEfErErFiLeNaMe);
string rTn_Sb_string = rTn.Sb.ToString();
foreach(string eXtEnDs_ReT_blocks_key in eXtEnDs_ReT.Blocks.Keys) {{
	if (rTn.Blocks.ContainsKey(eXtEnDs_ReT_blocks_key)) {{
		int[] eXtEnDs_ReT_blocks_value = eXtEnDs_ReT.Blocks[eXtEnDs_ReT_blocks_key];
		eXtEnDs_ReT.Sb.Remove(eXtEnDs_ReT_blocks_value[0], eXtEnDs_ReT_blocks_value[1]);
		int[] rTn_blocks_value = rTn.Blocks[eXtEnDs_ReT_blocks_key];
		eXtEnDs_ReT.Sb.Insert(eXtEnDs_ReT_blocks_value[0], rTn_Sb_string.Substring(rTn_blocks_value[0], rTn_blocks_value[1]));
		foreach(string eXtEnDs_ReT_blocks_keyb in eXtEnDs_ReT.Blocks.Keys) {{
			if (eXtEnDs_ReT_blocks_keyb == eXtEnDs_ReT_blocks_key) continue;
			int[] eXtEnDs_ReT_blocks_valueb = eXtEnDs_ReT.Blocks[eXtEnDs_ReT_blocks_keyb];
			if (eXtEnDs_ReT_blocks_valueb[0] >= eXtEnDs_ReT_blocks_value[0])
				eXtEnDs_ReT_blocks_valueb[0] = eXtEnDs_ReT_blocks_valueb[0] - eXtEnDs_ReT_blocks_value[1] + rTn_blocks_value[1];
		}}
		eXtEnDs_ReT_blocks_value[1] = rTn_blocks_value[1];
	}}
}}
return eXtEnDs_ReT;
", extends);
			} else {
				sb.Append(@"
return rTn;");
			}
			sb.Append(@"
		}
	}
//}
");
			var str = "FreeSql.Generator.TemplateEngin.TemplatePrint Print = print;";
			int dim_idx = sb.ToString().IndexOf(str) + str.Length;
			foreach (string dic_name in options_copy.Keys) {
				sb.Insert(dim_idx, string.Format(@"
			dynamic {0} = oPtIoNs[""{0}""];", dic_name));
			}
			//Console.WriteLine(sb.ToString());
			return Complie(sb.ToString(), @"TplDynamicCodeGenerate_view" + view);
		}
		private static string codeTreeEnd(Stack<string> codeTree, string tag) {
			string ret = string.Empty;
			Stack<int> pop = new Stack<int>();
			foreach (string ct in codeTree) {
				if (ct == "import" ||
					ct == "include") {
					pop.Push(1);
				} else if (ct == tag) {
					pop.Push(2);
					break;
				} else {
					if (string.IsNullOrEmpty(tag) == false) pop.Clear();
					break;
				}
			}
			if (pop.Count == 0 && string.IsNullOrEmpty(tag) == false)
				return string.Concat("语法错误，{", tag, "} {/", tag, "} 并没配对");
			while (pop.Count > 0 && pop.Pop() > 0) codeTree.Pop();
			return ret;
		}
		#region htmlSyntax
		private static string htmlSyntax(string tplcode, int num) {

			while (num-- > 0) {
				string[] arr = _reg_syntax.Split(tplcode);

				if (arr.Length == 1) break;
				for (int a = 1; a < arr.Length; a += 4) {
					string tag = string.Concat('<', arr[a]);
					string end = string.Concat("</", arr[a], '>');
					int fc = 1;
					for (int b = a; fc > 0 && b < arr.Length; b += 4) {
						if (b > a && arr[a].ToLower() == arr[b].ToLower()) fc++;
						int bpos = 0;
						while (true) {
							int fa = arr[b + 3].IndexOf(tag, bpos);
							int fb = arr[b + 3].IndexOf(end, bpos);
							if (b == a) {
								var z = arr[b + 3].IndexOf("/>");
								if ((fb == -1 || z < fb) && z != -1) {
									var y = arr[b + 3].Substring(0, z + 2);
									if (_reg_htmltag.IsMatch(y) == false)
										fb = z - end.Length + 2;
								}
							}
							if (fa == -1 && fb == -1) break;
							if (fa != -1 && (fa < fb || fb == -1)) {
								fc++;
								bpos = fa + tag.Length;
								continue;
							}
							if (fb != -1) fc--;
							if (fc <= 0) {
								var a1 = arr[a + 1];
								var end3 = string.Concat("{/", a1, "}");
								if (a1.ToLower() == "else") {
									if (_reg_blank.Replace(arr[a - 4 + 3], "").EndsWith("{/if}", StringComparison.CurrentCultureIgnoreCase) == true) {
										var idx = arr[a - 4 + 3].IndexOf("{/if}");
										arr[a - 4 + 3] = string.Concat(arr[a - 4 + 3].Substring(0, idx), arr[a - 4 + 3].Substring(idx + 5));
										//如果 @else="有条件内容"，则变换成 elseif 条件内容
										if (_reg_blank.Replace(arr[a + 2], "").Length > 0) a1 = "elseif";
										end3 = "{/if}";
									} else {
										arr[a] = string.Concat("指令 @", arr[a + 1], "='", arr[a + 2], "' 没紧接着 if/else 指令之后，无效. <", arr[a]);
										arr[a + 1] = arr[a + 2] = string.Empty;
									}
								}
								if (arr[a + 1].Length > 0) {
									if (_reg_blank.Replace(arr[a + 2], "").Length > 0 || a1.ToLower() == "else") {
										arr[b + 3] = string.Concat(arr[b + 3].Substring(0, fb + end.Length), end3, arr[b + 3].Substring(fb + end.Length));
										arr[a] = string.Concat("{", a1, " ", arr[a + 2], "}<", arr[a]);
										arr[a + 1] = arr[a + 2] = string.Empty;
									} else {
										arr[a] = string.Concat('<', arr[a]);
										arr[a + 1] = arr[a + 2] = string.Empty;
									}
								}
								break;
							}
							bpos = fb + end.Length;
						}
					}
					if (fc > 0) {
						arr[a] = string.Concat("不严谨的html格式，请检查 ", arr[a], " 的结束标签, @", arr[a + 1], "='", arr[a + 2], "' 指令无效. <", arr[a]);
						arr[a + 1] = arr[a + 2] = string.Empty;
					}
				}
				if (arr.Length > 0) tplcode = string.Join(string.Empty, arr);
			}
			return tplcode;
		}
		#endregion
		#region Complie
		private static ITemplateOutput Complie(string cscode, string typename) {
			var assemly = _compiler.Value.CompileCode(cscode);
			var type = assemly.DefinedTypes.Where(a => a.FullName.EndsWith(typename)).FirstOrDefault();
			return Activator.CreateInstance(type) as ITemplateOutput;
		}
		internal static Lazy<CSScriptLib.RoslynEvaluator> _compiler = new Lazy<CSScriptLib.RoslynEvaluator>(() => {
			//var dlls = Directory.GetFiles(Directory.GetParent(Type.GetType("IFreeSql, FreeSql").Assembly.Location).FullName, "*.dll");
			var compiler = new CSScriptLib.RoslynEvaluator();
			compiler.DisableReferencingFromCode = false;
			//compiler.DebugBuild = true;
			//foreach (var dll in dlls) {
			//	Console.WriteLine(dll);
			//	var ass = Assembly.LoadFile(dll);
			//	compiler.ReferenceAssembly(ass);
			//}
			compiler
				.ReferenceAssemblyOf<IFreeSql>()
				.ReferenceDomainAssemblies();
			return compiler;
		});

		#endregion

		#region Utils
		public class Utils {
			public static string ReplaceSingleQuote(object exp) {
				//将 ' 转换成 "
				string exp2 = string.Concat(exp);
				int quote_pos = -1;
				while (true) {
					int first_pos = quote_pos = exp2.IndexOf('\'', quote_pos + 1);
					if (quote_pos == -1) break;
					while (true) {
						quote_pos = exp2.IndexOf('\'', quote_pos + 1);
						if (quote_pos == -1) break;
						int r_cout = 0;
						for (int p = 1; true; p++) {
							if (exp2[quote_pos - p] == '\\') r_cout++;
							else break;
						}
						if (r_cout % 2 == 0/* && quote_pos - first_pos > 2*/) {
							string str1 = exp2.Substring(0, first_pos);
							string str2 = exp2.Substring(first_pos + 1, quote_pos - first_pos - 1);
							string str3 = exp2.Substring(quote_pos + 1);
							string str4 = str2.Replace("\"", "\\\"");
							quote_pos += str4.Length - str2.Length;
							exp2 = string.Concat(str1, "\"", str4, "\"", str3);
							break;
						}
					}
					if (quote_pos == -1) break;
				}
				return exp2;
			}
			public static string GetConstString(object obj) {
				return string.Concat(obj)
					.Replace("\\", "\\\\")
					.Replace("\"", "\\\"")
					.Replace("\r", "\\r")
					.Replace("\n", "\\n");
			}

			public static string ReadTextFile(string path) {
				byte[] bytes = ReadFile(path);
				return Encoding.UTF8.GetString(bytes).TrimStart((char)65279);
			}
			public static byte[] ReadFile(string path) {
				if (File.Exists(path)) {
					string destFileName = Path.GetTempFileName();
					File.Copy(path, destFileName, true);
					int read = 0;
					byte[] data = new byte[1024];
					using (MemoryStream ms = new MemoryStream()) {
						using (FileStream fs = new FileStream(destFileName, FileMode.OpenOrCreate, FileAccess.Read)) {
							do {
								read = fs.Read(data, 0, data.Length);
								if (read <= 0) break;
								ms.Write(data, 0, read);
							} while (true);
						}
						File.Delete(destFileName);
						data = ms.ToArray();
					}
					return data;
				}
				return new byte[] { };
			}
			public static string TranslateUrl(string url) {
				return TranslateUrl(url, null);
			}
			public static string TranslateUrl(string url, string baseDir) {
				if (string.IsNullOrEmpty(baseDir)) baseDir = AppContext.BaseDirectory + "/";
				if (string.IsNullOrEmpty(url)) return Path.GetDirectoryName(baseDir);
				if (url.StartsWith("~/")) url = url.Substring(1);
				if (url.StartsWith("/")) return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(baseDir), url.TrimStart('/')));
				if (url.StartsWith("\\")) return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(baseDir), url.TrimStart('\\')));
				if (url.IndexOf(":\\") != -1) return url;
				return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(baseDir), url));
			}
		}
		#endregion
	}
}
