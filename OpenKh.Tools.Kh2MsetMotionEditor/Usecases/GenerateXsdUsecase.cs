using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class GenerateXsdUsecase
    {
        public MemoryStream GenerateXsdFrom(Type type)
        {
            // Copied from https://stackoverflow.com/a/24181625

            var schemas = new XmlSchemas();
            var exporter = new XmlSchemaExporter(schemas);
            var mapping = new XmlReflectionImporter().ImportTypeMapping(type);
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

            foreach (XmlSchema schema in schemas.Take(1))
            {
                schema.Write(xmlWriter);
            }

            xmlWriter.Flush();

            return writer;
        }
    }
}
