using System;
using System.IO;

namespace iDeal
{
    public static class Log
    {
        private const string Sep = @"---------------------------------------";
        public static string Append(string log)
        {
            using (var writer = File.AppendText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,@"log.txt")))
            {                
                writer.WriteLine($">>> {DateTime.Now:T} <<<");                
                writer.WriteLine(Sep);
                writer.WriteLine(log);
                writer.WriteLine(Sep);
            }
            Console.WriteLine(log);
            Console.WriteLine(Sep);
            return log;
        }
    }
}
