using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.Usecases
{
    public class CombineBarUsecase
    {
        public IEnumerable<Bar.Entry> Combine(IEnumerable<Bar.Entry> left, IEnumerable<Bar.Entry> right)
        {
            var leftList = left.ToList();
            var rightList = right.ToList();

            foreach (var one in rightList)
            {
                var hitIndex = leftList.FindIndex(it => it.Type == one.Type && it.Name == one.Name);
                if (hitIndex == -1)
                {
                    leftList.Add(one);
                }
                else
                {
                    leftList[hitIndex] = one;
                }
            }

            return leftList.AsReadOnly();
        }
    }
}
