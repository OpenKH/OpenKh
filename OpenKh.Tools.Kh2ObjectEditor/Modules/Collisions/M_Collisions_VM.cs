using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Collisions
{
    public class M_Collisions_VM
    {
        public ObservableCollection<ObjectCollision> Collisions { get; set; }
        // Note: Copied as JSON in order to not copy the reference

        public M_Collisions_VM()
        {
            Collisions = new ObservableCollection<ObjectCollision>();
            reloadCollisions();
        }

        public void reloadCollisions()
        {
            Collisions.Clear();
            foreach(ObjectCollision coll in Mdlx_Service.Instance.CollisionFile.EntryList)
            {
                Collisions.Add(coll);
            }
        }
        public void saveCollisions()
        {
            Mdlx_Service.Instance.CollisionFile.EntryList.Clear();
            foreach (ObjectCollision coll in Collisions)
            {
                Mdlx_Service.Instance.CollisionFile.EntryList.Add(coll);
            }
        }


        public void copyCollision(ObjectCollision collision)
        {
            Mdlx_Service.Instance.CopiedCollision = System.Text.Json.JsonSerializer.Serialize(collision);
        }
        public void copyCollisionGroup(int group)
        {
            Mdlx_Service.Instance.CopiedCollisionList = new List<string>();
            foreach(ObjectCollision collision in Mdlx_Service.Instance.CollisionFile.EntryList)
            {
                if(collision.Group == group)
                {
                    Mdlx_Service.Instance.CopiedCollisionList.Add(System.Text.Json.JsonSerializer.Serialize(collision));
                }
            }
        }

        public void replaceCollision(int index)
        {
            if (Mdlx_Service.Instance.CopiedCollision == null)
                return;

            Mdlx_Service.Instance.CollisionFile.EntryList[index] = System.Text.Json.JsonSerializer.Deserialize<ObjectCollision>(Mdlx_Service.Instance.CopiedCollision);
            reloadCollisions();
        }
        public void addCopiedGroup()
        {
            if (Mdlx_Service.Instance.CopiedCollisionList == null)
                return;

            foreach (string collisionJson in Mdlx_Service.Instance.CopiedCollisionList)
            {
                Mdlx_Service.Instance.CollisionFile.EntryList.Add(System.Text.Json.JsonSerializer.Deserialize<ObjectCollision>(collisionJson));
            }
            reloadCollisions();
        }

        public void addCollision()
        {
            Mdlx_Service.Instance.CollisionFile.EntryList.Add(new ObjectCollision());
            reloadCollisions();
        }
        public void removeCollision(int index)
        {
            Mdlx_Service.Instance.CollisionFile.EntryList.RemoveAt(index);
            reloadCollisions();
        }
    }
}
