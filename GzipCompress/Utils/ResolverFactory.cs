using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GzipLib.Abstract;
using GzipLib;
using System.IO.Compression;

namespace GzipCompress.Utils
{
    public static class ResolverFactory
    {
        public static IZipFactory CreateFactory()
        {
            switch (Options.CompressionMode)
            {
                case System.IO.Compression.CompressionMode.Compress:
                    return new Compressor(Options.InputFile, Options.OutputFile, Options.NumberOfCores);
                case System.IO.Compression.CompressionMode.Decompress:
                    return new Decompressor(Options.InputFile, Options.OutputFile, Options.NumberOfCores);
                default:
                    return null;
            }
        }
    }
}
