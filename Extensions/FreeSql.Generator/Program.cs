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
            if (args != null && args.Length == 0) args = new[] { "?" };
            
            // 根据系统语言自动检测
            var culture = CultureInfo.CurrentUICulture.Name.ToLower();
            CoreErrorStrings.Language = (culture.StartsWith("zh") || culture.StartsWith("cn")) ? "cn" : "en";
            
            // 处理 -Lang 参数（覆盖系统语言）
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Trim().ToLower() == "-lang" && i + 1 < args.Length)
                {
                    var lang = args[i + 1].Trim().ToLower();
                    CoreErrorStrings.Language = (lang == "cn" || lang == "zh" || lang == "chinese") ? "cn" : "en";
                    // 移除语言参数
                    args = args.Where((val, idx) => idx != i && idx != i + 1).ToArray();
                    break;
                }
            }
            
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
