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
        private int posPick = -1;
        private IDictionary<int, string> ofs2Member;
        private Stack<ReadStack2> readStack2 = new Stack<ReadStack2>();

        class ReadStack2
        {
            internal KaitaiStruct self;
            internal KaitaiStream io;
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
            var pop = readStack2.Pop();
            var self = pop.self;

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
            var pos = (int)readStack2.Peek().io.Pos;

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
        }

        public void SwitchStart()
        {
            var pos = (int)readStack2.Peek().io.Pos;
            posPick = pos;
            writer.WriteLine($"{tailPos:X6} {pos:X6} {tailPos + pos:X6} {Indent}*");
        }
    }
}
