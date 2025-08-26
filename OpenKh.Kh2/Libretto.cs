using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;
namespace OpenKh.Kh2
//Can properly read/write and update. Can insert new entries between.
{
    public class Libretto
    {
        [Data] public int MagicCode { get; set; }
        [Data] public int Count { get; set; }
        [Data] public List<TalkMessageDefinition> Definitions { get; set; }
        [Data] public List<List<TalkMessageContent>> Contents { get; set; }

        public class TalkMessageDefinition
        {
            [Data] public ushort TalkMessageId { get; set; }
            [Data] public ushort Type { get; set; }
            [Data] public uint ContentPointer { get; set; }
        }

        public class TalkMessageContent
        {
            [Data] public ushort CodeType { get; set; }
            [Data] public ushort Unknown { get; set; }
            [Data] public uint TextId { get; set; }
        }

        public class TalkMessagePatch
        {
            [Data] public ushort TalkMessageId { get; set; }
            [Data] public ushort Type { get; set; }
            [Data] public List<ContentPatch> Contents { get; set; }
        }

        public class ContentPatch
        {
            [Data] public ushort CodeType { get; set; }
            [Data] public ushort Unknown { get; set; }
            [Data] public uint TextId { get; set; }
        }

        public static Libretto Read(Stream stream)
        {
            //Store initial position of stream.
            var basePosition = stream.Position;
            //Create new Libretto object, then read MagicCode & Count from stream.
            var libretto = new Libretto
            {
                MagicCode = stream.ReadInt32(),
                Count = stream.ReadInt32()
            };

            //Initialize definitions/contents list w/ capacity equal to count
            libretto.Definitions = new List<TalkMessageDefinition>(libretto.Count);
            libretto.Contents = new List<List<TalkMessageContent>>(libretto.Count);

            //Loop over number of definitions specified by count.
            for (int i = 0; i < libretto.Count; i++)
            {
                //Read TalkMessageDefinition from the stream, add to Definitions list.
                libretto.Definitions.Add(new TalkMessageDefinition
                {
                    TalkMessageId = stream.ReadUInt16(),
                    Type = stream.ReadUInt16(),
                    ContentPointer = stream.ReadUInt32()
                });
            }

            //Loop over each definition in the Definitions list.
            foreach (var definition in libretto.Definitions)
            {
                //Set stream position to the Content Pointer for the current definition.
                stream.Position = basePosition + definition.ContentPointer;
                //Create a new list to hold our TalkMessageContent objects for the current definition.
                var contents = new List<TalkMessageContent>();
                while (true)
                {
                    // Read a TalkMessageContent object manually from the stream
                    var content = new TalkMessageContent
                    {
                        CodeType = stream.ReadUInt16(),
                        Unknown = stream.ReadUInt16(),
                        TextId = stream.ReadUInt32()
                    };

                    // Check for the termination condition: CodeType == 0 and Unknown == 0
                    if (content.CodeType == 0 || content.CodeType == null || content.Unknown == 0 || content.Unknown == null)
                    {
                        break;
                    }

                    // Add content to the contents list
                    contents.Add(content);
                }
                //Add list of contents for the current definition to the Contents list.
                libretto.Contents.Add(contents);
            }

            return libretto;
        }


        public static void Write(Stream stream, Libretto libretto)
        {
            var basePosition = stream.Position;

            stream.Write(libretto.MagicCode);
            stream.Write(libretto.Count);

            var offset = 8 + libretto.Definitions.Count * 8;    //Set offset variable; start AFTER magiccode+count and update the offset later. Offset = 8 + # of Definitions*8.

            foreach (var definition in libretto.Definitions)
            {
                stream.Write(definition.TalkMessageId);                                              //Write the TalkMessage for each Definition.
                stream.Write(definition.Type);                                                       //Write the Unknown for each Definition.
                stream.Write(offset);                                                                //Write the offset for each Definition.
                offset += libretto.Contents[libretto.Definitions.IndexOf(definition)].Count * 8 + 4; //Update the Offset in each definition.
            }

            stream.Position = basePosition + 8 + libretto.Definitions.Count * 8;
            foreach (var contents in libretto.Contents)
            {
                foreach (var content in contents)
                {
                    stream.Write(content.CodeType);
                    stream.Write(content.Unknown);
                    stream.Write(content.TextId);
                    //if (content.Unknown1 == 0)
                    //    break;
                }
                //Break this until we figure out why it isn't reading.
                stream.Write(0); // Write the padding (0x00000000)
            }
        }
    }
}
