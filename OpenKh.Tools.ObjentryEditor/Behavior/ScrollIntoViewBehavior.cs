//https://stackoverflow.com/questions/16866309/listbox-scroll-into-view-with-mvvm
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace OpenKh.Tools.ObjentryEditor.Behavior
{
    public class ScrollIntoViewBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            ListBox listBox = AssociatedObject;
            ((INotifyCollectionChanged)listBox.Items).CollectionChanged += OnListBox_CollectionChanged;
        }

        protected override void OnDetaching()
        {
            ListBox listBox = AssociatedObject;
            ((INotifyCollectionChanged)listBox.Items).CollectionChanged -= OnListBox_CollectionChanged;
        }

        private void OnListBox_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ListBox listBox = AssociatedObject;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // scroll the new item into view   
                listBox.ScrollIntoView(e.NewItems[0]);
            }
        }
    }
}
