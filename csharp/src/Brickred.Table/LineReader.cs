using System.Collections.Generic;

namespace Brickred.Table
{
    public sealed class LineReader
    {
        private enum Status
        {
            Normal,
            ReadColumn,
            ReadNewline,
        }

        private string text;
        private int readIndex;
        private List<string> lineBuffer;

        public LineReader(string text)
        {
            this.text = text;
            this.readIndex = 0;
            this.lineBuffer = new List<string>();
        }

        public List<string> NextLine()
        {
            this.lineBuffer.Clear();

            if (this.readIndex >= this.text.Length) {
                return null;
            }

            Status status = Status.Normal;
            int colStart = this.readIndex;

            for (int i = this.readIndex; i < this.text.Length; ++i) {
                char c = this.text[i];

                if (status == Status.Normal) {
                    if (c == '\t') {
                        this.lineBuffer.Add("");
                        colStart = i + 1;
                    } else if (c == '\r') {
                        status = Status.ReadNewline;
                    } else {
                        status = Status.ReadColumn;
                    }
                } else if (status == Status.ReadColumn) {
                    if (c == '\t') {
                        this.lineBuffer.Add(GetColumn(colStart, i));
                        colStart = i + 1;
                        status = Status.Normal;
                    } else if (c == '\r') {
                        status = Status.ReadNewline;
                    }
                } else if (status == Status.ReadNewline) {
                    if (c == '\n') {
                        this.lineBuffer.Add(GetColumn(colStart, i - 1));
                        this.readIndex = i + 1;
                        return this.lineBuffer;
                    } else if (c == '\r') {
                        continue;
                    } else {
                        status = Status.ReadColumn;
                    }
                }
            }

            if (colStart < text.Length) {
                this.lineBuffer.Add(GetColumn(colStart, text.Length));
                this.readIndex = text.Length;
                return this.lineBuffer;
            }

            return null;
        }

        private string GetColumn(int colStart, int colEnd)
        {
            if (colEnd - colStart >= 2 &&
                this.text[colStart] == '"' &&
                this.text[colEnd - 1] == '"') {
                // trim quote mark
                colStart += 1;
                colEnd -= 1;
                // convert double quote mark to single quote mark
                return this.text.Substring(colStart, colEnd - colStart).Replace("\"\"", "\"");
            } else {
                return this.text.Substring(colStart, colEnd - colStart);
            }
        }
    }
}
