using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LidarCompiler
{
    internal class UserInput
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string GetString(string message)
        {
            Console.WriteLine($"{message}");
            string result = Console.ReadLine();
            if (string.IsNullOrEmpty(result))
            {
                _log.Error("Response cannot be empty, please try again.");
                return GetString(message);
            }
            return result;
        }

        public static (bool, ConsoleKey) GetKey(string message, params ConsoleKey[] allowedValues)
        {
            Console.WriteLine(message);
            ConsoleKey result = Console.ReadKey().Key;
            _log.Debug($"Key Pressed: {result}");
            if (result == ConsoleKey.Escape)
            {
                return (false, result);
            }
            if (result == ConsoleKey.Enter)
            {
                return (true, allowedValues[0]);
            }
            if (allowedValues.Contains(result))
            {
                return (true, result);
            }
            _log.Error("An invalid key was pressed. Please try again.");
            return GetKey(message, allowedValues);
        }
    }
}
