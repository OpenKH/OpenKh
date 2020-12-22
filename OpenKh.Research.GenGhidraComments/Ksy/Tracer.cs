using Kaitai;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Research.GenGhidraComments.Ksy
{
    public class Tracer
    {
        internal TextWriter writer = TextWriter.Null; //new StringWriter();

        private string entityName;
        private KaitaiStruct self;
        private KaitaiStream io;
        private KaitaiStruct parent;
        private KaitaiStruct root;
        private Stack<string> readStack = new Stack<string>();
        private Stack<string> memberStack = new Stack<string>();
        private Stack<string> arrayStack = new Stack<string>();
        private Stack<string> prefixStack = new Stack<string>();
        private Stack<int> posStack = new Stack<int>();
        private int posPick = -1;
        private IDictionary<int, string> ofs2Member;

        public Tracer(IDictionary<int, string> ofs2Member, int baseAdr)
        {
            this.ofs2Member = ofs2Member;

            posStack.Push(baseAdr);
            prefixStack.Push("");
        }

        private string Indent => new string(' ', readStack.Count + memberStack.Count + arrayStack.Count);
        private string Indent1 => Indent;
        private int tailPos => posStack.Peek();

        public void BeginRead(string entityName, KaitaiStruct self, KaitaiStream io, KaitaiStruct parent, KaitaiStruct root)
        {
            this.entityName = entityName;
            this.self = self;
            this.io = io;
            this.parent = parent;
            this.root = root;

            prefixStack.Push(prefixStack.Peek() + "." + entityName);
            var pos = (int)io.Pos;
            writer.WriteLine($"{tailPos:X6} {pos:X6} {tailPos + pos:X6} {Indent}>C {entityName} //{prefixStack.Peek()}");
            readStack.Push(entityName);
        }

        public void EndRead()
        {
            var self = this.self;

            // Complete preemptive scans.
            foreach (var prop in self.GetType().GetProperties())
            {
                prop.GetValue(self);
            }

            var entityName = readStack.Pop();
            writer.WriteLine($"                     {Indent1}<C {entityName}");

            prefixStack.Pop();
        }

        public void BeginMember(string memberName)
        {
            var pos = (int)io.Pos;

            prefixStack.Push(prefixStack.Peek() + "." + memberName);

            if (posPick != -1)
            {
                posStack.Push(posStack.Peek() + posPick);
                writer.WriteLine($"{tailPos:X6}               {Indent}>M {memberName}");
            }
            else
            {
                posStack.Push(posStack.Peek());
                writer.WriteLine($"{tailPos:X6} {pos:X6} {tailPos + pos:X6} {Indent}>M {memberName} //{prefixStack.Peek()}");
            }
            posPick = -1;

            ofs2Member[tailPos + pos] = prefixStack.Peek();

            memberStack.Push(memberName);
        }

        public void EndMember()
        {
            var memberName = memberStack.Pop();
            writer.WriteLine($"                     {Indent1}<M {memberName}");

            posStack.Pop();

            prefixStack.Pop();
        }

        public void BeginArrayMember(string memberName)
        {
            prefixStack.Push(prefixStack.Peek() + "." + memberName);
            var pos = (int)io.Pos;
            writer.WriteLine($"{tailPos:X6} {pos:X6} {tailPos + pos:X6} {Indent}>A {memberName} //{prefixStack.Peek()}");
            arrayStack.Push(memberName);
        }

        public void EndArrayMember()
        {
            var memberName = arrayStack.Pop();
            writer.WriteLine($"                     {Indent1}<A {memberName}");
            prefixStack.Pop();
        }

        public void SwitchStart()
        {
            var pos = (int)io.Pos;
            posPick = pos;
            writer.WriteLine($"{tailPos:X6} {pos:X6} {tailPos + pos:X6} {Indent}*");
        }
    }
}
