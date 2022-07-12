using System;
using System.IO;
using CFDG.API;

namespace LidarCompiler
{
    public class Program
    {
        private static string _indexFile;

        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Logging.Error("There were too many arguments passed to the application.");
                ExitApplication(-1001);
            }
            if (args.Length == 1)
            {
                if (File.Exists(args[0]))
                {
                    _indexFile = args[0];
                }
            }
            if (_indexFile is null)
            {
                string baseFolder = UserInput.GetString("Please enter the base folder to compile a list of valid folders:");
                if (!Directory.Exists(baseFolder))
                {
                    Logging.Error("Could not get a valid answer.");
                    ExitApplication(-1002);
                }
                GatherLidarFolders(baseFolder);
                if (_indexFile is null)
                {
                    Logging.Error("No reference file was provided. Cannot continue.");
                    ExitApplication(-1003);
                }
            }
            GatherIndexInformation();
        }

        private static void GatherIndexInformation()
        {
            string tempIndex = Path.Combine(Directory.GetParent(_indexFile).FullName, $"{Path.GetRandomFileName()}.idx");
            foreach (string directory in File.ReadLines(_indexFile))
            {
                ProcessDirectory(tempIndex, directory);
            }
        }

        private static void ProcessDirectory(string tempIndex, string directory)
        {
            string[] parts = directory.Split(',');
            string directoryStr = parts[0];
            foreach (string file in Directory.GetFiles(directoryStr))
            {
                Lidar lidar = new Lidar(file);
                File.AppendAllText(tempIndex, $"{file},{parts[1]},{lidar.Meta.NorthBound:0.000},{lidar.Meta.EastBound:0.000},{lidar.Meta.SouthBound:0.000},{lidar.Meta.WestBound:0.000}{Environment.NewLine}");
            }
        }

        private static void GatherLidarFolders(string baseDir)
        {
            _indexFile = Path.Combine(baseDir, $"relevant_files_{DateTime.Now:yyMMdd_HHmmss}.txt");
            CheckFolder(baseDir);
        }

        private static void CheckFolder(string directory)
        {
            Logging.Info($"Checking \"{directory}\"");
            StatePlaneZone zone = StatePlaneZone.EF;
            var lidarFiles = Directory.GetFiles(directory, "*.la?");
            if (lidarFiles.Length > 0)
            {
                Console.Clear();
                Logging.Info($"Lidar files found");
                (bool status, ConsoleKey key) = UserInput.GetKey($"{directory}{Environment.NewLine}" +
                    $"Please enter a zone:{Environment.NewLine}" +
                    $"1. 901 - Florida East{Environment.NewLine}" +
                    $"2. 902 - Florida West{Environment.NewLine}" +
                    $"3. 903 - Florida North", ConsoleKey.D1, ConsoleKey.NumPad1, ConsoleKey.D2, ConsoleKey.NumPad2, ConsoleKey.D3, ConsoleKey.NumPad3);
                if (status)
                {
                    switch (key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                        default: { zone = StatePlaneZone.EF; break; }
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2: { zone = StatePlaneZone.WF; break; }
                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3: { zone = StatePlaneZone.NF; break; }
                    }
                }
                File.AppendAllText(_indexFile, $"{directory},{zone}{Environment.NewLine}");
                Logging.Info($"Directory added to reference file.");
            } else
            {
                Logging.Warning($"No lidar files found");
            }
            foreach (string subDir in Directory.GetDirectories(directory))
            {
                CheckFolder(subDir);
            }
        }

        static void ExitApplication(int errorCode)
        {
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            Environment.Exit(errorCode);
        }

        private enum StatePlaneZone
        {
            EF = 901,
            WF = 902,
            NF = 903
        };
    }
}