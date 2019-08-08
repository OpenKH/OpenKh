using System.Collections.Generic;
using System.IO;
using OpenKh.Kh2;
using OpenKh.Kh2.Battle;
using OpenKh.Tools.Kh2BattleEditor.Extensions;
using OpenKh.Tools.Kh2BattleEditor.Interfaces;
using Xe.Tools;

namespace OpenKh.Tools.Kh2BattleEditor.ViewModels
{
    public class EnmpViewModel : BaseNotifyPropertyChanged, IBattleGetChanges
    {
        private readonly BaseBattle<Enmp> _enmp;

        public EnmpViewModel(IEnumerable<Bar.Entry> entries)
        {
            _enmp = Enmp.Read(entries.GetBattleStream(EntryName));
        }

        public EnmpViewModel()
        {
            _enmp = new BaseBattle<Enmp>
            {
                Items = new Enmp[0]
            };
        }

        public string EntryName => "enmp";

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            _enmp.Write(stream);

            return stream;
        }

        public string Foo { get; set; } = "Hello world!";
    }
}
