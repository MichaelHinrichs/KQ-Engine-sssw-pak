﻿//Written for games in the KQ Engine.
//Devine Twins https://store.steampowered.com/app/2723140
using System;
using System.IO;
using System.IO.Compression;

namespace KQ_Engine_Extractor
{
    static class Program
    {
        public static BinaryReader br;

        static void Main(string[] args)
        {
            using FileStream source = File.OpenRead(args[0]);
            br = new(source);
            br.BaseStream.Position = 0x48;

            System.Collections.Generic.List<SUBFILE>  fileTable = new();
            for (int i = 0; i < 12409; i++)
            {
                string name = NullTerminatedString();
                if (br.BaseStream.Position < (i + 1) * 0x50 + 0x38)
                    br.BaseStream.Position = (i + 1) * 0x50 + 0x38;

                fileTable.Add(new SUBFILE
                {
                    name = name,
                    offset = br.ReadInt32(),
                    unknown1 = br.ReadInt32(),
                    size = br.ReadInt32(),
                    unknown2 = br.ReadInt32()
                });
            }

            Directory.CreateDirectory(Path.GetDirectoryName(args[0]) + "//" + Path.GetFileNameWithoutExtension(args[0]));
            foreach (SUBFILE sub in fileTable)
            {
                br.BaseStream.Position = sub.offset;
                br.ReadInt16();
                using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes(sub.size - 2)), CompressionMode.Decompress))
                    ds.CopyTo(File.Create(Path.GetDirectoryName(args[0]) + "//" + Path.GetFileNameWithoutExtension(args[0]) + "//" + sub.name));
            }
        }

        public struct SUBFILE
        {
            public string name;
            public int offset;
            public int unknown1;
            public int size;
            public int unknown2;
        }

        public static string NullTerminatedString()
        {
            char[] fileName = Array.Empty<char>();
            char readchar = (char)1;
            while (readchar > 0)
            {
                readchar = br.ReadChar();
                Array.Resize(ref fileName, fileName.Length + 1);
                fileName[^1] = readchar;
            }
            Array.Resize(ref fileName, fileName.Length - 1);
            string name = new(fileName);
            return name;
        }
    }
}
