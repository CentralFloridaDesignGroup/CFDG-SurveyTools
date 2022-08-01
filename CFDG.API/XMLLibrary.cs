using System;
using System.IO;
using System.Xml.Linq;

namespace CFDG.API
{
    /// <summary>
    /// Internal XML Handling including reading and writing
    /// </summary>
    /// 
    [Obsolete("Use the Setting class", true)]
    public class XML
    {
        #region Properties
        private static readonly string xmlFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\AppSettings.xml";

        private static XElement XmlElement { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Read the value of <paramref name="key"/> in the <paramref name="category"/>
        /// </summary>
        /// <param name="category">Setting category (case-insesitive).</param>
        /// <param name="key">Key name (case-insesitive).</param>
        /// <returns>Value or null if not found.</returns>
        public static dynamic ReadValue(string category, string key)
        {
            if (XmlElement == null)
            {
                XmlElement = XDocument.Load(xmlFile).Root;
            }
            category = category.ToLower();
            key = key.ToLower();

            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(key))
            {
                return null;
            }

            string[] value = RetreiveValue(category, key);
            if (value == null)
            {
                return null;
            }

            return ConvertStringToType(value);
        }
        #endregion

        #region Private Methods

        private static string[] RetreiveValue(string category, string key)
        {
            try
            {
                string value = XmlElement
                    .Element(category)
                    .Element(key)
                    .Attribute("value")
                    .Value;
                string type = XmlElement
                    .Element(category)
                    .Element(key)
                    .Attribute("type")
                    .Value;
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }
                return new string[] { value, type };
            }
            catch
            {
                return null;
            }
        }

        private static dynamic ConvertStringToType(string[] content)
        {
            string value = content[0];
            string type = content[1];

            switch (type.ToLower())
            {
                case "int":
                {
                    if (int.TryParse(value, out int convert))
                    {
                        return convert;
                    }
                    return null;
                }
                case "bool":
                {
                    if (bool.TryParse(value, out bool convert))
                    {
                        return convert;
                    }
                    return null;
                }
                case "string":
                default:
                    return value;
            }
        }
        #endregion
    }
}
