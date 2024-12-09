using System.Collections.Generic;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class EditCollectionNoErrorUsecase
    {
        public bool DeleteAt<T>(List<T>? list, int index)
        {
            if (list != null && (uint)index < (uint)list.Count)
            {
                list.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Append<T>(List<T>? list, T item)
        {
            if (list != null)
            {
                list.Add(item);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InsertAt<T>(List<T>? list, int index, T item)
        {
            if (list != null && (uint)index <= (uint)list.Count)
            {
                list.Insert(index, item);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
