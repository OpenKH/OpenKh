using Kaitai;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Research.GenGhidraComments.Ksy
{
    public class Tracer
    {
        //internal TextWriter writer = TextWriter.Null; //new StringWriter();
        internal TextWriter writer = new StringWriter();

        private Stack<string> readStack = new Stack<string>();
        private Stack<string> memberStack = new Stack<string>();
        private Stack<string> arrayStack = new Stack<string>();
        private Stack<string> prefixStack = new Stack<string>();
        private Stack<int> posStack = new Stack<int>();
        private IDictionary<int, string> ofs2Member;
        private Stack<ReadStack2> readStack2 = new Stack<ReadStack2>();

        class ReadStack2
        {
            internal KaitaiStruct self;
            internal KaitaiStream io;
            internal int seek = -1;
            internal int lastPos = -1;
            internal bool newIo = false;
            internal List<string> members = new List<string>();

            internal void MarkMember(string name)
            {
                if (!members.Contains(name))
                {
                    members.Add(name);
                }
            }
        }

        /// <param name="fileExtAndDelim">"mset:" or such.</param>
        public Tracer(IDictionary<int, string> ofs2Member, int baseAdr, string fileExtAndDelim)
        {
            this.ofs2Member = ofs2Member;

            posStack.Push(baseAdr);
            prefixStack.Push(fileExtAndDelim);
        }

        private string Indent => new string(' ', readStack.Count + memberStack.Count + arrayStack.Count);
        private string Indent1 => Indent;
        private int tailPos => posStack.Peek();

        public void BeginRead(string entityName, KaitaiStruct self, KaitaiStream io, KaitaiStruct parent, KaitaiStruct root)
        {
            readStack2.Push(
                new ReadStack2
                {
                    self = self,
                    io = io,
                }
            );

            prefixStack.Push(prefixStack.Peek() + "." + entityName);
            var pos = (int)io.Pos;
            writer.WriteLine($"{tailPos:X6} {pos:X6} {tailPos + pos:X6} {Indent}>C {entityName} //{prefixStack.Peek()}");
            readStack.Push(entityName);
        }

        public void EndRead()
        {
            var pop = readStack2.Peek();
            var self = pop.self;

            // Complete preemptive scans.
            foreach (var prop in self.GetType().GetProperties())
            {
                var obj = prop.GetValue(self);

                if (pop.members.Contains(prop.Name))
                {
                    if (obj is IEnumerable<KaitaiStruct> subItems)
                    {
                        foreach (var sub in subItems)
                        {
                            foreach (var propSub in sub.GetType().GetProperties())
                            {
                                propSub.GetValue(sub);
                            }
                        }
                    }
                }
            }

            readStack2.Pop();

            var entityName = readStack.Pop();
            writer.WriteLine($"                     {Indent1}<C {entityName}");

            prefixStack.Pop();
        }

        public void Seek(long pos)
        {
            readStack2.Peek().seek = (int)pos;
        }

        public void DeclareNewIo()
        {
            var peek = readStack2.Peek();
            peek.newIo = true;
        }

        public void BeginMember(string memberName)
        {
            var peek = readStack2.Peek();
            var pos = (peek.seek != -1) ? peek.seek : (peek.lastPos != -1) ? peek.lastPos : (int)peek.io.Pos;

            peek.MarkMember(memberName);

            prefixStack.Push(prefixStack.Peek() + "." + memberName);

            writer.WriteLine($"{tailPos:X6} {pos:X6} {tailPos + pos:X6} {Indent}>M {memberName} //{prefixStack.Peek()}");

            posStack.Push(peek.newIo ? posStack.Peek() + pos : posStack.Peek());

            peek.lastPos = -1;
            peek.seek = -1;
            peek.newIo = false;

            ofs2Member[tailPos + pos] = prefixStack.Peek();

            memberStack.Push(memberName);
        }

        public void EndMember()
        {
            var memberName = memberStack.Pop();
            writer.WriteLine($"                     {Indent1}<M {memberName}");

            posStack.Pop();

            prefixStack.Pop();

            readStack2.Peek().lastPos = (int)readStack2.Peek().io.Pos;
        }

        public void BeginArrayMember(string memberName)
        {
            var peek = readStack2.Peek();
            peek.MarkMember(memberName);

            prefixStack.Push(prefixStack.Peek() + "." + memberName);
            var pos = (int)readStack2.Peek().io.Pos;
            writer.WriteLine($"{tailPos:X6} {pos:X6} {tailPos + pos:X6} {Indent}>A {memberName} //{prefixStack.Peek()}");
            arrayStack.Push(memberName);

            ofs2Member[tailPos + pos] = prefixStack.Peek();
        }

        public void EndArrayMember()
        {
            var memberName = arrayStack.Pop();
            writer.WriteLine($"                     {Indent1}<A {memberName}");
            prefixStack.Pop();

            readStack2.Peek().lastPos = (int)readStack2.Peek().io.Pos;
        }

        public void SwitchStart()
        {
            var pos = (int)readStack2.Peek().io.Pos;
            writer.WriteLine($"{tailPos:X6} {pos:X6} {tailPos + pos:X6} {Indent}*");
        }
    }
}
