using System;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public class GetDiffService
    {
        public string Name { get; set; }
        public Func<byte[], byte[], Task<byte[]>> DiffAsync { get; set; }
    }
}
