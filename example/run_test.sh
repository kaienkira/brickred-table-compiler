#!/bin/bash

set -o pipefail

script_name=`basename "$0"`
script_abs_name=`readlink -f "$0"`
script_path=`dirname "$script_abs_name"`

cd "$script_path"/../compiler && make build
if [ $? -ne 0 ]; then exit 1; fi

# create test dir
test_dir="$script_path"/test
mkdir -p "$test_dir"
if [ $? -ne 0 ]; then exit 1; fi
cd "$test_dir"
if [ $? -ne 0 ]; then exit 1; fi

# copy files
cp "$script_path"/../compiler/bin/brickred-table-compiler.exe .
if [ $? -ne 0 ]; then exit 1; fi
cp "$script_path"/table.xml .
if [ $? -ne 0 ]; then exit 1; fi

# cpp test
mono brickred-table-compiler.exe -f table.xml -l cpp -r server
if [ $? -ne 0 ]; then exit 1; fi
mono brickred-table-compiler.exe -f table.xml -l csharp -r client
if [ $? -ne 0 ]; then exit 1; fi
