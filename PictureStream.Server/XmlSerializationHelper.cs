using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PictureStream.Server
{
    public static class XmlSerializationHelper
    {
        public static T Deserialize<T>(string xmlContents)
        {
            // Create a serializer
            using (StringReader s = new StringReader(xmlContents))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(s);
            }
        }

        public static string Serialize<T>(T value)
        {
            if (value == null)
                return null;

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
            settings.Indent = false;
            settings.OmitXmlDeclaration = false;

            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }
                return textWriter.ToString();
            }
        }
    }

}
