#!/usr/bin/env bash

if [ '$ROWAN_BIN' = '' ]; then
	export TEMP_IR_PATH=$MERLIN_ROOT/Bin/mono_debug
else
	export TEMP_IR_PATH=$ROWAN_BIN
fi

if [ '$IR_OPTIONS' = '' ]; then
	export IR_OPTIONS=-X:Interpret
fi

export HOME=~/
export RUBY_EXE=$TEMP_IR_PATH/ir.exe

mono $TEMP_IR_PATH/ir.exe $IR_OPTIONS $*
#
# There should be no operations after this point so that the exitcode or ir.exe will be avilable as ERRORLEVEL
#
