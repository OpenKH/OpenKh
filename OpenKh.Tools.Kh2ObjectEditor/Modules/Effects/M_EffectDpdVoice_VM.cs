using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public class M_EffectDpdVoice_VM : NotifyPropertyChangedBase
    {
        public DpdVsf Voice { get; set; }

        public int VsfNo
        {
            get { return Voice.VsfNo; }
            set
            {
                Voice.VsfNo = value;
                OnPropertyChanged("VsfNo");
            }
        }
        public int ModelNumber
        {
            get { return Voice.ModelNumber; }
            set
            {
                Voice.ModelNumber = value;
                OnPropertyChanged("ModelNumber");
            }
        }

        private ObservableCollection<IndexWrapper> _indices;
        public ObservableCollection<IndexWrapper> Indices
        {
            get { return _indices; }
            set
            {
                _indices = value;
                OnPropertyChanged("Indices");
            }
        }

        public M_EffectDpdVoice_VM(DpdVsf vsf)
        {
            Voice = vsf;
            _indices = new ObservableCollection<IndexWrapper>();
            foreach (ushort value in Voice.Indices)
            {
                _indices.Add(new IndexWrapper { Index = value });
            }
        }

        public void SaveIndices()
        {
            Voice.Indices.Clear();
            foreach (IndexWrapper value in Indices)
            {
                Voice.Indices.Add(value.Index);
            }
            Debug.WriteLine("Indices saved!");
        }

        public class IndexWrapper
        {
            public ushort Index { get; set; }
        }
    }
}
