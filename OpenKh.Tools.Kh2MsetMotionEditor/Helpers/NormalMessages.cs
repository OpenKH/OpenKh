using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public class NormalMessages
    {
        private int _nextId = 1;
        private readonly List<Tuple<int, string>> _pairs = new List<Tuple<int, string>>();

        public void Add(string message)
        {
            _pairs.Add(new Tuple<int, string>(_nextId, message));
            _nextId += 1;
        }

        public void Close(int id)
        {
            _pairs.RemoveAll(pair => pair.Item1 == id);
        }

        public IEnumerable<Tuple<int, string>> GetPairs() => _pairs.AsReadOnly();
    }
}
