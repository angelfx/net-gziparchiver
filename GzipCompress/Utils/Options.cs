using System;
using System.IO.Compression;
using System.Linq;
using System.IO;

namespace GzipCompress.Utils
{
    public static class Options
    {
        /// <summary>
        /// Save compression mode: compress or decompress
        /// </summary>
        public static CompressionMode CompressionMode { get; set; }

        /// <summary>
        /// Input file for compress or decompress
        /// </summary>
        public static string InputFile { get; set; }

        /// <summary>
        /// Output file
        /// </summary>
        public static string OutputFile { get; set; }

        /// <summary>
        /// Number of available cores
        /// </summary>
        public static int NumberOfCores { get; set; }

        /// <summary>
        /// Parse input arguments
        /// </summary>
        /// <param name="args"></param>
        public static void IntitalizeArgs(string[] args)
        {
            if (args == null || args.Length != 3 || args.Any(s => string.IsNullOrWhiteSpace(s)))
                throw new ArgumentException("Invalid arguments");

            try
            {
                Options.GetMode(args[0]);
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException(string.Format("Argument {0} is Wrong", args[0]), ex);
            }

            if (!File.Exists(args[1]))
            {
                FileNotFoundException innerEx = new FileNotFoundException("File not found", args[1]);
                throw new ArgumentException(string.Format("Argument {0} is Wrong", args[1]), innerEx);
            }
            Options.InputFile = args[1];

            try
            {
                string outputDirectory = Path.GetDirectoryName(args[2]);
                string outputFileName = Path.GetFileName(args[2]);
                if (string.IsNullOrWhiteSpace(outputFileName) || outputDirectory == null || (outputDirectory != string.Empty && !Directory.Exists(outputDirectory)))
                {
                    throw new Exception("Wrong path for save");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("Argument {0} is wrong", args[2]), ex);
            }
            Options.OutputFile = args[2];

            Options.NumberOfCores = Environment.ProcessorCount;
        }

        /// <summary>
        /// Get compression mode
        /// </summary>
        /// <param name="value"></param>
        private static void GetMode(string value)
        {
            if (value.ToLower() != "compress" && value.ToLower() != "decompress")
            {
                throw new InvalidOperationException(string.Format("Argument: {0} is wrong. Use compress/decompress", value));
            }

            Options.CompressionMode = (CompressionMode)Enum.Parse(typeof(CompressionMode), value, true);
        }
    }
}
