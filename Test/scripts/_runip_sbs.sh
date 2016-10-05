#!/bin/sh

#
# side by side test invocation
#

if [ -x "/usr/bin/python2.7" ] ; then
    /usr/bin/python2.7 runsbs.py $1
    ${DLR_ROOT}/Languages/IronPython/Internal/ipy.sh runsbs.py $1
else
    echo "Could not find valid python interpreter"
    exit -1
fi

