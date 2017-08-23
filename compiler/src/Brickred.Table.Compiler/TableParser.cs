using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Brickred.Table.Compiler
{
    public sealed class TableParser : IDisposable
    {
        private TableDescriptor descriptor = null;
        public TableDescriptor Descriptor
        {
            get { return this.descriptor; }
        }

        public TableParser()
        {
        }

        ~TableParser()
        {
            Dispose();
        }

        public bool Parse(string defineFilePath)
        {
            this.descriptor = new TableDescriptor();

            XDocument xmlDoc = null;
            try {
                this.descriptor.FilePath = Path.GetFullPath(defineFilePath);
                xmlDoc = XDocument.Load(
                    this.descriptor.FilePath, LoadOptions.SetLineInfo);
            } catch (Exception e) {
                Console.Error.WriteLine(string.Format(
                    "error: can not load define file `{0}`: {1}",
                    defineFilePath, e.Message));
                return false;
            }

            // check root node name
            if (xmlDoc.Root.Name != "define") {
                PrintLineError(xmlDoc.Root,
                    "root node must be `define` node");
                return false;
            }

            // parse readers
            {
                IEnumerator<XElement> iter = xmlDoc.XPathSelectElements(
                    "/define/reader").GetEnumerator();
                while (iter.MoveNext()) {
                    XElement element = iter.Current;

                    if (AddReaderDef(element) == false) {
                        return false;
                    }
                }
            }

            // parse global structs
            {
                IEnumerator<XElement> iter = xmlDoc.XPathSelectElements(
                    "/define/struct").GetEnumerator();
                while (iter.MoveNext()) {
                    XElement element = iter.Current;

                    if (AddStructDef(null, element) == false) {
                        return false;
                    }
                }
            }

            // parse tables
            {
                IEnumerator<XElement> iter = xmlDoc.XPathSelectElements(
                    "/define/table").GetEnumerator();
                while (iter.MoveNext()) {
                    XElement element = iter.Current;

                    if (AddTableDef(element) == false) {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool FilterByReader(string reader)
        {
            if (this.descriptor == null) {
                return false;
            }

            if (this.descriptor.Readers.ContainsKey(reader) == false) {
                Console.Error.WriteLine(string.Format(
                    "error: reader `{0}` is not defined", reader));
                return false;
            }

            // remove unread tables
            List<TableDescriptor.TableDef> filteredTables =
                new List<TableDescriptor.TableDef>();
            for (int i = 0; i < this.descriptor.Tables.Count; ++i) {
                TableDescriptor.TableDef tableDef =
                    this.descriptor.Tables[i];

                if (tableDef.Readers.Count == 0 ||
                    tableDef.Readers.ContainsKey(reader)) {
                    filteredTables.Add(tableDef);
                } else {
                    this.descriptor.TableNameIndex.Remove(tableDef.Name);
                }
            }
            this.descriptor.Tables = filteredTables;

            // remove unread columns
            for (int i = 0; i < this.descriptor.Tables.Count; ++i) {
                TableDescriptor.TableDef tableDef =
                    this.descriptor.Tables[i];

                List<TableDescriptor.TableDef.ColumnDef> filteredColumns =
                    new List<TableDescriptor.TableDef.ColumnDef>();
                for (int j = 0; j < tableDef.Columns.Count; ++j) {
                    TableDescriptor.TableDef.ColumnDef columnDef =
                        tableDef.Columns[j];
                    if (columnDef == tableDef.TableKey ||
                        columnDef.Readers.Count == 0 ||
                        columnDef.Readers.ContainsKey(reader)) {
                        filteredColumns.Add(columnDef);
                    } else {
                        tableDef.ColumnNameIndex.Remove(columnDef.Name);
                    }
                }
                tableDef.Columns = filteredColumns;
                CalculateTableKeyColumnIndex(tableDef);
            }

            return true;
        }

        public void Dispose()
        {
            if (this.descriptor != null) {
                this.descriptor = null;
            }
        }

        private int GetLineNumber(XElement element)
        {
            IXmlLineInfo lineInfo = (IXmlLineInfo)element;
            return lineInfo.LineNumber;
        }

        private void PrintLineError(
            string fileName, int lineNumber,
            string format, params object[] args)
        {
            Console.Error.WriteLine(
                string.Format("error:{0}:{1}: ", fileName, lineNumber) +
                string.Format(format, args));
        }

        private void PrintLineError(XElement element,
            string format, params object[] args)
        {
            PrintLineError(this.descriptor.FilePath,
                GetLineNumber(element), format, args);
        }

        private bool AddReaderDef(XElement element)
        {
            // check name attr
            string name;
            {
                XAttribute attr = element.Attribute("name");
                if (attr == null) {
                    PrintLineError(element,
                        "`reader` node must contain a `name` attribute");
                    return false;
                }
                name = attr.Value;
            }
            if (Regex.IsMatch(name, @"^[a-zA-Z_]\w*$") == false) {
                PrintLineError(element,
                    "`reader` node `name` attribute is invalid");
                return false;
            }
            if (this.descriptor.Readers.ContainsKey(name)) {
                PrintLineError(element,
                    "`reader` node `name` attribute duplicated");
                return false;
            }

            TableDescriptor.ReaderDef def =
                new TableDescriptor.ReaderDef();
            def.Name = name;
            def.LineNumber = GetLineNumber(element);

            // check namespace attr
            do {
                XAttribute attr = element.Attribute("namespace");
                if (attr == null) {
                    break;
                }

                string[] namespaceParts = attr.Value.Split('.');
                for (int i = 0; i < namespaceParts.Length; ++i) {
                    if (Regex.IsMatch(namespaceParts[i],
                            @"^[a-zA-Z_]\w*$") == false) {
                        PrintLineError(element,
                            "`reader` node `namespace` attribute is invalid");
                        return false;
                    }
                }

                def.Namespace = attr.Value;
                for (int i = 0; i < namespaceParts.Length; ++i) {
                    def.NamespaceParts.Add(namespaceParts[i]);
                }
            } while (false);

            this.descriptor.Readers.Add(def.Name, def);

            return true;
        }

        private bool AddStructDef(
            TableDescriptor.TableDef tableDef, XElement element)
        {
            // check name attr
            string name;
            {
                XAttribute attr = element.Attribute("name");
                if (attr == null) {
                    PrintLineError(element,
                        "`struct` node must contain a `name` attribute");
                    return false;
                }
                name = attr.Value;
            }
            if (Regex.IsMatch(name, @"^[a-zA-Z_]\w*$") == false) {
                PrintLineError(element,
                    "`struct` node `name` attribute is invalid");
                return false;
            }
            if (tableDef == null) {
                if (this.descriptor.GlobalStructNameIndex.ContainsKey(name) ||
                    this.descriptor.TableNameIndex.ContainsKey(name)) {
                    PrintLineError(element,
                        "`struct` node `name` attribute duplicated");
                    return false;
                }
            } else {
                if (tableDef.LocalStructNameIndex.ContainsKey(name)) {
                    PrintLineError(element,
                        "`struct` node `name` attribute duplicated");
                    return false;
                }
                if (name == "Row" ||
                    name == "Rows" ||
                    name == "RowSet" ||
                    name == "RowSets") {
                    PrintLineError(element,
                        "local struct can not be named as " +
                        "`Row`, `Rows`, `RowSet` or `RowSets`");
                    return false;
                }
            }

            TableDescriptor.StructDef def =
                new TableDescriptor.StructDef();
            def.ParentRef = tableDef;
            def.Name = name;
            def.LineNumber = GetLineNumber(element);

            // parse fields
            {
                IEnumerator<XElement> iter =
                    element.Elements().GetEnumerator();
                while (iter.MoveNext()) {
                    XElement childElement = iter.Current;

                    if (childElement.Name != "field") {
                        PrintLineError(childElement,
                            "expect a `field` node");
                        return false;
                    }

                    if (AddStructFieldDef(def, childElement) == false) {
                        return false;
                    }
                }
            }

            if (tableDef == null) {
                this.descriptor.GlobalStructs.Add(def);
                this.descriptor.GlobalStructNameIndex.Add(def.Name, def);
            } else {
                tableDef.LocalStructs.Add(def);
                tableDef.LocalStructNameIndex.Add(def.Name, def);
            }

            return true;
        }

        private bool AddStructFieldDef(
            TableDescriptor.StructDef structDef, XElement element)
        {
            // check name attr
            string name;
            {
                XAttribute attr = element.Attribute("name");
                if (attr == null) {
                    PrintLineError(element,
                        "`field` node must contain a `name` attribute");
                    return false;
                }
                name = attr.Value;
            }
            if (Regex.IsMatch(name, @"^[a-zA-Z_]\w*$") == false) {
                PrintLineError(element,
                    "`field` node `name` attribute is invalid");
                return false;
            }
            if (structDef.FieldNameIndex.ContainsKey(name)) {
                PrintLineError(element,
                    "`field` node `name` attribute duplicated");
                return false;
            }

            // check type attr
            string type;
            {
                XAttribute attr = element.Attribute("type");
                if (attr == null) {
                    PrintLineError(element,
                        "`field` node must contain a `type` attribute");
                    return false;
                }
                type = attr.Value;
            }

            TableDescriptor.StructDef.FieldDef def =
                new TableDescriptor.StructDef.FieldDef();
            def.ParentRef = structDef;
            def.Name = name;
            def.LineNumber = GetLineNumber(element);

            if (type == "int") {
                def.Type = TableDescriptor.StructDef.FieldType.Int;
            } else if (type == "string") {
                def.Type = TableDescriptor.StructDef.FieldType.String;
            } else {
                PrintLineError(element,
                    "type `{0}` is invalid", type);
                return false;
            }

            structDef.Fields.Add(def);
            structDef.FieldNameIndex.Add(def.Name, def);

            return true;
        }

        private bool AddTableDef(XElement element)
        {
            // check name attr
            string name;
            {
                XAttribute attr = element.Attribute("name");
                if (attr == null) {
                    PrintLineError(element,
                        "`table` node must contain a `name` attribute");
                    return false;
                }
                name = attr.Value;
            }
            if (Regex.IsMatch(name, @"^[a-zA-Z_]\w*$") == false) {
                PrintLineError(element,
                    "`table` node `name` attribute is invalid");
                return false;
            }
            if (this.descriptor.TableNameIndex.ContainsKey(name) ||
                this.descriptor.GlobalStructNameIndex.ContainsKey(name)) {
                PrintLineError(element,
                    "`table` node `name` attribute duplicated");
                return false;
            }

            TableDescriptor.TableDef def =
                new TableDescriptor.TableDef();
            def.Name = name;
            def.LineNumber = GetLineNumber(element);

            {
                IEnumerator<XElement> iter =
                    element.Elements().GetEnumerator();
                while (iter.MoveNext()) {
                    XElement childElement = iter.Current;

                    if (childElement.Name == "struct") {
                        // parse local struct
                        if (AddStructDef(def, childElement) == false) {
                            return false;
                        }
                    } else if (childElement.Name == "col") {
                        // parse column
                        if (AddTableColumnDef(def, childElement) == false) {
                            return false;
                        }
                    } else {
                        PrintLineError(childElement,
                            "expect a `struct` or `col` node");
                        return false;
                    }
                }
            }

            // check key/setkey attr
            {
                string key;

                XAttribute attr = element.Attribute("key");
                if (attr != null) {
                    key = attr.Value;
                    def.TableKeyType =
                        TableDescriptor.TableDef.KeyType.SingleKey;
                } else {
                    attr = element.Attribute("setkey");
                    if (attr != null) {
                        key = attr.Value;
                        def.TableKeyType =
                            TableDescriptor.TableDef.KeyType.SetKey;
                    } else {
                        PrintLineError(element,
                            "`table` node must contain a " +
                            "`key` or `setkey` attribute");
                        return false;
                    }
                }

                TableDescriptor.TableDef.ColumnDef tableKey = null;
                if (def.ColumnNameIndex.TryGetValue(
                        key, out tableKey) == false) {
                    PrintLineError(element,
                        "table key `{0}` is not defined", key);
                    return false;
                }
                if (tableKey.Type != TableDescriptor.TableDef.ColumnType.Int &&
                    tableKey.Type != TableDescriptor.TableDef.ColumnType.String) {
                    PrintLineError(element,
                        "table key can only be `int` or `string` type");
                    return false;
                }
                def.TableKey = tableKey;
            }

            // check file attr
            {
                XAttribute attr = element.Attribute("file");
                if (attr == null) {
                    PrintLineError(element,
                        "`table` node must contain a `file` attribute");
                    return false;
                }
                def.FileName = attr.Value;
            }

            // check readby attr
            {
                XAttribute attr = element.Attribute("readby");
                if (attr != null) {
                    string[] readers = attr.Value.Split('|');
                    for (int i = 0; i < readers.Length; ++i) {
                        string reader = readers[i];

                        TableDescriptor.ReaderDef readerDef = null;
                        if (this.descriptor.Readers.TryGetValue(
                                reader, out readerDef) == false) {
                            PrintLineError(element,
                                "reader `{0}` is not defined", reader);
                            return false;
                        }
                        def.Readers[reader] = readerDef;
                    }
                }
            }

            CalculateTableKeyColumnIndex(def);
            this.descriptor.Tables.Add(def);
            this.descriptor.TableNameIndex.Add(def.Name, def);

            return true;
        }

        private bool AddTableColumnDef(
            TableDescriptor.TableDef tableDef, XElement element)
        {
            // check name attr
            string name;
            {
                XAttribute attr = element.Attribute("name");
                if (attr == null) {
                    PrintLineError(element,
                        "`col` node must contain a `name` attribute");
                    return false;
                }
                name = attr.Value;
            }
            if (Regex.IsMatch(name, @"^[a-zA-Z_]\w*$") == false) {
                PrintLineError(element,
                    "`col` node `name` attribute is invalid");
                return false;
            }
            if (tableDef.ColumnNameIndex.ContainsKey(name)) {
                PrintLineError(element,
                    "`col` node `name` attribute duplicated");
                return false;
            }

            // check type attr
            string type;
            {
                XAttribute attr = element.Attribute("type");
                if (attr == null) {
                    PrintLineError(element,
                        "`col` node must contain a `type` attribute");
                    return false;
                }
                type = attr.Value;
            }

            TableDescriptor.TableDef.ColumnDef def =
                new TableDescriptor.TableDef.ColumnDef();
            def.ParentRef = tableDef;
            def.Name = name;
            def.LineNumber = GetLineNumber(element);

            // get type info
            string columnTypeStr = type;
            {
                Match m = Regex.Match(type, @"^list{(.+)}$");
                if (m.Success) {
                    columnTypeStr = m.Groups[1].Value;
                    def.Type = TableDescriptor.TableDef.ColumnType.List;
                }
            }

            TableDescriptor.TableDef.ColumnType columnType =
                TableDescriptor.TableDef.ColumnType.None;
            if (columnTypeStr == "int") {
                columnType = TableDescriptor.TableDef.ColumnType.Int;
            } else if (columnTypeStr == "string") {
                columnType = TableDescriptor.TableDef.ColumnType.String;
            } else {
                for (;;) {
                    TableDescriptor.StructDef refStructDef = null;

                    // check is local struct
                    if (tableDef.LocalStructNameIndex.TryGetValue(
                            columnTypeStr, out refStructDef)) {
                        columnType = TableDescriptor.TableDef.ColumnType.Struct;
                        def.RefStructDef = refStructDef;
                        break;
                    }

                    // check is global struct
                    if (this.descriptor.GlobalStructNameIndex.TryGetValue(
                            columnTypeStr, out refStructDef)) {
                        columnType = TableDescriptor.TableDef.ColumnType.Struct;
                        def.RefStructDef = refStructDef;
                        break;
                    }

                    PrintLineError(element,
                        "type `{0}` is undefined", columnTypeStr);
                    return false;
                }
            }

            if (def.Type == TableDescriptor.TableDef.ColumnType.List) {
                def.ListType = columnType;
            } else {
                def.Type = columnType;
            }

            // check readby attr
            {
                XAttribute attr = element.Attribute("readby");
                if (attr != null) {
                    string[] readers = attr.Value.Split('|');
                    for (int i = 0; i < readers.Length; ++i) {
                        string reader = readers[i];

                        TableDescriptor.ReaderDef readerDef = null;
                        if (this.descriptor.Readers.TryGetValue(
                                reader, out readerDef) == false) {
                            PrintLineError(element,
                                "reader `{0}` is not defined", reader);
                            return false;
                        }
                        def.Readers[reader] = readerDef;
                    }
                }
            }

            tableDef.Columns.Add(def);
            tableDef.ColumnNameIndex.Add(def.Name, def);

            return true;
        }

        private void CalculateTableKeyColumnIndex(
            TableDescriptor.TableDef tableDef)
        {
            for (int i = 0; i < tableDef.Columns.Count; ++i) {
                TableDescriptor.TableDef.ColumnDef columnDef =
                    tableDef.Columns[i];
                if (columnDef == tableDef.TableKey) {
                    tableDef.TableKeyColumnIndex = i;
                    return;
                }
            }
        }

    }
}
