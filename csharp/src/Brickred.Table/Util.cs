using System.Collections.Generic;

namespace Brickred.Table
{
    public sealed class Util
    {
        public static int Atoi(string str)
        {
            int ret = 0;
            int.TryParse(str, out ret);

            return ret;
        }

        public static void ReadColumnIntList(
            string col, ref List<int> ret)
        {
            if (col.Length == 0) {
                return;
            }

            ColumnSpliter s = new ColumnSpliter(col, '|');
            int v = 0;
            while (s.NextInt(ref v)) {
                ret.Add(v);
            }
        }

        public static void ReadColumnStringList(
            string col, ref List<string> ret)
        {
            if (col.Length == 0) {
                return;
            }

            ColumnSpliter s = new ColumnSpliter(col, '|');
            string v = "";
            while (s.NextString(ref v)) {
                ret.Add(v);
            }
        }

        public static bool ReadColumnStructList<T>(
            string col, ref List<T> ret) where T : BaseStruct, new()
        {
            if (col.Length == 0) {
                return true;
            }

            ColumnSpliter s = new ColumnSpliter(col, '|');
            string str = "";
            while (s.NextString(ref str)) {
                T v = new T();
                if (v.Parse(str) == false) {
                    return false;
                }
                ret.Add(v);
            }

            return true;
        }
    }
}
