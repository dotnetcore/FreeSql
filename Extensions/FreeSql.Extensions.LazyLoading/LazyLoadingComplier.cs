using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;

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
