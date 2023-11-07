using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public class AgeManager
    {
        private int _age = 1;
        private AgeManager? _parent;

        public AgeManager Branch(bool markDirty = false)
        {
            return new AgeManager
            {
                _parent = this,
                _age = markDirty ? _age - 1 : _age,
            };
        }

        public AgeManager Bump()
        {
            ++_age;
            return this;
        }

        public bool IsSame()
        {
            return _age == _parent!._age;
        }

        public bool IsDirty()
        {
            return _age != _parent!._age;
        }

        public bool NeedToCatchUpAnyOf(params AgeManager[] ageManagers)
        {
            var dirty = NeedToCatchUp();
            foreach (var ageManager in ageManagers)
            {
                if (ageManager.NeedToCatchUp())
                {
                    dirty = true;
                }
            }
            return dirty;
        }

        public bool NeedToCatchUp()
        {
            if (IsDirty())
            {
                CatchUp();
                return true;
            }
            else
            {
                return false;
            }
        }

        public AgeManager CatchUp()
        {
            _age = _parent!._age;
            return this;
        }

        public AgeManager MarkDirty()
        {
            _age = _parent!._age - 1;
            return this;
        }
    }
}
