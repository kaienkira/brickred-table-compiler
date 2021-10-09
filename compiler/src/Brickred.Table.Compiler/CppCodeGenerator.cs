using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Brickred.Table.Compiler
{
    using ColumnType = TableDescriptor.TableDef.ColumnType;
    using FieldType = TableDescriptor.StructDef.FieldType;
    using TableKeyType = TableDescriptor.TableDef.KeyType;

    public sealed class CppCodeGenerator : BaseCodeGenerator
    {
        private TableDescriptor descriptor = null;
        private string reader = "";
        private string newLineStr = "";

        public CppCodeGenerator()
        {
        }

        public override void Dispose()
        {
            this.newLineStr = "";

            this.reader = "";
            if (this.descriptor != null) {
                this.descriptor = null;
            }
        }

        public override bool Generate(
            TableDescriptor descriptor, string reader,
            string outputDir, NewLineType newLineType)
        {
            this.descriptor = descriptor;
            this.reader = reader;

            if (newLineType == NewLineType.Dos) {
                this.newLineStr = "\r\n";
            } else {
                this.newLineStr = "\n";
            }

            for (int i = 0; i < this.descriptor.GlobalStructs.Count; ++i) {
                TableDescriptor.StructDef def =
                    this.descriptor.GlobalStructs[i];
                string underscoreName = CamelToUnderscore(def.Name);

                string headerFilePath = Path.Combine(
                    outputDir, underscoreName + ".h");
                string headerFileContent = GenerateGlobalStructHeaderFile(def);
                try {
                    File.WriteAllText(headerFilePath, headerFileContent);
                } catch (Exception e) {
                    Console.Error.WriteLine(string.Format(
                        "error: write file {0} failed: {1}",
                        headerFilePath, e.Message));
                    return false;
                }

                string sourceFilePath = Path.Combine(
                    outputDir, underscoreName + ".cc");
                string sourceFileContent = GenerateGlobalStructSourceFile(def);
                try {
                    File.WriteAllText(sourceFilePath, sourceFileContent);
                } catch (Exception e) {
                    Console.Error.WriteLine(string.Format(
                        "error: write file {0} failed: {1}",
                        sourceFilePath, e.Message));
                    return false;
                }
            }

            for (int i = 0; i < this.descriptor.Tables.Count; ++i) {
                TableDescriptor.TableDef def = this.descriptor.Tables[i];
                string underscoreName = CamelToUnderscore(def.Name);

                string headerFilePath = Path.Combine(
                    outputDir, underscoreName + ".h");
                string headerFileContent = GenerateTableHeaderFile(def);
                try {
                    File.WriteAllText(headerFilePath, headerFileContent);
                } catch (Exception e) {
                    Console.Error.WriteLine(string.Format(
                        "error: write file {0} failed: {1}",
                        headerFilePath, e.Message));
                    return false;
                }

                string sourceFilePath = Path.Combine(
                    outputDir, underscoreName + ".cc");
                string sourceFileContent = GenerateTableSourceFile(def);
                try {
                    File.WriteAllText(sourceFilePath, sourceFileContent);
                } catch (Exception e) {
                    Console.Error.WriteLine(string.Format(
                        "error: write file {0} failed: {1}",
                        sourceFilePath, e.Message));
                    return false;
                }
            }

            return true;
        }

        private string GenerateGlobalStructHeaderFile(
            TableDescriptor.StructDef structDef)
        {
            string dontEditComment;
            string includeGuardStart;
            string includeGuardEnd;
            string includeFileDecl;
            string namespaceDeclStart;
            string namespaceDeclEnd;
            string structDecl;

            GetDontEditComment(
                out dontEditComment);
            GetNamespaceDecl(
                out namespaceDeclStart, out namespaceDeclEnd);
            GetGlobalStructHeaderFileIncludeGuard(structDef,
                out includeGuardStart, out includeGuardEnd);
            GetGlobalStructHeaderFileIncludeFileDecl(structDef,
                out includeFileDecl);
            GetHeaderFileStructDecl(structDef, "",
                out structDecl);

            StringBuilder sb = new StringBuilder();
            sb.Append(dontEditComment);
            sb.Append(includeGuardStart);
            sb.Append(this.newLineStr);
            if (includeFileDecl != "") {
                sb.Append(includeFileDecl);
                sb.Append(this.newLineStr);
            }
            if (namespaceDeclStart != "") {
                sb.Append(namespaceDeclStart);
                sb.Append(this.newLineStr);
            }
            sb.Append(structDecl);
            sb.Append(this.newLineStr);
            if (namespaceDeclEnd != "") {
                sb.Append(namespaceDeclEnd);
                sb.Append(this.newLineStr);
            }
            sb.Append(includeGuardEnd);

            return sb.ToString();
        }

        private string GenerateGlobalStructSourceFile(
            TableDescriptor.StructDef structDef)
        {
            string dontEditComment;
            string namespaceDeclStart;
            string namespaceDeclEnd;
            string includeFileDecl;
            string structImpl;

            GetDontEditComment(
                out dontEditComment);
            GetNamespaceDecl(
                out namespaceDeclStart, out namespaceDeclEnd);
            GetGlobalStructSourceFileIncludeFileDecl(structDef,
                out includeFileDecl);
            GetSourceFileStructImpl(structDef,
                out structImpl);

            StringBuilder sb = new StringBuilder();
            sb.Append(dontEditComment);
            sb.Append(includeFileDecl);
            sb.Append(this.newLineStr);
            if (namespaceDeclStart != "") {
                sb.Append(namespaceDeclStart);
                sb.Append(this.newLineStr);
            }
            sb.Append(structImpl);
            if (namespaceDeclEnd != "") {
                sb.Append(this.newLineStr);
                sb.Append(namespaceDeclEnd);
            }

            return sb.ToString();
        }

        private string GenerateTableHeaderFile(
            TableDescriptor.TableDef tableDef)
        {
            string dontEditComment;
            string includeGuardStart;
            string includeGuardEnd;
            string includeFileDecl;
            string namespaceDeclStart;
            string namespaceDeclEnd;
            string tableDecl;

            GetDontEditComment(
                out dontEditComment);
            GetNamespaceDecl(
                out namespaceDeclStart, out namespaceDeclEnd);
            GetTableHeaderFileIncludeGuard(tableDef,
                out includeGuardStart, out includeGuardEnd);
            GetTableHeaderFileIncludeFileDecl(tableDef,
                out includeFileDecl);
            GetTableHeaderFileTableDecl(tableDef,
                out tableDecl);

            StringBuilder sb = new StringBuilder();
            sb.Append(dontEditComment);
            sb.Append(includeGuardStart);
            sb.Append(this.newLineStr);
            if (includeFileDecl != "") {
                sb.Append(includeFileDecl);
                sb.Append(this.newLineStr);
            }
            if (namespaceDeclStart != "") {
                sb.Append(namespaceDeclStart);
                sb.Append(this.newLineStr);
            }
            sb.Append(tableDecl);
            sb.Append(this.newLineStr);
            if (namespaceDeclEnd != "") {
                sb.Append(namespaceDeclEnd);
                sb.Append(this.newLineStr);
            }
            sb.Append(includeGuardEnd);

            return sb.ToString();
        }

        private string GenerateTableSourceFile(
            TableDescriptor.TableDef tableDef)
        {
            string dontEditComment;
            string namespaceDeclStart;
            string namespaceDeclEnd;
            string includeFileDecl;
            string tableImpl;
            List<string> implList = new List<string>();

            GetDontEditComment(
                out dontEditComment);
            GetNamespaceDecl(
                out namespaceDeclStart, out namespaceDeclEnd);
            GetTableSourceFileIncludeFileDecl(tableDef,
                out includeFileDecl);

            for (int i = 0; i < tableDef.LocalStructs.Count; ++i) {
                string impl;
                GetSourceFileStructImpl(tableDef.LocalStructs[i], out impl);
                implList.Add(impl);
            }

            GetTableSourceFileTableImpl(tableDef,
                out tableImpl);
            implList.Add(tableImpl);

            StringBuilder sb = new StringBuilder();
            sb.Append(dontEditComment);
            sb.Append(includeFileDecl);
            sb.Append(this.newLineStr);
            if (namespaceDeclStart != "") {
                sb.Append(namespaceDeclStart);
                sb.Append(this.newLineStr);
            }
            sb.Append(string.Join(this.newLineStr, implList));
            if (namespaceDeclEnd != "") {
                sb.Append(this.newLineStr);
                sb.Append(namespaceDeclEnd);
            }

            return sb.ToString();
        }

        private string CamelToUnderscore(string camelName)
        {
            return Regex.Replace(camelName,
                @"(?<=[a-z0-9])([A-Z])", "_$0").ToLower();
        }

        private void GetDontEditComment(out string output)
        {
            output = string.Format(
                "/*{0}" +
                " * Generated by brickred table compiler.{0}" +
                " * Do not edit unless you are sure that you know what you are doing.{0}" +
                " */{0}",
                this.newLineStr);
        }

        private string GetCppType(
            TableDescriptor.StructDef.FieldDef fieldDef)
        {
            string cppType = "";
            if (fieldDef.Type == FieldType.Int) {
                cppType = "int32_t";
            } else if (fieldDef.Type == FieldType.String) {
                cppType = "std::string";
            }

            return cppType;
        }

        private string GetCppType(
            TableDescriptor.TableDef.ColumnDef columnDef)
        {
            ColumnType checkType;
            if (columnDef.Type == ColumnType.List) {
                checkType = columnDef.ListType;
            } else {
                checkType = columnDef.Type;
            }

            string cppType = "";
            if (checkType == ColumnType.Int) {
                cppType = "int32_t";
            } else if (checkType == ColumnType.String) {
                cppType = "std::string";
            } else if (checkType == ColumnType.Struct) {
                cppType = columnDef.RefStructDef.Name;
            }

            if (columnDef.Type == ColumnType.List) {
                return string.Format("std::vector<{0}>", cppType);
            } else {
                return cppType;
            }
        }

        private void GetNamespaceDecl(
            out string start, out string end)
        {
            start = "";
            end = "";

            TableDescriptor.ReaderDef readerDef = null;
            if (this.descriptor.Readers.TryGetValue(
                    this.reader, out readerDef) == false) {
                return;
            }

            string namespaceName =
                string.Join("::", readerDef.NamespaceParts);

            start = string.Format("namespace {0} {{{1}",
                namespaceName, this.newLineStr);
            end = string.Format("}} // namespace {0}{1}",
                namespaceName, this.newLineStr);
        }

        private void GetHeaderFileStructDecl(
            TableDescriptor.StructDef structDef,
            string indent, out string output)
        {
            StringBuilder sb = new StringBuilder();

            string start = string.Format(
                "{0}class {1} {{{2}" +
                "{0}public:{2}" +
                "{0}    {1}();{2}" +
                "{0}    ~{1}();{2}" +
                "{2}" +
                "{0}    bool parse(const std::string &text);{2}",
                indent, structDef.Name, this.newLineStr);
            string end = string.Format(
                "{0}}};{1}", indent, this.newLineStr);

            sb.Append(start);

            if (structDef.Fields.Count > 0) {
                sb.Append(this.newLineStr);
                sb.AppendFormat("{0}public:{1}",
                    indent, this.newLineStr);
            }

            indent += "    ";
            for (int i = 0; i < structDef.Fields.Count; ++i) {
                TableDescriptor.StructDef.FieldDef fieldDef =
                    structDef.Fields[i];

                string cppType = GetCppType(fieldDef);
                sb.AppendFormat("{0}{1} {2};{3}",
                    indent, cppType, fieldDef.Name,
                    this.newLineStr);
            }

            sb.Append(end);

            output = sb.ToString();
        }

        private void GetSourceFileStructImpl(
            TableDescriptor.StructDef structDef, out string output)
        {
            StringBuilder sb = new StringBuilder();

            string constructorImpl;
            string destructorImpl;
            string parseFuncImpl;

            GetSourceFileStructImplConstructor(
                structDef, out constructorImpl);
            GetSourceFileStructImplDestructor(
                structDef, out destructorImpl);
            GetSourceFileStructImplParseFunc(
                structDef, out parseFuncImpl);

            sb.Append(constructorImpl);
            sb.Append(this.newLineStr);
            sb.Append(destructorImpl);
            sb.Append(this.newLineStr);
            sb.Append(parseFuncImpl);

            output = sb.ToString();
        }

        private void GetSourceFileStructImplConstructor(
            TableDescriptor.StructDef structDef, out string output)
        {
            StringBuilder sb = new StringBuilder();

            List<string> initListImpl = new List<string>();

            for (int i = 0; i < structDef.Fields.Count; ++i) {
                TableDescriptor.StructDef.FieldDef fieldDef =
                    structDef.Fields[i];
                if (fieldDef.Type == FieldType.Int) {
                    initListImpl.Add(string.Format(
                        "    {0}(0)", fieldDef.Name));
                }
            }

            string initList = "";
            if (initListImpl.Count > 0) {
                initList = string.Format(" :{0}", this.newLineStr) +
                    string.Join(string.Format(",{0}", this.newLineStr),
                        initListImpl);
            }

            string parentClassPrefix = "";
            if (structDef.ParentRef != null) {
                parentClassPrefix = string.Format("{0}::",
                    structDef.ParentRef.Name);
            }

            string start = string.Format(
                "{0}{1}::{1}(){2}{3}" +
                "{{{3}",
                parentClassPrefix, structDef.Name,
                initList, this.newLineStr);
            string end = string.Format(
                "}}{0}", this.newLineStr);

            sb.Append(start);
            sb.Append(end);

            output = sb.ToString();
        }

        private void GetSourceFileStructImplDestructor(
            TableDescriptor.StructDef structDef, out string output)
        {
            string parentClassPrefix = "";
            if (structDef.ParentRef != null) {
                parentClassPrefix = string.Format("{0}::",
                    structDef.ParentRef.Name);
            }

            output = string.Format(
                "{0}{1}::~{1}(){2}" +
                "{{{2}" +
                "}}{2}",
                parentClassPrefix, structDef.Name, this.newLineStr);
        }

        private void GetSourceFileStructImplParseFunc(
            TableDescriptor.StructDef structDef, out string output)
        {
            StringBuilder sb = new StringBuilder();

            string parentClassPrefix = "";
            if (structDef.ParentRef != null) {
                parentClassPrefix = string.Format("{0}::",
                    structDef.ParentRef.Name);
            }

            string start = string.Format(
                "bool {0}{1}::parse(const std::string &text){2}" +
                "{{{2}",
                parentClassPrefix, structDef.Name, this.newLineStr);
            string end = string.Format(
                "}}{0}", this.newLineStr);
            string indent = "    ";

            sb.Append(start);

            if (structDef.Fields.Count == 0) {
                sb.AppendFormat(
                    "{0}return true;{1}",
                    indent, this.newLineStr);
            } else {
                sb.AppendFormat(
                    "{0}brickred::table::ColumnSpliter s(text, ';');{1}" +
                    "{1}",
                    indent, this.newLineStr);

                for (int i = 0; i < structDef.Fields.Count; ++i) {
                    TableDescriptor.StructDef.FieldDef fieldDef =
                        structDef.Fields[i];

                    if (fieldDef.Type == FieldType.Int) {
                        sb.AppendFormat(
                            "{0}if (s.nextInt(&this->{1}) == false) {{{2}" +
                            "{0}    return false;{2}" +
                            "{0}}}{2}",
                            indent, fieldDef.Name, this.newLineStr);
                    } else if (fieldDef.Type == FieldType.String) {
                        sb.AppendFormat(
                            "{0}if (s.nextString(&this->{1}) == false) {{{2}" +
                            "{0}    return false;{2}" +
                            "{0}}}{2}",
                            indent, fieldDef.Name, this.newLineStr);
                    }
                }

                sb.AppendFormat(
                    "{0}if (s.nextString(nullptr)) {{{1}" +
                    "{0}    return false;{1}" +
                    "{0}}}{1}" +
                    "{1}" +
                    "{0}return true;{1}",
                    indent, this.newLineStr);
            }

            sb.Append(end);

            output = sb.ToString();
        }

        private void GetGlobalStructHeaderFileIncludeGuard(
            TableDescriptor.StructDef structDef,
            out string start, out string end)
        {
            List<string> guardNameParts = new List<string>();

            guardNameParts.Add("BRICKRED_TABLE_GENERATED");

            TableDescriptor.ReaderDef readerDef = null;
            if (this.descriptor.Readers.TryGetValue(
                    this.reader, out readerDef)) {
                guardNameParts.AddRange(readerDef.NamespaceParts);
            }

            guardNameParts.Add(Regex.Replace(
                CamelToUnderscore(structDef.Name), @"[^\w]", "_"));
            guardNameParts.Add("H");

            string guardName = string.Join("_", guardNameParts).ToUpper();

            start = string.Format(
                "#ifndef {0}{1}" +
                "#define {0}{1}",
                guardName, this.newLineStr);
            end = string.Format(
                "#endif{0}",
                this.newLineStr);
        }

        private void GetGlobalStructHeaderFileIncludeFileDecl(
            TableDescriptor.StructDef structDef, out string output)
        {
            bool useStdIntH = false;

            {
                for (int i = 0; i < structDef.Fields.Count; ++i) {
                    TableDescriptor.StructDef.FieldDef fieldDef =
                        structDef.Fields[i];

                    if (fieldDef.Type == FieldType.Int) {
                        useStdIntH = true;
                    }
                }
            }

            string systemHeaderDecl = "";
            {
                StringBuilder sb = new StringBuilder();

                if (useStdIntH) {
                    sb.AppendFormat("#include <cstdint>{0}",
                        this.newLineStr);
                }

                sb.AppendFormat("#include <string>{0}",
                    this.newLineStr);

                systemHeaderDecl = sb.ToString();
            }

            {
                StringBuilder sb = new StringBuilder();

                sb.Append(systemHeaderDecl);

                output = sb.ToString();
            }
        }

        private void GetGlobalStructSourceFileIncludeFileDecl(
            TableDescriptor.StructDef structDef, out string output)
        {
            string customHeaderDecl = "";
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat(
                    "#include <brickred/table/column_spliter.h>{0}",
                    this.newLineStr);

                customHeaderDecl = sb.ToString();
            }

            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(
                    "#include \"{0}.h\"{1}",
                    CamelToUnderscore(structDef.Name),
                    this.newLineStr);
                sb.Append(this.newLineStr);
                sb.Append(customHeaderDecl);

                output = sb.ToString();
            }
        }

        private void GetTableHeaderFileIncludeGuard(
            TableDescriptor.TableDef tableDef,
            out string start, out string end)
        {
            List<string> guardNameParts = new List<string>();

            guardNameParts.Add("BRICKRED_TABLE_GENERATED");

            TableDescriptor.ReaderDef readerDef = null;
            if (this.descriptor.Readers.TryGetValue(
                    this.reader, out readerDef)) {
                guardNameParts.AddRange(readerDef.NamespaceParts);
            }

            guardNameParts.Add(Regex.Replace(
                CamelToUnderscore(tableDef.Name), @"[^\w]", "_"));
            guardNameParts.Add("H");

            string guardName = string.Join("_", guardNameParts).ToUpper();

            start = string.Format(
                "#ifndef {0}{1}" +
                "#define {0}{1}",
                guardName, this.newLineStr);
            end = string.Format(
                "#endif{0}",
                this.newLineStr);
        }

        private void GetTableHeaderFileIncludeFileDecl(
            TableDescriptor.TableDef tableDef, out string output)
        {
            bool useStdIntH = false;
            bool useVectorH = false;
            bool useMapH = false;
            Dictionary<string, TableDescriptor.StructDef> refGlobalStructDefs =
                new Dictionary<string, TableDescriptor.StructDef>();

            {
                if (tableDef.TableKeyType == TableKeyType.SingleKey) {
                    useMapH = true;
                } else if (tableDef.TableKeyType == TableKeyType.SetKey) {
                    useMapH = true;
                    useVectorH = true;
                }

                for (int i = 0; i < tableDef.Columns.Count; ++i) {
                    TableDescriptor.TableDef.ColumnDef columnDef =
                        tableDef.Columns[i];

                    ColumnType checkType;
                    if (columnDef.Type == ColumnType.List) {
                        checkType = columnDef.ListType;
                        useVectorH = true;
                    } else {
                        checkType = columnDef.Type;
                    }

                    if (checkType == ColumnType.Int) {
                        useStdIntH = true;
                    } else if (checkType == ColumnType.Struct) {
                        if (columnDef.RefStructDef.ParentRef == null) {
                            refGlobalStructDefs[columnDef.RefStructDef.Name] =
                                columnDef.RefStructDef;
                        }
                    }
                }

                for (int i = 0; i < tableDef.LocalStructs.Count; ++i) {
                    TableDescriptor.StructDef structDef =
                        tableDef.LocalStructs[i];

                    for (int j = 0; j < structDef.Fields.Count; ++j) {
                        TableDescriptor.StructDef.FieldDef fieldDef =
                            structDef.Fields[j];

                        if (fieldDef.Type == FieldType.Int) {
                            useStdIntH = true;
                        }
                    }
                }
            }

            string systemHeaderDecl = "";
            {
                StringBuilder sb = new StringBuilder();

                if (useStdIntH) {
                    sb.AppendFormat("#include <cstdint>{0}",
                        this.newLineStr);
                }
                if (useMapH) {
                    sb.AppendFormat("#include <map>{0}",
                        this.newLineStr);
                }

                sb.AppendFormat("#include <string>{0}",
                    this.newLineStr);

                if (useVectorH) {
                    sb.AppendFormat("#include <vector>{0}",
                        this.newLineStr);
                }

                systemHeaderDecl = sb.ToString();
            }

            string customHeaderDecl = "";
            {
                StringBuilder sb = new StringBuilder();

                Dictionary<string, TableDescriptor.StructDef>.Enumerator iter =
                    refGlobalStructDefs.GetEnumerator();
                while (iter.MoveNext()) {
                    TableDescriptor.StructDef structDef =
                        iter.Current.Value;

                    sb.AppendFormat(
                        "#include \"{0}.h\"{1}",
                        CamelToUnderscore(structDef.Name),
                        this.newLineStr);
                }

                customHeaderDecl = sb.ToString();
            }

            {
                StringBuilder sb = new StringBuilder();

                sb.Append(systemHeaderDecl);
                if (customHeaderDecl != "") {
                    sb.Append(this.newLineStr);
                    sb.Append(customHeaderDecl);
                }

                output = sb.ToString();
            }
        }

        private void GetTableHeaderFileTableDecl(
            TableDescriptor.TableDef tableDef, out string output)
        {
            StringBuilder sb = new StringBuilder();

            string start = string.Format(
                "class {0} {{{1}" +
                "public:{1}",
                tableDef.Name, this.newLineStr);
            string end = string.Format(
                "}};{0}", this.newLineStr);
            string rowClassDecl;
            string funcDecl;
            string memberDecl;

            GetTableHeaderFileTableDeclRowClassDecl(
                tableDef, out rowClassDecl);
            GetTableHeaderFileTableDeclFuncDecl(
                tableDef, out funcDecl);
            GetTableHeaderFileTableDeclMemberDecl(
                tableDef, out memberDecl);

            sb.Append(start);

            string indent = "    ";

            for (int i = 0; i < tableDef.LocalStructs.Count; ++i) {
                string decl;
                GetHeaderFileStructDecl(tableDef.LocalStructs[i],
                    indent, out decl);
                sb.Append(decl);
                sb.Append(this.newLineStr);
            }

            if (rowClassDecl != "") {
                sb.Append(rowClassDecl);
                sb.Append(this.newLineStr);
            }
            sb.Append(funcDecl);
            sb.Append(this.newLineStr);
            sb.Append(memberDecl);

            sb.Append(end);

            output = sb.ToString();
        }

        private void GetTableHeaderFileTableDeclRowClassDecl(
            TableDescriptor.TableDef tableDef, out string output)
        {
            StringBuilder sb = new StringBuilder();
            string indent = "    ";

            string start = string.Format(
                "{0}class Row {{{1}" +
                "{0}public:{1}" +
                "{0}    Row();{1}" +
                "{0}    ~Row();{1}",
                indent, this.newLineStr);
            string end = string.Format(
                "{0}}};{1}", indent, this.newLineStr);

            sb.Append(start);

            if (tableDef.Columns.Count > 0) {
                sb.Append(this.newLineStr);
                sb.AppendFormat("{0}public:{1}",
                    indent, this.newLineStr);
            }

            for (int i = 0; i < tableDef.Columns.Count; ++i) {
                TableDescriptor.TableDef.ColumnDef columnDef =
                    tableDef.Columns[i];

                string cppType = GetCppType(columnDef);
                sb.AppendFormat("{0}    {1} {2};{3}",
                    indent, cppType, columnDef.Name,
                    this.newLineStr);
            }

            sb.Append(end);

            if (tableDef.TableKeyType == TableKeyType.SingleKey) {
                sb.Append(this.newLineStr);
                sb.AppendFormat(
                    "{0}using Rows = std::map<{1}, Row>;{2}",
                    indent, GetCppType(tableDef.TableKey), this.newLineStr);
            } else if (tableDef.TableKeyType == TableKeyType.SetKey) {
                sb.Append(this.newLineStr);
                sb.AppendFormat(
                    "{0}using RowSet = std::vector<Row>;{1}" +
                    "{0}using RowSets = std::map<{2}, RowSet>;{1}",
                    indent, this.newLineStr, GetCppType(tableDef.TableKey));
            }

            output = sb.ToString();
        }

        private void GetTableHeaderFileTableDeclFuncDecl(
            TableDescriptor.TableDef tableDef, out string output)
        {
            StringBuilder sb = new StringBuilder();
            string indent = "    ";

            sb.AppendFormat(
                "{0}{1}();{2}" +
                "{0}~{1}();{2}" +
                "{2}" +
                "{0}bool parse(const std::string &text, " +
                "std::string *error_info);{2}",
                indent, tableDef.Name, this.newLineStr);

            string cppType = GetCppType(tableDef.TableKey);
            if (tableDef.TableKey.Type == ColumnType.String) {
                cppType = string.Format("const {0} &", cppType);
            } else {
                cppType = string.Format("{0} ", cppType);
            }

            if (tableDef.TableKeyType == TableKeyType.SingleKey) {
                sb.AppendFormat(
                    "{0}const Row *getRow({1}key) const;{2}" +
                    "{0}const Rows &getRows() const " +
                    "{{ return rows_; }}{2}",
                    indent, cppType, this.newLineStr);

            } else if (tableDef.TableKeyType == TableKeyType.SetKey) {
                sb.AppendFormat(
                    "{0}const RowSet *getRowSet({1}key) const;{2}" +
                    "{0}const RowSets &getRowSets() const " +
                    "{{ return row_sets_; }}{2}",
                    indent, cppType, this.newLineStr);
            }

            output = sb.ToString();
        }

        private void GetTableHeaderFileTableDeclMemberDecl(
            TableDescriptor.TableDef tableDef, out string output)
        {
            StringBuilder sb = new StringBuilder();
            string indent = "    ";

            sb.AppendFormat("private:{0}", this.newLineStr);
            if (tableDef.TableKeyType == TableKeyType.SingleKey) {
                sb.AppendFormat(
                    "{0}Rows rows_;{1}",
                    indent, this.newLineStr);
            } else if (tableDef.TableKeyType == TableKeyType.SetKey) {
                sb.AppendFormat(
                    "{0}RowSets row_sets_;{1}",
                    indent, this.newLineStr);
            }

            output = sb.ToString();
        }

        private void GetTableSourceFileIncludeFileDecl(
            TableDescriptor.TableDef tableDef, out string output)
        {
            bool useCStdlibH = false;
            bool useBrickredTableColumnSpliterH = false;

            {
                for (int i = 0; i < tableDef.Columns.Count; ++i) {
                    TableDescriptor.TableDef.ColumnDef columnDef =
                        tableDef.Columns[i];

                    if (columnDef.Type == ColumnType.Int) {
                        useCStdlibH = true;
                    } else if (columnDef.Type == ColumnType.Struct ||
                               columnDef.Type == ColumnType.List) {
                        useBrickredTableColumnSpliterH = true;
                    }
                }
            }

            string systemHeaderDecl = "";
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("#include <cstddef>{0}",
                    this.newLineStr);

                if (useCStdlibH) {
                    sb.AppendFormat("#include <cstdlib>{0}",
                        this.newLineStr);
                }

                systemHeaderDecl = sb.ToString();
            }

            string customHeaderDecl = "";
            {
                StringBuilder sb = new StringBuilder();

                if (useBrickredTableColumnSpliterH) {
                    sb.AppendFormat(
                        "#include <brickred/table/column_spliter.h>{0}",
                        this.newLineStr);
                }

                sb.AppendFormat(
                    "#include <brickred/table/line_reader.h>{0}",
                    this.newLineStr);
                sb.AppendFormat(
                    "#include <brickred/table/util.h>{0}",
                    this.newLineStr);

                customHeaderDecl = sb.ToString();
            }

            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat(
                    "#include \"{0}.h\"{1}",
                    CamelToUnderscore(tableDef.Name),
                    this.newLineStr);
                if (systemHeaderDecl != "") {
                    sb.Append(this.newLineStr);
                    sb.Append(systemHeaderDecl);
                }
                if (customHeaderDecl != "") {
                    sb.Append(this.newLineStr);
                    sb.Append(customHeaderDecl);
                }

                output = sb.ToString();
            }
        }

        private void GetTableSourceFileTableImpl(
            TableDescriptor.TableDef tableDef, out string output)
        {
            StringBuilder sb = new StringBuilder();

            string rowConstructorImpl;
            string rowDestructorImpl;
            string constructorImpl;
            string destructorImpl;
            string parseFuncImpl;
            string getRowFuncImpl = "";

            GetTableSourceFileTableImplRowConstructor(
                tableDef, out rowConstructorImpl);
            GetTableSourceFileTableImplRowDestructor(
                tableDef, out rowDestructorImpl);
            GetTableSourceFileTableImplConstructor(
                tableDef, out constructorImpl);
            GetTableSourceFileTableImplDestructor(
                tableDef, out destructorImpl);
            GetTableSourceFileTableImplParseFunc(
                tableDef, out parseFuncImpl);

            if (tableDef.TableKeyType == TableKeyType.SingleKey) {
                GetTableSourceFileTableImplGetRowFunc(
                    tableDef, out getRowFuncImpl);
            } else if (tableDef.TableKeyType == TableKeyType.SetKey) {
                GetTableSourceFileTableImplGetRowSetFunc(
                    tableDef, out getRowFuncImpl);
            }

            sb.Append(rowConstructorImpl);
            sb.Append(this.newLineStr);
            sb.Append(rowDestructorImpl);
            sb.Append(this.newLineStr);
            sb.Append(constructorImpl);
            sb.Append(this.newLineStr);
            sb.Append(destructorImpl);
            sb.Append(this.newLineStr);
            sb.Append(parseFuncImpl);
            sb.Append(this.newLineStr);
            sb.Append(getRowFuncImpl);

            output = sb.ToString();
        }

        private void GetTableSourceFileTableImplRowConstructor(
            TableDescriptor.TableDef tableDef, out string output)
        {
            StringBuilder sb = new StringBuilder();

            List<string> initListImpl = new List<string>();

            for (int i = 0; i < tableDef.Columns.Count; ++i) {
                TableDescriptor.TableDef.ColumnDef columnDef =
                    tableDef.Columns[i];

                if (columnDef.Type == ColumnType.Int) {
                    initListImpl.Add(string.Format(
                        "    {0}(0)", columnDef.Name));
                }
            }

            string initList = "";
            if (initListImpl.Count > 0) {
                initList = string.Format(" :{0}", this.newLineStr) +
                    string.Join(string.Format(",{0}", this.newLineStr),
                        initListImpl);
            }

            string start = string.Format(
                "{0}::Row::Row(){1}{2}" +
                "{{{2}",
                tableDef.Name, initList, this.newLineStr);
            string end = string.Format(
                "}}{0}", this.newLineStr);

            sb.Append(start);
            sb.Append(end);

            output = sb.ToString();
        }

        private void GetTableSourceFileTableImplRowDestructor(
            TableDescriptor.TableDef tableDef, out string output)
        {
            output = string.Format(
                "{0}::Row::~Row(){1}" +
                "{{{1}" +
                "}}{1}",
                tableDef.Name, this.newLineStr);
        }

        private void GetTableSourceFileTableImplConstructor(
            TableDescriptor.TableDef tableDef, out string output)
        {
            output = string.Format(
                "{0}::{0}(){1}" +
                "{{{1}" +
                "}}{1}",
                tableDef.Name, this.newLineStr);
        }

        private void GetTableSourceFileTableImplDestructor(
            TableDescriptor.TableDef tableDef, out string output)
        {
            output = string.Format(
                "{0}::~{0}(){1}" +
                "{{{1}" +
                "}}{1}",
                tableDef.Name, this.newLineStr);
        }

        private void GetTableSourceFileTableImplParseFunc(
            TableDescriptor.TableDef tableDef, out string output)
        {
            StringBuilder sb = new StringBuilder();

            string start = string.Format(
                "bool {0}::parse(const std::string &text, " +
                "std::string *error_info){1}" +
                "{{{1}" +
                "    brickred::table::LineReader r(text);{1}" +
                "    const brickred::table::LineReader::LineBuffer " +
                "*line_buffer = nullptr;{1}" +
                "    size_t column_count_req = {2};{1}" +
                "{1}",
                tableDef.Name, this.newLineStr, tableDef.Columns.Count);
            string end = string.Format(
                "    *error_info = \"\";{0}" +
                "    return true;{0}" +
                "}}{0}",
                this.newLineStr);

            string readCommentLine;
            string readNameLine;
            string readDataLine = "";

            GetTableSourceFileTableImplParseFuncReadCommentLine(
                tableDef, out readCommentLine);
            GetTableSourceFileTableImplParseFuncReadNameLine(
                tableDef, out readNameLine);

            if (tableDef.TableKeyType == TableKeyType.SingleKey) {
                GetTableSourceFileTableImplParseFuncSingleKeyReadDataLine(
                    tableDef, out readDataLine);
            } else if (tableDef.TableKeyType == TableKeyType.SetKey) {
                GetTableSourceFileTableImplParseFuncSetKeyReadDataLine(
                    tableDef, out readDataLine);
            }

            sb.Append(start);
            sb.Append(readCommentLine);
            sb.Append(readNameLine);
            sb.Append(readDataLine);
            sb.Append(end);

            output = sb.ToString();
        }

        private void GetTableSourceFileTableImplParseFuncReadCommentLine(
            TableDescriptor.TableDef tableDef, out string output)
        {
            string indent = "    ";

            output = string.Format(
                "{0}// read comment line{1}" +
                "{0}line_buffer = r.nextLine();{1}" +
                "{0}if (line_buffer == nullptr) {{{1}" +
                "{0}    *error_info = \"comment line is required\";{1}" +
                "{0}    return false;{1}" +
                "{0}}}{1}" +
                "{0}if (line_buffer->size() != column_count_req) {{{1}" +
                "{0}    *error_info = brickred::table::util::error({1}" +
                "{0}        \"comment line column count %zd is invalid," +
                " should be %zd\",{1}" +
                "{0}        line_buffer->size(), column_count_req);{1}" +
                "{0}    return false;{1}" +
                "{0}}}{1}" +
                "{1}",
                indent, this.newLineStr);
        }

        private void GetTableSourceFileTableImplParseFuncReadNameLine(
            TableDescriptor.TableDef tableDef, out string output)
        {
            StringBuilder sb = new StringBuilder();
            string indent = "    ";

            string start = string.Format(
                "{0}// read name line{1}" +
                "{0}line_buffer = r.nextLine();{1}" +
                "{0}if (line_buffer == nullptr) {{{1}" +
                "{0}    *error_info = \"name line is required\";{1}" +
                "{0}    return false;{1}" +
                "{0}}}{1}" +
                "{0}if (line_buffer->size() != column_count_req) {{{1}" +
                "{0}    *error_info = brickred::table::util::error({1}" +
                "{0}        \"name line column count %zd is invalid," +
                " should be %zd\",{1}" +
                "{0}        line_buffer->size(), column_count_req);{1}" +
                "{0}    return false;{1}" +
                "{0}}}{1}" +
                "{0}{{{1}" +
                "{0}    size_t col_number = 0;{1}" +
                "{1}",
                indent, this.newLineStr);
            string end = string.Format(
                "{0}}}{1}" +
                "{1}",
                indent, this.newLineStr);

            sb.Append(start);

            indent += "    ";
            for (int i = 0; i < tableDef.Columns.Count; ++i) {
                TableDescriptor.TableDef.ColumnDef columnDef =
                    tableDef.Columns[i];

                sb.AppendFormat(
                    "{0}if ((*line_buffer)[col_number++] != \"{1}\") {{{2}" +
                    "{0}    *error_info = brickred::table::util::error({2}" +
                    "{0}        \"column %zd should be named as `{1}`\"," +
                    " col_number);{2}" +
                    "{0}    return false;{2}" +
                    "{0}}}{2}",
                    indent, columnDef.Name, this.newLineStr);
            }

            sb.Append(end);

            output = sb.ToString();
        }

        private void GetTableSourceFileTableImplParseFuncSingleKeyReadDataLine(
            TableDescriptor.TableDef tableDef, out string output)
        {
            string indent = "    ";

            string parseColumns;
            GetTableSourceFileTableImplParseFuncParseColumns(
                tableDef, out parseColumns);

            string keyFormat = "";
            string keyValue = "";
            if (tableDef.TableKey.Type == ColumnType.Int) {
                keyFormat = "%d";
                keyValue = string.Format(
                    "row.{0}", tableDef.TableKey.Name);
            } else if (tableDef.TableKey.Type == ColumnType.String) {
                keyFormat = "%s";
                keyValue = string.Format(
                    "row.{0}.c_str()", tableDef.TableKey.Name);
            }

            output = string.Format(
                "{0}// read data lines{1}" +
                "{0}size_t line_number = 3;{1}" +
                "{0}rows_.clear();{1}" +
                "{0}for (;;) {{{1}" +
                "{0}    line_buffer = r.nextLine();{1}" +
                "{0}    if (line_buffer == nullptr) {{{1}" +
                "{0}        break;{1}" +
                "{0}    }}{1}" +
                "{0}    if (line_buffer->size() != column_count_req) {{{1}" +
                "{0}        *error_info = brickred::table::util::error({1}" +
                "{0}            \"line %zd column count %zd is invalid, " +
                "should be %zd\",{1}" +
                "{0}            line_number, line_buffer->size(), " +
                "column_count_req);{1}" +
                "{0}        return false;{1}" +
                "{0}    }}{1}" +
                "{0}    if ((*line_buffer)[{2}].empty()) {{{1}" +
                "{0}        *error_info = brickred::table::util::error({1}" +
                "{0}            \"line %zd key `{3}` is empty\", " +
                "line_number);{1}" +
                "{0}        return false;{1}" +
                "{0}    }}{1}" +
                "{1}" +
                "{0}    Row row;{1}" +
                "{0}    size_t col_number = 0;{1}" +
                "{1}" +
                "{4}" +
                "{1}" +
                "{0}    if (getRow(row.{3}) != nullptr) {{{1}" +
                "{0}        *error_info = brickred::table::util::error({1}" +
                "{0}            \"line %zd key `{3}` value {5} is duplicated\", " +
                "line_number, {6});{1}" +
                "{0}        return false;{1}" +
                "{0}    }}{1}" +
                "{1}" +
                "{0}    rows_.insert(std::make_pair(row.{3}, row));{1}" +
                "{1}" +
                "{0}    line_number += 1;{1}" +
                "{0}}}{1}" +
                "{1}",
                indent, this.newLineStr,
                tableDef.TableKeyColumnIndex, tableDef.TableKey.Name,
                parseColumns, keyFormat, keyValue);
        }

        private void GetTableSourceFileTableImplParseFuncSetKeyReadDataLine(
            TableDescriptor.TableDef tableDef, out string output)
        {
            string indent = "    ";

            string parseColumns;
            GetTableSourceFileTableImplParseFuncParseColumns(
                tableDef, out parseColumns);

            string keyDefine = "";
            string keyFormat = "";
            string keyValue = "";
            if (tableDef.TableKey.Type == ColumnType.Int) {
                keyDefine = "int32_t key = ::atoi(key_str->c_str())";
                keyFormat = "%d";
                keyValue = string.Format(
                    "row.{0}", tableDef.TableKey.Name);
            } else if (tableDef.TableKey.Type == ColumnType.String) {
                keyDefine = "std::string key = *key_str";
                keyFormat = "%s";
                keyValue = string.Format(
                    "row.{0}.c_str()", tableDef.TableKey.Name);
            }

            output = string.Format(
                "{0}// read data lines{1}" +
                "{0}size_t line_number = 3;{1}" +
                "{0}std::string last_key;{1}" +
                "{0}row_sets_.clear();{1}" +
                "{0}for (;;) {{{1}" +
                "{0}    line_buffer = r.nextLine();{1}" +
                "{0}    if (line_buffer == nullptr) {{{1}" +
                "{0}        break;{1}" +
                "{0}    }}{1}" +
                "{0}    if (line_buffer->size() != column_count_req) {{{1}" +
                "{0}        *error_info = brickred::table::util::error({1}" +
                "{0}            \"line %zd column count %zd is invalid, " +
                "should be %zd\",{1}" +
                "{0}            line_number, line_buffer->size(), " +
                "column_count_req);{1}" +
                "{0}        return false;{1}" +
                "{0}    }}{1}" +
                "{1}" +
                "{0}    const std::string *key_str = &(*line_buffer)[{2}];{1}" +
                "{0}    if (key_str->empty()) {{{1}" +
                "{0}        if (last_key.empty() == false) {{{1}" +
                "{0}            key_str = &last_key;{1}" +
                "{0}        }} else {{{1}" +
                "{0}            *error_info = brickred::table::util::error({1}" +
                "{0}                \"line %zd key `{3}` is empty\", " +
                "line_number);{1}" +
                "{0}            return false;{1}" +
                "{0}        }}{1}" +
                "{0}    }}{1}" +
                "{1}" +
                "{0}    Row row;{1}" +
                "{0}    size_t col_number = 0;{1}" +
                "{1}" +
                "{4}" +
                "{1}" +
                "{0}    {7};{1}" +
                "{0}    if (*key_str != last_key) {{{1}" +
                "{0}        if (getRowSet(key) != nullptr) {{{1}" +
                "{0}            *error_info = brickred::table::util::error({1}" +
                "{0}                \"line %zd key `{3}` value {5} is duplicated\",{1} " +
                "{0}                line_number, {6});{1}" +
                "{0}            return false;{1}" +
                "{0}        }}{1}" +
                "{0}        RowSet row_set;{1}" +
                "{0}        row_set.push_back(row);{1}" +
                "{0}        row_sets_.insert(std::make_pair(key, row_set));{1}" +
                "{0}        last_key = *key_str;{1}" +
                "{0}    }} else {{{1}" +
                "{0}        row.{3} = key;{1}" +
                "{0}        row_sets_[key].push_back(row);{1}" +
                "{0}    }}{1}" +
                "{1}" +
                "{0}    line_number += 1;{1}" +
                "{0}}}{1}" +
                "{1}",
                indent, this.newLineStr,
                tableDef.TableKeyColumnIndex, tableDef.TableKey.Name,
                parseColumns, keyFormat, keyValue, keyDefine);
        }

        private void GetTableSourceFileTableImplParseFuncParseColumns(
            TableDescriptor.TableDef tableDef, out string output)
        {
            StringBuilder sb = new StringBuilder();
            string indent = "        ";

            for (int i = 0; i < tableDef.Columns.Count; ++i) {
                TableDescriptor.TableDef.ColumnDef columnDef =
                    tableDef.Columns[i];

                ColumnType checkType;
                if (columnDef.Type == ColumnType.List) {
                    checkType = columnDef.ListType;
                } else {
                    checkType = columnDef.Type;
                }
                bool isList = (columnDef.Type == ColumnType.List);

                if (checkType == ColumnType.Int) {
                    if (isList) {
                        sb.AppendFormat(
                            "{0}brickred::table::util::readColumnIntList(" +
                            "(*line_buffer)[col_number++], &row.{1});{2}",
                            indent, columnDef.Name, this.newLineStr);
                    } else {
                        sb.AppendFormat(
                            "{0}row.{1} = ::atoi(" +
                            "(*line_buffer)[col_number++].c_str());{2}",
                            indent, columnDef.Name, this.newLineStr);
                    }
                } else if (checkType == ColumnType.String) {
                    if (isList) {
                        sb.AppendFormat(
                            "{0}brickred::table::util::readColumnStringList(" +
                            "(*line_buffer)[col_number++], &row.{1});{2}",
                            indent, columnDef.Name, this.newLineStr);
                    } else {
                        sb.AppendFormat(
                            "{0}row.{1} = (*line_buffer)[col_number++];{2}",
                            indent, columnDef.Name, this.newLineStr);
                    }
                } else if (checkType == ColumnType.Struct) {
                    if (isList) {
                        sb.AppendFormat(
                            "{0}if (brickred::table::util::readColumnStructList({1}" +
                            "{0}        (*line_buffer)[col_number++], &row.{2}) " +
                            "== false) {{{1}" +
                            "{0}    *error_info = brickred::table::util::error({1}" +
                            "{0}        \"line %zd column `{2}` value is invalid\", " +
                            "line_number);{1}" +
                            "{0}    return false;{1}" +
                            "{0}}}{1}",
                            indent, this.newLineStr, columnDef.Name);
                    } else {
                        sb.AppendFormat(
                            "{0}if (row.{1}.parse(" +
                            "(*line_buffer)[col_number++]) == false) {{{2}" +
                            "{0}    *error_info = brickred::table::util::error({2}" +
                            "{0}        \"line %zd column `{1}` value is invalid\", " +
                            "line_number);{2}" +
                            "{0}    return false;{2}" +
                            "{0}}}{2}",
                            indent, columnDef.Name, this.newLineStr);
                    }
                }
            }

            output = sb.ToString();
        }

        private void GetTableSourceFileTableImplGetRowFunc(
            TableDescriptor.TableDef tableDef, out string output)
        {
            string cppType = GetCppType(tableDef.TableKey);
            if (tableDef.TableKey.Type == ColumnType.String) {
                cppType = string.Format("const {0} &", cppType);
            } else {
                cppType = string.Format("{0} ", cppType);
            }

            output = string.Format(
                "const {0}::Row *{0}::getRow({1}key) const{2}" +
                "{{{2}" +
                "    Rows::const_iterator iter = rows_.find(key);{2}" +
                "    if (iter == rows_.end()) {{{2}" +
                "        return nullptr;{2}" +
                "    }}{2}" +
                "{2}" +
                "    return &iter->second;{2}" +
                "}}{2}",
                tableDef.Name, cppType, this.newLineStr);
        }

        private void GetTableSourceFileTableImplGetRowSetFunc(
            TableDescriptor.TableDef tableDef, out string output)
        {
            string cppType = GetCppType(tableDef.TableKey);
            if (tableDef.TableKey.Type == ColumnType.String) {
                cppType = string.Format("const {0} &", cppType);
            } else {
                cppType = string.Format("{0} ", cppType);
            }

            output = string.Format(
                "const {0}::RowSet *{0}::getRowSet({1}key) const{2}" +
                "{{{2}" +
                "    RowSets::const_iterator iter = row_sets_.find(key);{2}" +
                "    if (iter == row_sets_.end()) {{{2}" +
                "        return nullptr;{2}" +
                "    }}{2}" +
                "{2}" +
                "    return &iter->second;{2}" +
                "}}{2}",
                tableDef.Name, cppType, this.newLineStr);
        }
    }
}
