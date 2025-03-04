using OpenKh.Kh2.Ard;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class EventsXmlRootTest
    {
        [Fact]
        public void GenerateXsd()
        {
            // Copied from https://stackoverflow.com/a/24181625

            var schemas = new XmlSchemas();
            var exporter = new XmlSchemaExporter(schemas);
            var mapping = new XmlReflectionImporter().ImportTypeMapping(typeof(EventsXmlRoot));
            exporter.ExportTypeMapping(mapping);
            var writer = new MemoryStream();
            var xmlWriter = XmlWriter.Create(
                writer,
                new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    IndentChars = " ",
                }
            );

            foreach (XmlSchema schema in schemas)
            {
                schema.Write(xmlWriter);
            }
            File.WriteAllBytes("EventsXmlRoot.xsd", writer.ToArray());
        }
    }
}
