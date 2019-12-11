using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FreeSql.Generator
{
    public class Program
    {
        static void Main(string[] args)
        {
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
