using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.Services
{
    public class ClipboardService
    {
        // Note: Objects are copied as bytes or string in order to not copy the reference

        private Stream _motion { get; set; }
        public void StoreMotion(Bar.Entry motion)
        {
            motion.Stream.Position = 0;
            _motion = new MemoryStream();
            motion.Stream.CopyTo(_motion);
            _motion.Position = 0;
            motion.Stream.Position = 0;
        }
        public Stream FetchMotion()
        {
            if (_motion == null)
                return null;

            Stream copy = new MemoryStream();
            _motion.CopyTo(copy);
            copy.Position = 0;
            _motion.Position = 0;

            return copy;
        }

        private Stream _dpd { get; set; }
        public void StoreDpd(Dpd dpd)
        {
            _dpd = dpd.getAsStream();
        }
        public Dpd FetchDpd()
        {
            if (_dpd == null)
                return null;

            return new Dpd(_dpd);
        }

        private string _collision { get; set; }
        public void StoreCollision(ObjectCollision collision)
        {
            _collision = System.Text.Json.JsonSerializer.Serialize(collision);
        }
        public ObjectCollision FetchCollision()
        {
            if (_collision == null)
                return null;

            return System.Text.Json.JsonSerializer.Deserialize<ObjectCollision>(_collision);
        }

        private List<string> _collisionGroup { get; set; }
        public void StoreCollisionGroup(List<ObjectCollision> collisions)
        {
            _collisionGroup = new List<string>();
            foreach (ObjectCollision collision in collisions)
            {
                _collisionGroup.Add(System.Text.Json.JsonSerializer.Serialize(collision));
            }
        }
        public List<ObjectCollision> FetchCollisionGroup()
        {
            if (_collisionGroup == null)
                return null;

            List<ObjectCollision> returnCollisions = new List<ObjectCollision>();
            foreach (string collisionJson in _collisionGroup)
            {
                returnCollisions.Add(System.Text.Json.JsonSerializer.Deserialize<ObjectCollision>(collisionJson));
            }
            return returnCollisions;
        }



        // SINGLETON
        private ClipboardService() { }
        private static ClipboardService _instance = null;
        public static ClipboardService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ClipboardService();
                }
                return _instance;
            }
        }
        public static void Reset()
        {
            _instance = new ClipboardService();
        }
    }
}
