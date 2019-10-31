using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FreeSql.Extensions.LazyLoading
{

    public class LazyLoadingComplier
    {

#if ns20
        internal static Lazy<CSScriptLib.RoslynEvaluator> _compiler = new Lazy<CSScriptLib.RoslynEvaluator>(() =>
        {
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

        public static Assembly CompileCode(string cscode)
        {
            return _compiler.Value.CompileCode(cscode);
        }
#else


		public static Assembly CompileCode(string cscode) {

			using (var compiler = CodeDomProvider.CreateProvider("cs")) {

				var objCompilerParameters = new CompilerParameters();
				objCompilerParameters.ReferencedAssemblies.Add("System.dll");
				objCompilerParameters.ReferencedAssemblies.Add("FreeSql.dll");
				objCompilerParameters.GenerateExecutable = false;
				objCompilerParameters.GenerateInMemory = true;

				CompilerResults cr = compiler.CompileAssemblyFromSource(objCompilerParameters, cscode);

				if (cr.Errors.Count > 0)
					throw new Exception(cr.Errors[0].ErrorText);

				return cr.CompiledAssembly;
			}
		}

#endif
    }
}
