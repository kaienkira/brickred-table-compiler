using Client.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class App
{
    private static string GetTableFileContent(string filePath)
    {
        string fileContent;
        try {
            fileContent = File.ReadAllText(
                filePath, Encoding.Unicode);
        } catch (Exception e) {
            Console.Error.WriteLine(string.Format(
                "can not open file {0}: {1}",
                filePath, e.Message));
            return "";
        }

        return fileContent;
    }

    public static int Main(string[] args)
    {
        string csvDir = ".";
        if (args.Length > 0) {
            csvDir = args[0];
        }

        TblCopy tblCopy = new TblCopy();
        TblEffect tblEffect = new TblEffect();
        TblItem tblItem = new TblItem();
        TblNpc tblNpc = new TblNpc();
        TblSkillLevel tblSkillLevel = new TblSkillLevel();
        string errorInfo;

        if (tblCopy.Parse(GetTableFileContent(
                Path.Combine(csvDir, "copy.csv")),
                out errorInfo) == false) {
            Console.Error.WriteLine(string.Format(
                "parse {0} failed: {1}",
                "copy.csv", errorInfo));
            return 1;
        }
        if (tblEffect.Parse(GetTableFileContent(
                Path.Combine(csvDir, "effect.csv")),
                out errorInfo) == false) {
            Console.Error.WriteLine(string.Format(
                "parse {0} failed: {1}",
                "effect.csv", errorInfo));
            return 1;
        }
        if (tblItem.Parse(GetTableFileContent(
                Path.Combine(csvDir, "item.csv")),
                out errorInfo) == false) {
            Console.Error.WriteLine(string.Format(
                "parse {0} failed: {1}",
                "item.csv", errorInfo));
            return 1;
        }
        if (tblNpc.Parse(GetTableFileContent(
                Path.Combine(csvDir, "npc.csv")),
                out errorInfo) == false) {
            Console.Error.WriteLine(string.Format(
                "parse {0} failed: {1}",
                "npc.csv", errorInfo));
            return 1;
        }
        if (tblSkillLevel.Parse(GetTableFileContent(
                Path.Combine(csvDir, "skill_level.csv")),
                out errorInfo) == false) {
            Console.Error.WriteLine(string.Format(
                "parse {0} failed: {1}",
                "skill_level.csv", errorInfo));
            return 1;
        }

        {
            TblEffect.Row row = tblEffect.GetRow(3);
            if (row != null) {
                Console.WriteLine(string.Format(
                    "tbl_effect:3:resource_path: {0}",
                    row.resource_path));
            }
        }
        {
            List<TblSkillLevel.Row> rowSet = tblSkillLevel.GetRowSet(100503);
            if (rowSet != null) {
                Console.WriteLine(string.Format(
                    "tbl_skill_level:100503:range_param:p1: {0}",
                    rowSet[0].range_param.p1));
            }
        }

        return 0;
    }
}
