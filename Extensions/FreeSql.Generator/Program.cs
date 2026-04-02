using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace FreeSql.Generator
{
    public class Program
    {
        static void Main(string[] args)
        {
            var culture = CultureInfo.CurrentUICulture.Name.ToLower();
            CoreErrorStrings.Language = (culture.StartsWith("zh") || culture.StartsWith("cn")) ? "cn" : "en";

            if (args != null && args.Length > 0)
            {
                var argList = args.ToList();
                var langIndex = argList.FindIndex(arg => string.Equals(arg, "-lang", StringComparison.OrdinalIgnoreCase));
                if (langIndex >= 0 && langIndex + 1 < argList.Count)
                {
                    var lang = argList[langIndex + 1].Trim().ToLower();
                    CoreErrorStrings.Language = (lang == "cn" || lang == "zh" || lang == "chinese") ? "cn" : "en";
                    argList.RemoveAt(langIndex);
                    argList.RemoveAt(langIndex);
                    args = argList.ToArray();
                }
            }

            if (args != null && args.Length == 0) args = new[] { "?" };

            ManualResetEvent wait = new ManualResetEvent(false);
            new Thread(() => {
                Thread.CurrentThread.Join(TimeSpan.FromSeconds(1));
                ConsoleApp app = new ConsoleApp(args, wait);
            }).Start();
            wait.WaitOne();
            return;
        }
    }
}
