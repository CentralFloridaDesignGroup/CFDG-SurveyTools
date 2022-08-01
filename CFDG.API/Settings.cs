using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CFDG.API
{
    public static class Settings
    {
        private static readonly string _baseSettingFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\AppSettings.xml";

        private static Dictionary<string, string> _keys;

        public static T GetValue<T>(string key)
        {
            if (_keys == null)
            {
                Logging.Debug("Initalizing keys");
                Initalize();
            }
            if (!_keys.ContainsKey(key))
            {
                Logging.Error($"Could not find the key \"{key}\"");
                return default;
            }

            string value = _keys[key];

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static void Initalize()
        {
            _keys = new Dictionary<string, string>();
            if (!File.Exists(_baseSettingFile))
            {
                Logging.Error("The settings file could not be found.");
                return;
            }

            XDocument xDoc = XDocument.Load(_baseSettingFile);
            XElement xElement = xDoc.Root;
            if (xElement == null)
            {
                Logging.Error("The settings document could not be parsed");
                return;
            }

            Logging.Debug("Processing elements.");
            ProcessElement(xElement, "");
            if (!string.IsNullOrEmpty(xElement.Attribute("Override").Value))
            {
                HandleOverrideFile(xElement.Attribute("Override").Value);
            }
        }

        private static void HandleOverrideFile(string value)
        {
            if (!File.Exists(value))
            {
                return;
            }
            Logging.Info("Processing override file.");

            XDocument xDoc = XDocument.Load(value);
            XElement xElement = xDoc.Root;
            if (xElement == null)
            {
                Logging.Error("The settings document could not be parsed");
                return;
            }

            Logging.Debug("Processing elements.");
            ProcessElement(xElement, "");
        }

        private static void ProcessElement(XElement xElement, string root)
        {
            foreach (XElement element in xElement.Elements())
            {

                string key = string.IsNullOrEmpty(root) ? element.Name.ToString() : root + "." + element.Name;
                Logging.Debug($"Key: " + key);
                if (!element.HasElements)
                {
                    Logging.Debug("Added key to keys");
                    if (_keys.ContainsKey(key))
                    {
                        _keys[key] = element.Value;
                        continue;
                    }
                    _keys.Add(key, element.Value);
                    continue;
                }
                Logging.Debug("Processing subkey");
                ProcessElement(element, key);
            }
        }
    }
}
