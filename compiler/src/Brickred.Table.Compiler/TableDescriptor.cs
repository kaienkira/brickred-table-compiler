using System.Collections.Generic;

namespace Brickred.Table.Compiler
{
    public sealed class TableDescriptor
    {
        public sealed class ReaderDef
        {
            // reader name
            public string Name = "";
            // define in line number
            public int LineNumber = 0;

            public string Namespace = "";
            public List<string> NamespaceParts = new List<string>();
        }

        public sealed class StructDef
        {
            public enum FieldType
            {
                None = 0,
                Int = 1,
                String = 2,
            }

            public sealed class FieldDef
            {
                // link to parent define
                public StructDef ParentRef = null;
                // field name
                public string Name = "";
                // define in line number
                public int LineNumber = 0;

                public FieldType Type = FieldType.None;
            }

            // link to parent define, null when struct is global
            public TableDef ParentRef = null;
            // struct name
            public string Name = "";
            // define in line number
            public int LineNumber = 0;

            // in file define order
            public List<FieldDef> Fields = new List<FieldDef>();
            // FieldDef.Name -> FieldDef
            public Dictionary<string, FieldDef> FieldNameIndex =
                new Dictionary<string, FieldDef>();
        }

        public sealed class TableDef
        {
            public enum ColumnType
            {
                None = 0,
                Int = 1,
                String = 2,
                Struct = 3,
                List = 4,
            }

            public enum KeyType
            {
                None = 0,
                SingleKey = 1,
                SetKey = 2,
            }

            public sealed class ColumnDef
            {
                // link to parent define
                public TableDef ParentRef = null;
                // column name
                public string Name = "";
                // define in line number
                public int LineNumber = 0;

                public ColumnType Type = ColumnType.None;
                public ColumnType ListType = ColumnType.None;
                public StructDef RefStructDef = null;
                // read by
                public Dictionary<string, ReaderDef> Readers =
                    new Dictionary<string, ReaderDef>();
            }

            // table name
            public string Name = "";
            // define in line number
            public int LineNumber = 0;

            // table key
            public ColumnDef TableKey = null;
            // table key type
            public KeyType TableKeyType = KeyType.None;
            // table key column index
            public int TableKeyColumnIndex = 0;
            // file name
            public string FileName = "";
            // read by
            public Dictionary<string, ReaderDef> Readers =
                new Dictionary<string, ReaderDef>();
            // in file define order
            public List<StructDef> LocalStructs = new List<StructDef>();
            // StructDef.Name -> StructDef
            public Dictionary<string, StructDef> LocalStructNameIndex =
                new Dictionary<string, StructDef>();
            // in file define order
            public List<ColumnDef> Columns = new List<ColumnDef>();
            // ColumnDef.Name -> ColumnDef
            public Dictionary<string, ColumnDef> ColumnNameIndex =
                new Dictionary<string, ColumnDef>();
        }

        public string FilePath = "";

        // reader define
        // ReaderDef.Name -> ReaderDef
        public Dictionary<string, ReaderDef> Readers =
            new Dictionary<string, ReaderDef>();

        // global struct define
        // in file define order
        public List<StructDef> GlobalStructs = new List<StructDef>();
        // StructDef.Name -> StructDef
        public Dictionary<string, StructDef> GlobalStructNameIndex =
            new Dictionary<string, StructDef>();

        // table define
        // in file define order
        public List<TableDef> Tables = new List<TableDef>();
        // TableDef.Name -> TableDef
        public Dictionary<string, TableDef> TableNameIndex =
            new Dictionary<string, TableDef>();
    }
}
