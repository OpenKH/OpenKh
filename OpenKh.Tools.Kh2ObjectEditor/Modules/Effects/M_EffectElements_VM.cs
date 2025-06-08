using OpenKh.Kh2;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    internal class M_EffectElements_VM
    {
        internal List<Pax.Element> OriginalElements;
        public ObservableCollection<M_EffectElements_Wrapper> LoadedEntries { get; set; } = new ObservableCollection<M_EffectElements_Wrapper>();

        public void LoadEntries(List<Pax.Element> paxElements)
        {
            OriginalElements = paxElements;

            LoadedEntries.Clear();
            foreach (Pax.Element element in OriginalElements)
            {
                LoadedEntries.Add(M_EffectElements_Wrapper.Wrap(element));
            }
        }

        public void SaveEntries()
        {
            if(OriginalElements == null)
            {
                throw new System.Exception("[M_EffectElements_VM] original entries not loaded");
            }

            OriginalElements.Clear();
            foreach (M_EffectElements_Wrapper wrapper in LoadedEntries)
            {
                OriginalElements.Add(wrapper.Unwrap());
            }
        }
    }
}
