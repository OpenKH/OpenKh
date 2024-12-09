using OpenKh.Tools.Kh2MsetMotionEditor.Models.Presets;
using System.IO;
using System.Xml.Serialization;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class LoadXmlUsecase
    {
        private readonly GenerateXsdUsecase _generateXsdUsecase;

        public LoadXmlUsecase(
            GenerateXsdUsecase generateXsdUsecase
        )
        {
            _generateXsdUsecase = generateXsdUsecase;
        }

        public RootElement LoadXmlOrCreateNewOne<RootElement>(
            string xmlFile,
            string? alsoXsd
        )
            where RootElement : new()
        {
            if (alsoXsd != null && !File.Exists(alsoXsd))
            {
                File.WriteAllBytes(
                    alsoXsd,
                    _generateXsdUsecase
                        .GenerateXsdFrom(typeof(MdlxMsetPresets))
                        .ToArray()
                );
            }

            if (File.Exists(xmlFile))
            {
                using var stream = File.OpenRead(xmlFile);
                return (RootElement)new XmlSerializer(typeof(RootElement))
                    .Deserialize(stream)!;
            }
            else
            {
                using var stream = File.Create(xmlFile);
                var root = new RootElement();
                new XmlSerializer(typeof(RootElement))
                    .Serialize(stream, root);

                return root;
            }
        }
    }
}
