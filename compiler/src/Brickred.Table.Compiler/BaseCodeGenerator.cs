using System;

namespace Brickred.Table.Compiler
{
    public abstract class BaseCodeGenerator : IDisposable
    {
        public enum NewLineType
        {
            None = 0,
            Unix = 1,
            Dos = 2,
        }

        public BaseCodeGenerator()
        {
        }

        ~BaseCodeGenerator()
        {
            Dispose();
        }

        public abstract void Dispose();
        public abstract bool Generate(
            TableDescriptor descriptor, string reader,
            string outputDir, NewLineType newLineType);
    }
}
