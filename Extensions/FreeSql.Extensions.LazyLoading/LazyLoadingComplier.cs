using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FreeSql.Extensions.LazyLoading {

	public class LazyLoadingComplier {

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

		public static Assembly CompileCode(string cscode) {
			return _compiler.Value.CompileCode(cscode);
		}
	}
}
