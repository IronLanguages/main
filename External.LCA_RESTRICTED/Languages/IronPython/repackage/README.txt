This directory contains exploded version of the packages bundled with
ensurepip.

As long as patches are not accepted upstream, we need to
maintain our own copy.

In case of changes in pip or setuptools:
1. run:
ipy makewheel.py
2. commit package files from:
IronLanguages/External.LCA_RESTRICTED/Languages/IronPython/27/Lib/ensurepip/_bundled
