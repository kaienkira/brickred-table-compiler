.PHONY: build

TARGET = bin/brickred-table-cutter.exe
SRC = \
src/Cutter.cs \
src/Mono.Options/Options.cs \
src/Brickred.Table.Compiler/TableDescriptor.cs \
src/Brickred.Table.Compiler/TableParser.cs \

build:
	@mcs -r:System.Xml.Linq $(SRC) -out:$(TARGET)

clean:
	@rm -f $(TARGET)
