using OpenKh.Bbs;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.BbsEventTableEditor.ViewModels
{
    public class EventsViewModel : GenericListModel<EventViewModel>
    {
        public EventsViewModel() :
            this(new Event[0])
        { }

        public EventsViewModel(IEnumerable<Event> events) :
            base(events.Select(x => new EventViewModel(x)))
        {

        }

        public Visibility GuideVisibility => IsItemSelected ? Visibility.Collapsed : Visibility.Visible;
        public Visibility EditVisibility => IsItemSelected ? Visibility.Visible : Visibility.Collapsed;

        protected override void OnSelectedItem(EventViewModel item)
        {
            OnPropertyChanged(nameof(GuideVisibility));
            OnPropertyChanged(nameof(EditVisibility));
            base.OnSelectedItem(item);
        }

        protected override EventViewModel OnNewItem() =>
            new EventViewModel(new Event());
    }
}
