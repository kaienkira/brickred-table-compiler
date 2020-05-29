include config.mak

TARGET = build/libbrickredtable
SRCS = \
src/brickred/table/column_spliter.cc \
src/brickred/table/line_reader.cc \
src/brickred/table/util.cc \

LINK_TYPE = static
INCLUDE = -Isrc
CPP_FLAG = $(BRICKRED_COMPILE_FLAG)
BUILD_DIR = build

include mak/main.mak

.PHONY: install

install:
	@mkdir -p "$(BRICKRED_INSTALL_PREFIX)"/include/brickred/table
	@cp src/brickred/table/*.h \
		"$(BRICKRED_INSTALL_PREFIX)"/include/brickred/table
	@mkdir -p "$(BRICKRED_INSTALL_PREFIX)"/lib
	@cp "$(FINAL_TARGET)" "$(BRICKRED_INSTALL_PREFIX)"/lib
