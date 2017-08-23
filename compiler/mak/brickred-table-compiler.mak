.PHONY: build

TARGET = bin/brickred-table-compiler.exe
SRC = \
src/Compiler.cs \
src/Mono.Options/Options.cs \
src/Brickred.Table.Compiler/BaseCodeGenerator.cs \
src/Brickred.Table.Compiler/CppCodeGenerator.cs \
src/Brickred.Table.Compiler/CSharpCodeGenerator.cs \
src/Brickred.Table.Compiler/TableDescriptor.cs \
src/Brickred.Table.Compiler/TableParser.cs \

build:
	@mcs -r:System.Xml.Linq $(SRC) -out:$(TARGET)

clean:
	@rm -f $(TARGET)
