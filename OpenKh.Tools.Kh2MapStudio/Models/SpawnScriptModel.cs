using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Tools.Kh2MapStudio.Models
{
    class SpawnScriptModel
    {
        private readonly string _name;
        private List<SpawnScript> _spawnScripts;

        SpawnScriptModel(string name, List<SpawnScript> spawnScripts)
        {
            _name = name;
            _spawnScripts = spawnScripts;
            Decompiled = SpawnScriptParser.Decompile(spawnScripts);
        }

        public IEnumerable<int> ProgramIds => _spawnScripts.Select(x => (int)x.ProgramId);
        public string Decompiled { get; set; }
        public bool IsError { get; set; }
        public string LastError { get; set; }

        public void Compile()
        {
            try
            {
                IsError = false;
                LastError = "Success!";
                _spawnScripts = SpawnScriptParser.Compile(Decompiled).ToList();
            }
            catch (SpawnScriptParserException ex)
            {
                IsError = true;
                LastError = ex.Message;
            }
        }

        public IEnumerable<Bar.Entry> SaveToBar(IEnumerable<Bar.Entry> entries)
        {
            var memStream = new MemoryStream();
            SpawnScript.Write(memStream, _spawnScripts);

            return entries.AddOrReplace(new Bar.Entry
            {
                Name = _name,
                Type = Bar.EntryType.SpawnScript,
                Stream = memStream
            });
        }

        public static SpawnScriptModel Create(List<Bar.Entry> entries, string name)
        {
            var entry = entries
                .FirstOrDefault(x => x.Name == name && x.Type == Bar.EntryType.SpawnScript);
            if (entry == null)
                return null;

            return new SpawnScriptModel(name, SpawnScript.Read(entry.Stream));
        }
    }
}
