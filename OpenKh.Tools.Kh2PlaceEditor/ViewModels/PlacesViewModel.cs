using OpenKh.Engine;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.Kh2PlaceEditor.ViewModels
{
    public class PlacesViewModel
    {
        private readonly IMessageProvider _messageProvider;

        public PlacesViewModel(IMessageProvider messageProvider, Dictionary<string, List<Place>> places)
        {
            _messageProvider = messageProvider;
            Items = new GenericListModel<PlaceViewModel>(places
                .Select(x => new
                {
                    World = x.Key,
                    Places = x.Value.Select((place, i) => new
                    {
                        Index = i,
                        Place = place
                    })
                })
                .SelectMany(x => x.Places, (x, place) => new PlaceViewModel(_messageProvider, x.World, place.Index, place.Place))
            );
        }


        public GenericListModel<PlaceViewModel> Items { get; }

        public Dictionary<string, List<Place>> Places => Items
            .GroupBy(x => x.World)
            .ToDictionary(x => x.Key, x => x.Select(vm => vm.Place).ToList());

        public void RefreshAllMessages()
        {
            foreach (var item in Items)
                (item as PlaceViewModel)?.RefreshMessages();
        }
    }
}
