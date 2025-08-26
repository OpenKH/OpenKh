using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Collisions
{
    public class M_Collisions_VM
    {
        public ObservableCollection<ObjectCollision> Collisions { get; set; }

        public List<string> CollisionShapeOptions => new CollisionShapeConverter().Options.Values.ToList();
        public List<string> CollisionTypeOptions => new CollisionTypeConverter().Options.Values.ToList();

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

        // Tests the collisions ingame.
        // IMPORTANT: the Collisions must be of the same length, be careful because there's no control of this.
        public void TestCollisionsIngame()
        {
            string filename = Path.GetFileName(MdlxService.Instance.MdlxPath);

            if (filename == "")
                return;

            long fileAddress;
            try
            {
                fileAddress = ProcessService.getAddressOfFile(filename);
            }
            catch (Exception exc)
            {
                System.Windows.Forms.MessageBox.Show("Game is not running", "There was an error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            if (fileAddress == 0)
            {
                System.Windows.Forms.MessageBox.Show("Couldn't find file", "There was an error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            int entryOffset = -1;
            foreach (Bar.Entry entry in MdlxService.Instance.MdlxBar)
            {
                if (entry.Type == Bar.EntryType.ModelCollision)
                {
                    entryOffset = entry.Offset;
                    break;
                }
            }
            if (entryOffset == -1)
            {
                System.Windows.Forms.MessageBox.Show("AI file not found", "There was an error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            long collisionFileAddress = fileAddress + entryOffset;

            MemoryStream collisionStream = (MemoryStream)MdlxService.Instance.CollisionFile.toStream();
            byte[] streamBytes = collisionStream.ToArray();
            MemoryAccess.writeMemory(ProcessService.KH2Process, collisionFileAddress, streamBytes, true);
        }
    }
}
