using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Collisions
{
    public class M_Collisions_VM
    {
        public ObservableCollection<ObjectCollision> Collisions { get; set; }

        public M_Collisions_VM()
        {
            Collisions = new ObservableCollection<ObjectCollision>();
            reloadCollisions();
        }

        public void reloadCollisions()
        {
            Collisions.Clear();
            foreach(ObjectCollision coll in MdlxService.Instance.CollisionFile.EntryList)
            {
                Collisions.Add(coll);
            }
        }
        public void saveCollisions()
        {
            MdlxService.Instance.CollisionFile.EntryList.Clear();
            foreach (ObjectCollision coll in Collisions)
            {
                MdlxService.Instance.CollisionFile.EntryList.Add(coll);
            }
        }


        public void copyCollision(ObjectCollision collision)
        {
            ClipboardService.Instance.StoreCollision(collision);
        }
        public void copyCollisionGroup(int group)
        {
            List<ObjectCollision> collisions = new List<ObjectCollision>();
            foreach (ObjectCollision collision in MdlxService.Instance.CollisionFile.EntryList)
            {
                if (collision.Group == group)
                {
                    collisions.Add(collision);
                }
            }

            ClipboardService.Instance.StoreCollisionGroup(collisions);
        }

        public void replaceCollision(int index)
        {
            if (ClipboardService.Instance.FetchCollision() == null)
                return;

            MdlxService.Instance.CollisionFile.EntryList[index] = ClipboardService.Instance.FetchCollision();
            reloadCollisions();
        }
        public void addCopiedGroup()
        {
            if (ClipboardService.Instance.FetchCollisionGroup() == null)
                return;

            foreach (ObjectCollision collision in ClipboardService.Instance.FetchCollisionGroup())
            {
                MdlxService.Instance.CollisionFile.EntryList.Add(collision);
            }
            reloadCollisions();
        }

        public void addCollision()
        {
            MdlxService.Instance.CollisionFile.EntryList.Add(new ObjectCollision());
            reloadCollisions();
        }
        public void removeCollision(int index)
        {
            MdlxService.Instance.CollisionFile.EntryList.RemoveAt(index);
            reloadCollisions();
        }
    }
}
