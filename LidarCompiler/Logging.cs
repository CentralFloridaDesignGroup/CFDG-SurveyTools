using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LidarCompiler
{
    internal class Logging
    {
        public static void Debug(string message)
        {
#if DEBUG
            WriteMessage(message, LogLevel.Debug, true);
#endif
        }

        public static void Info(string message)
        {
            WriteMessage(message, LogLevel.Info, true);
        }

        public static void Warning(string message)
        {
            WriteMessage(message, LogLevel.Warning, true);
        }

        public static void Error(string message)
        {
            WriteMessage(message, LogLevel.Error, true);
        }

        private static void WriteMessage(string message, LogLevel logLevel, bool writeLevel)
        {
            ConsoleColor color;
            switch (logLevel)
            {
                case LogLevel.Debug: color = ConsoleColor.Green; break;
                case LogLevel.Warning: color = ConsoleColor.Yellow; break;
                case LogLevel.Error: color = ConsoleColor.Red; break;
                case LogLevel.Info:
                default: color = ConsoleColor.White; break;
            }
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:MM-dd-yy HH-mm-ss}]{(writeLevel ? $"[{logLevel}]" : "")}: {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        internal enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }
    }
}
