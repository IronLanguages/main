#!/bin/bash

OS=`uname`

if [[ $OS == "Darwin" ]] ; then
    PROCESSOR_ARCHITECTURE=x86_64
else
    PROCESSOR_ARCHITECTURE=`lscpu | grep Architecture | sed -e 's/.*:\s*//g'`
fi

# ---------------------------------------------------------------------------
if [[ "$PROCESSOR_ARCHITECTURE" == "x86" ]] ; then
    FLAVOR=x86
elif [[ "$PROCESSOR_ARCHITECTURE" == "x86_64" ]] ; then
    FLAVOR=x64
fi

echo "${FLAVOR} (IronPython)"

# ---------------------------------------------------------------------------
# Sanity checks

if [[ -z "$DLR_BIN" ]] ; then
    echo "You need to set DLR_BIN before running tests"
    exit -1
fi

TEST_DIR=`pwd`/@test
if [[ -e ${TEST_DIR} ]] ; then
	echo "Removing ${TEST_DIR}"
        rm -rf ${TEST_DIR}
fi

# ---------------------------------------------------------------------------
# GLOBALS

EXECUTABLE="mono $DLR_BIN/ipy.exe"
TEST=$*
ORIG_TEST_OPTIONS=${TEST_OPTIONS}


# ---------------------------------------------------------------------------
# Handle SNAP jobs types here

if [[ ! -z "$ISINTERACTIVE" ]] ; then
    TEST=${DLR_ROOT}/Languages/IronPython/Scripts/run_interactive.py ${TEST}
fi

if [[ ! -z "$ISCOMPILED" ]] ; then
    TEST=${DLR_ROOT}/Languages/IronPython/Scripts/run_compiled.py ${TEST}
fi

# ---------------------------------------------------------------------------
# Determine which version of the CPython standard library this particular test
# needs to be run against.

# set TEST_OPTIONS=-X:Python25 %TEST_OPTIONS%

ORIG_IRONPYTHONPATH=${IRONPYTHONPATH}
export IRONPYTHONPATH=${DLR_ROOT}/External.LCA_RESTRICTED/Languages/IronPython/27/Lib
DONE=0

# ---------------------------------------------------------------------------
# Run the test
#while [ "${DONE}" != "1" ] ; do
    echo "${EXECUTABLE} ${TEST_OPTIONS} ${TEST}"
    ${EXECUTABLE} ${TEST_OPTIONS} ${TEST}
    LAST_EL=$?

#    if [[ "${IS_FLAKEY}" != "" ]] ; then
#        IS_FLAKEY=$((IS_FLAKEY-1))
#        if [[ "${IS_FLAKEY}" == "0" ]] ; then
#            IS_FLAKEY=
#        fi
#
#        if [[ "${LAST_EL}" == "0" ]] ; then
#            DONE=1
#        else
#	    echo "This flakey test failed. Will try rerunning again."
#        fi
#    fi
#done

# ---------------------------------------------------------------------------
# Restore the environment
export TEST_OPTIONS=${ORIG_TEST_OPTIONS}
export IRONPYTHONPATH=${ORIG_IRONPYTHONPATH}


# ---------------------------------------------------------------------------
# Exit appropriately for the platform
exit ${LAST_EL}
