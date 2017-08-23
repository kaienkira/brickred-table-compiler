include config.mak

TARGET = build/libbrickredtable
SRCS = \
src/brickred/table/column_spliter.cc \
src/brickred/table/line_reader.cc \
src/brickred/table/util.cc \

LINK_TYPE = static
INCLUDE = -Isrc
BUILD_DIR = build

include mak/main.mak

.PHONY: install

install:
	@mkdir -p $(BR_INSTALL_PREFIX)/include/brickred/table
	@cp src/brickred/table/*.h $(BR_INSTALL_PREFIX)/include/brickred/table
	@mkdir -p $(BR_INSTALL_PREFIX)/lib
	@cp $(FINAL_TARGET) $(BR_INSTALL_PREFIX)/lib
