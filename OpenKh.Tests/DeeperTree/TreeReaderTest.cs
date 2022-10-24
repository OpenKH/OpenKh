using OpenKh.DeeperTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenKh.Tests.DeeperTree
{
    public class TreeReaderTest
    {
        private static readonly TreeReader _treeReader = new TreeReaderBuilder()
            .AddType(nameof(SetProject), typeof(SetProject))
            .Build()
            ;

        private class SetProject
        {
            public string Name { get; set; }
        }

        [Fact]
        public void CollectionTest()
        {
            Assert.Equal(
                expected: ToJson(
                    new SetProject[]
                    {
                        new SetProject { Name = "a", },
                        new SetProject { Name = "b", },
                    }
                ),
                actual: ToJson(
                    _treeReader.Deserialize<SetProject[]>("SetProject{Name a} SetProject{Name b}")
                )
            );

            Assert.Equal(
                expected: ToJson(
                    new SetProject[]
                    {
                        new SetProject { Name = "a", },
                        new SetProject { Name = "b", },
                    }
                ),
                actual: ToJson(
                    _treeReader.Deserialize<List<SetProject>>("SetProject{Name a} SetProject{Name b}")
                )
            );

            Assert.Equal(
                expected: ToJson(
                    new SetProject[0]
                ),
                actual: ToJson(
                    _treeReader.Deserialize<SetProject[]>("")
                )
            );

            Assert.Equal(
                expected: ToJson(
                    new SetProject[0]
                ),
                actual: ToJson(
                    _treeReader.Deserialize<List<SetProject>>("")
                )
            );
        }

        private static string ToJson(object any) => System.Text.Json.JsonSerializer.Serialize(any);

        [Fact]
        public void NonQuotedStringTests()
        {
            Assert.Equal(
                expected: "123_456/abc.def",
                actual: _treeReader.Deserialize<SetProject>("Name 123_456/abc.def").Name
            );
        }

        [Fact]
        public void QuotedStringTests()
        {
            Assert.Equal(
                expected: "123\r\n\t456\\789",
                actual: _treeReader.Deserialize<SetProject>("Name \"123\\r\\n\\t456\\\\789\"").Name
            );

            Assert.Equal(
                expected: "123 456  789",
                actual: _treeReader.Deserialize<SetProject>("Name \"123 456  789\"").Name
            );

            Assert.Equal(
                expected: "abc\ndef",
                actual: _treeReader.Deserialize<SetProject>("Name \"abc\\\ndef\"").Name
            );

            Assert.Equal(
                expected: "abc\r\ndef",
                actual: _treeReader.Deserialize<SetProject>("Name \"abc\\\r\ndef\"").Name
            );
        }
    }
}
