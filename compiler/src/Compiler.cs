using Brickred.Table.Compiler;
using Mono.Options;
using System;
using System.IO;

public sealed class App
{
    private static void PrintUsage()
    {
        Console.Error.WriteLine("brickred table compiler");
        Console.Error.WriteLine(string.Format(
            "usage: {0} " +
            "-f <define_file> " +
            "-l <language> " +
            "-r <reader>",
            AppDomain.CurrentDomain.FriendlyName));
        Console.Error.WriteLine(
            "    [-o <output_dir>]");
        Console.Error.WriteLine(
            "    [-n <new_line_type>] (unix|dos) default is unix");
        Console.Error.WriteLine(
            "language supported: cpp csharp");
    }

    public static int Main(string[] args)
    {
        string optDefineFilePath = "";
        string optLanguage = "";
        string optReader = "";
        string optOutputDir = "";
        string optNewLineType = "";

        // parse command line options
        {
            OptionSet options = new OptionSet();
            options.Add("f=", v => optDefineFilePath = v);
            options.Add("l=", v => optLanguage = v);
            options.Add("r=", v => optReader = v);
            options.Add("o=", v => optOutputDir = v);
            options.Add("n=", v => optNewLineType = v);

            try {
                options.Parse(args);
            } catch (OptionException e) {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
        }

        // check command line options
        if (optDefineFilePath == "" ||
            optLanguage == "" ||
            optReader == "") {
            PrintUsage();
            return 1;
        }
        if (optOutputDir == "") {
            optOutputDir = ".";
        }

        if (File.Exists(optDefineFilePath) == false) {
            Console.Error.WriteLine(string.Format(
                "error: can not find define file `{0}`",
                optDefineFilePath));
            return 1;
        }
        if (Directory.Exists(optOutputDir) == false) {
            Console.Error.WriteLine(string.Format(
                "error: can not find output directory `{0}`",
                optOutputDir));
            return 1;
        }

        using (TableParser parser = new TableParser()) {
            if (parser.Parse(optDefineFilePath) == false) {
                return 1;
            }
            if (parser.FilterByReader(optReader) == false) {
                return 1;
            }

            BaseCodeGenerator generator = null;
            if (optLanguage == "cpp") {
                generator = new CppCodeGenerator();
            } else if (optLanguage == "csharp") {
                generator = new CSharpCodeGenerator();
            } else {
                Console.Error.WriteLine(string.Format(
                    "error: language `{0}` is not supported",
                    optLanguage));
                return 1;
            }

            using (generator) {
                BaseCodeGenerator.NewLineType newLineType =
                    BaseCodeGenerator.NewLineType.Unix;
                if (optNewLineType == "dos") {
                    newLineType = BaseCodeGenerator.NewLineType.Dos;
                }

                if (generator.Generate(parser.Descriptor,
                        optReader, optOutputDir, newLineType) == false) {
                    return 1;
                }
            }
        }

        return 0;
    }
}
