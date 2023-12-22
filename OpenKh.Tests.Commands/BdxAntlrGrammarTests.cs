using Antlr4.Runtime;
using OpenKh.Command.Bdxio.Models;
using OpenKh.Command.Bdxio.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenKh.Tests.Commands
{
    public class BdxAntlrGrammarTests
    {
        [Theory]
        [MemberData(nameof(GetOpcodes))]
        public void TestOpcodeNamesCanBeParsedCorrectly(string opcode)
        {
            var stream = FromString(opcode);
            var lexer = new BdxScriptLexer(stream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new BdxScriptParser(tokens);
            var id = parser.id();
            Assert.Equal(expected: opcode, actual: id.GetText());
        }

        [Theory]
        [InlineData("cvt.s.w")]
        [InlineData("keyword.12345")]
        public void TestTokenCanBeParsedAsIdCorrectly(string token)
        {
            var stream = FromString(token);
            var lexer = new BdxScriptLexer(stream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new BdxScriptParser(tokens);
            var id = parser.id();
            Assert.Equal(expected: token, actual: id.GetText());
        }

        public static IEnumerable<object[]> GetOpcodes()
        {
            return new string[0]
                .Concat(
                    BdxInstructionDescs.GetDescs()
                        .Select(desc => desc.Name)
                )
                .Concat(
                    BdxInstructionDescs.GetDescs()
                        .SelectMany(desc => desc.OldNames)
                )
                .Distinct()
                .Select(name => new object[] { name });
        }

        private static ICharStream FromString(string script)
        {
            var stream = CharStreams.fromString(script);
            return stream;
        }
    }
}
