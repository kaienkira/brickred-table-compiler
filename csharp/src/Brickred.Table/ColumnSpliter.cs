namespace Brickred.Table
{
    public sealed class ColumnSpliter
    {
        private string text;
        private char delimiter;
        private int readIndex;

        public ColumnSpliter(string text, char delimiter)
        {
            this.text = text;
            this.delimiter = delimiter;
            this.readIndex = 0;
        }

        public bool NextInt(ref int val)
        {
            string ret = "";
            if (NextString(ref ret) == false) {
                return false;
            }
            val = Util.Atoi(ret);

            return true;
        }

        public bool NextString(ref string val)
        {
            if (this.readIndex > this.text.Length) {
                return false;
            } else if (this.readIndex == this.text.Length) {
                if (val != null) {
                    val = "";
                }
                this.readIndex += 1;
                return true;
            }

            for (int i = this.readIndex; i < this.text.Length; ++i) {
                char c = this.text[i];

                if (c == this.delimiter) {
                    if (val != null) {
                        val = this.text.Substring(
                            this.readIndex, i - this.readIndex);
                    }
                    this.readIndex = i + 1;
                    return true;
                }
            }

            if (this.readIndex < this.text.Length) {
                if (val != null) {
                    val = this.text.Substring(
                        this.readIndex, this.text.Length - this.readIndex);
                }
                this.readIndex = this.text.Length + 1;
                return true;
            }

            return false;
        }

        public bool NextString()
        {
            string ret = null;
            return NextString(ref ret);
        }
    }
}
