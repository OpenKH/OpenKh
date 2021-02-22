// MIT License
// 
// Copyright(c) 2018 Luciano (Xeeynamo) Ciccariello
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// Part of this software belongs to XeEngine toolset and United Lines Studio
// and it is currently used to create commercial games by Luciano Ciccariello.
// Please do not redistribuite this code under your own name, stole it or use
// it artfully, but instead support it and its author. Thank you.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.BarTool.Models
{
    public class EnumItemModel<T>
    {
        public T Value { get; set; }

        public string Name { get; set; }
    }

    public class EnumModel<T> : IEnumerable<EnumItemModel<T>>
         where T : struct, IConvertible
    {
        private readonly IEnumerable<EnumItemModel<T>> items;

        public EnumModel()
        {
            var type = typeof(T);
            if (type.IsEnum == false)
            {
                throw new InvalidOperationException($"{type} is not an enum.");
            }

            items = Enum.GetValues(type)
                .Cast<T>()
                .Select(e => new EnumItemModel<T>()
                {
                    Value = e,
                    Name = e.ToString()
                });
        }

        public IEnumerator<EnumItemModel<T>> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}
