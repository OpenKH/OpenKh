using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public class ErrorMessages
    {
        public int Count => List.Count;

        public IEnumerable<string> Messages => List.Select(it => $"- {it.Message}");

        public bool Any() => Messages.Any();

        public void Clear()
        {
            List.Clear();
        }

        public void Add(Exception ex)
        {
            List.Add(ex);
        }

        public string GetFullMessages()
        {
            return string.Join(
                Environment.NewLine + Environment.NewLine,
                List
            );
        }

        public List<Exception> List { get; set; } = new List<Exception>();
    }
}
