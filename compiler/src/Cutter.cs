using Brickred.Table.Compiler;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public sealed class App
{
    private static void PrintUsage()
    {
        Console.Error.WriteLine("brickred table cutter");
        Console.Error.WriteLine(string.Format(
            "usage: {0} " +
            "-f <define_file> " +
            "-r <reader> " +
            "-i <input_file_dir> " +
            "-o <output_file_dir>",
            AppDomain.CurrentDomain.FriendlyName));
    }

    public static int Main(string[] args)
    {
        string optDefineFilePath = "";
        string optReader = "";
        string optInputDir = "";
        string optOutputDir = "";

        // parse command line options
        {
            OptionSet options = new OptionSet();
            options.Add("f=", v => optDefineFilePath = v);
            options.Add("r=", v => optReader = v);
            options.Add("i=", v => optInputDir = v);
            options.Add("o=", v => optOutputDir = v);

            try {
                options.Parse(args);
            } catch (OptionException e) {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
        }

        // check command line options
        if (optDefineFilePath == "" ||
            optReader == "" ||
            optInputDir == "" ||
            optOutputDir == "") {
            PrintUsage();
            return 1;
        }

        if (File.Exists(optDefineFilePath) == false) {
            Console.Error.WriteLine(string.Format(
                "error: can not find define file `{0}`",
                optDefineFilePath));
            return 1;
        }
        if (Directory.Exists(optInputDir) == false) {
            Console.Error.WriteLine(string.Format(
                "error: can not find input directory `{0}`",
                optInputDir));
            return 1;
        }
        if (Directory.Exists(optOutputDir) == false) {
            Console.Error.WriteLine(string.Format(
                "error: can not find output directory `{0}`",
                optOutputDir));
            return 1;
        }
        if (Path.GetFullPath(optOutputDir) ==
            Path.GetFullPath(optInputDir)) {
            Console.Error.WriteLine(string.Format(
                "error: output directory can not be same as input directory"));
            return 1;
        }

        using (TableParser parser = new TableParser()) {
            if (parser.Parse(optDefineFilePath) == false) {
                return 1;
            }

            if (CutTables(parser.Descriptor, optReader,
                    optInputDir, optOutputDir) == false) {
                return 1;
            }
        }

        return 0;
    }

    private static bool CutTables(
        TableDescriptor descriptor, string reader,
        string inputDir, string outputDir)
    {
        // check reader
        if (descriptor.Readers.ContainsKey(reader) == false) {
            Console.Error.WriteLine(string.Format(
                "error: reader `{0}` is not defined", reader));
            return false;
        }

        for (int i = 0; i < descriptor.Tables.Count; ++i) {
            TableDescriptor.TableDef tableDef = descriptor.Tables[i];

            if (tableDef.Readers.Count == 0 ||
                tableDef.Readers.ContainsKey(reader)) {
                if (CutTable(tableDef, reader,
                        inputDir, outputDir) == false) {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool CutTable(
        TableDescriptor.TableDef tableDef, string reader,
        string inputDir, string outputDir)
    {
        string[] lineSep = {"\r\n"};
        string[] colSep = {"\t"};

        Dictionary<int, bool> deletedColumns =
            new Dictionary<int, bool>();

        for (int i = 0; i < tableDef.Columns.Count; ++i) {
            TableDescriptor.TableDef.ColumnDef columnDef =
                tableDef.Columns[i];

            if (columnDef == tableDef.TableKey ||
                columnDef.Readers.Count == 0 ||
                columnDef.Readers.ContainsKey(reader)) {
                continue;
            }
            deletedColumns.Add(i, true);
        }

        // read input file
        string inputFileContent = null;
        try {
            byte[] fileBin = File.ReadAllBytes(
                Path.Combine(inputDir, tableDef.FileName));
            inputFileContent = Encoding.UTF8.GetString(fileBin);
        } catch (Exception e) {
            Console.Error.WriteLine(string.Format(
                "error: can not read input file `{0}`: {1}",
                tableDef.FileName, e.Message));
            return false;
        }

        // split lines
        string[] lines = inputFileContent.Split(lineSep,
            StringSplitOptions.None);
        if (lines[lines.Length - 1] != "") {
            Console.Error.WriteLine(string.Format(
                "error: input file `{0}` file line ending is required",
                tableDef.FileName));
            return false;
        }

        int lineCount = lines.Length - 1;
        if (lineCount < 2) {
            Console.Error.WriteLine(string.Format(
                "error: input file `{0}` " +
                "comment line and name line is required",
                tableDef.FileName));
            return false;
        }

        // split columns
        List<List<string>> lineCols = new List<List<string>>();
        for (int i = 0; i < lineCount; ++i) {
            List<string> cols = new List<string>(
                lines[i].Split(colSep, StringSplitOptions.None));

            if (cols.Count != tableDef.Columns.Count) {
                Console.Error.WriteLine(string.Format(
                    "error: input file `{0}` " +
                    "line {1} column count {2} is invalid, " +
                    "should be {3}",
                    tableDef.Name, i + 1, cols.Count,
                    tableDef.Columns.Count));
                return false;
            }
            lineCols.Add(cols);
        }

        // check name line
        for (int i = 0; i < tableDef.Columns.Count; ++i) {
            TableDescriptor.TableDef.ColumnDef columnDef =
                tableDef.Columns[i];

            if (lineCols[1][i] != columnDef.Name) {
                Console.Error.WriteLine(string.Format(
                    "error: input file `{0}` " +
                    "column {1} should be named as `{2}`",
                    tableDef.Name, i + 1, columnDef.Name));
                return false;
            }
        }

        // cut columns
        StringBuilder sb = new StringBuilder();
        List<string> outputCols = new List<string>();

        for (int i = 0; i < lineCount; ++i) {
            List<string> inputCols = lineCols[i];
            outputCols.Clear();

            for (int j = 0; j < inputCols.Count; ++j) {
                if (deletedColumns.ContainsKey(j)) {
                    continue;
                }
                outputCols.Add(inputCols[j]);
            }
            sb.Append(string.Join("\t", outputCols));
            sb.Append("\r\n");
        }

        string outputFileContent = sb.ToString();

        // write output file
        try {
            byte[] fileBin = Encoding.UTF8.GetBytes(outputFileContent);
            File.WriteAllBytes(
                Path.Combine(outputDir, tableDef.FileName),
                fileBin);
        } catch (Exception e) {
            Console.Error.WriteLine(string.Format(
                "error: can not write output file `{0}`: {1}",
                tableDef.FileName, e.Message));
            return false;
        }

        return true;
    }
}
