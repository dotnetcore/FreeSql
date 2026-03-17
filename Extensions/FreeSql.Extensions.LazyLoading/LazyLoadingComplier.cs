using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace FreeSql.Extensions.LazyLoading
{

    public class LazyLoadingComplier
    {

#if ns20
		//public static Assembly CompileCode(string cscode)
		//{
		//    Natasha.AssemblyComplier complier = new Natasha.AssemblyComplier();
		//    //complier.Domain = DomainManagment.Random;
		//    complier.Add(cscode);
		//    return complier.GetAssembly();
		//}

		/*2026-3-13：应用单文件发布模式，导航功能报错。
		internal static Lazy<CSScriptLib.RoslynEvaluator> _compiler = new Lazy<CSScriptLib.RoslynEvaluator>(() =>
		{
			var compiler = new CSScriptLib.RoslynEvaluator();
			compiler.DisableReferencingFromCode = false;
			compiler
				.ReferenceAssemblyOf<IFreeSql>()
				.ReferenceDomainAssemblies();
			return compiler;
		});

		public static Assembly CompileCode(string cscode)
		{
			return _compiler.Value.CompileCode(cscode);
		}
		*/

		//2026-3-13：删除CS-Script.Core库，改用官方的库 Microsoft.CodeAnalysis.CSharp。
		private static readonly HashSet<MetadataReference> references = new();
		static LazyLoadingComplier()
		{
			foreach (var eve in AppDomain.CurrentDomain.GetAssemblies().AsParallel())
			{
				var ass = CreateMetadataReference(eve);
				if (ass != null)
					references.Add(ass);
			}
			references.Add(CreateMetadataReference(typeof(String).Assembly));
			references.Add(CreateMetadataReference(Assembly.GetEntryAssembly()));
			references.Add(CreateMetadataReference(Assembly.GetCallingAssembly()));
			references.Add(CreateMetadataReference(Assembly.GetExecutingAssembly()));
			references.Add(CreateMetadataReference(typeof(FreeSql.FreeSqlBuilder).Assembly));
		}
		public static Assembly CompileCode(string cscode)
		{
			var tree = CSharpSyntaxTree.ParseText(cscode);
			using var ms = new MemoryStream();
			var result = CSharpCompilation.Create("DynamicAssembly")
				.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, reportSuppressedDiagnostics: false))
				.AddReferences(references)
				.AddSyntaxTrees(tree).Emit(ms);
			if (result.Success)
			{
				return Assembly.Load(ms.ToArray());
			}
			else
			{
				throw new Exception(string.Join(Environment.NewLine, from eve in result.Diagnostics select eve.ToString()));
			}
		}
		public static MetadataReference CreateMetadataReference(Assembly assembly)
		{
			if (!string.IsNullOrEmpty(assembly.Location))
				return MetadataReference.CreateFromFile(assembly.Location);

			unsafe
			{   // 纯内存 Assembly（.NET 5+）
				if (assembly.TryGetRawMetadata(out byte* blob, out int length))
				{
					var moduleMetadata = ModuleMetadata.CreateFromMetadata((IntPtr)blob, length);
					return AssemblyMetadata.Create(moduleMetadata).GetReference();
				}
			}
			return null;
		}

#else

        public static Assembly CompileCode(string cscode)
        {

            var files = Directory.GetFiles(Directory.GetParent(Type.GetType("IFreeSql, FreeSql").Assembly.Location).FullName);
            using (var compiler = CodeDomProvider.CreateProvider("cs"))
            {
                var objCompilerParameters = new CompilerParameters();
                objCompilerParameters.ReferencedAssemblies.Add("System.dll");
                objCompilerParameters.ReferencedAssemblies.Add("System.Core.dll");
                objCompilerParameters.ReferencedAssemblies.Add("FreeSql.dll");
                foreach (var dll in files)
                {
                    if (!dll.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) &&
                        !dll.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) continue;

                    //Console.WriteLine(dll);
                    var dllName = string.Empty;
                    var idx = dll.LastIndexOf('/');
                    if (idx != -1) dllName = dll.Substring(idx + 1);
                    else
                    {
                        idx = dll.LastIndexOf('\\');
                        if (idx != -1) dllName = dll.Substring(idx + 1);
                    }
                    if (string.IsNullOrEmpty(dllName)) continue;
                    try
                    {
                        var ass = Assembly.LoadFile(dll);
                        objCompilerParameters.ReferencedAssemblies.Add(dllName);
                    }
                    catch
                    {

                    }
                }
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
