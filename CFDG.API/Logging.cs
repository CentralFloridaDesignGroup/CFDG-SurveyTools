using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFDG.API
{
    public static class Logging
    {
        public static void Debug(string message)
        {
            WriteMessage($"[Debug][{DateTime.Now:HH-mm-ss.ff}]: {message}");
        }

        public static void Info(string message)
        {
            WriteMessage($"[Info][{DateTime.Now:HH-mm-ss.ff}]: {message}");
        }

        public static void Warning(string message)
        {
            WriteMessage($"[Warning][{DateTime.Now:HH-mm-ss.ff}]: {message}");
        }

        public static void Error(string message)
        {
            WriteMessage($"[Error][{DateTime.Now:HH-mm-ss.ff}]: {message}");
            throw new Exception(message);
        }


        private static void WriteMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
#if DEBUG
                throw new ArgumentNullException("The message was null or empty.");
#endif

            }

            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
