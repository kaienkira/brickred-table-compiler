TARGET = build/Brickred.Table.dll
SRCS = \
src/Brickred.Table/BaseStruct.cs \
src/Brickred.Table/ColumnSpliter.cs \
src/Brickred.Table/LineReader.cs \
src/Brickred.Table/Util.cs \

.PHONY: build clean

build:
	@mcs -t:library -out:$(TARGET) $(SRCS)

clean:
	@rm -f $(TARGET)
