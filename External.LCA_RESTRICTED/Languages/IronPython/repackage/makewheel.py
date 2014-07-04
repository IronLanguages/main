# this script recreates and copy package files to be bundled with ensurepip
#
# the source for packages are pip and setuptools
# the target is stdlib
#
# Usage: ipy makewheel.py
#
# Author: Pawel Jasinski
#
import os
import shutil
import sys
import zipfile


script_dir = os.path.dirname(os.path.abspath(__file__))

def zipdir(path, zipname):
    with zipfile.ZipFile(zipname, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for root, dirs, files in os.walk(path):
            for f in files:
                zipf.write(os.path.join(root, f))

def repackage(subdir, wheel):
    os.chdir(script_dir)
    if os.path.exists(wheel):
        os.unlink(wheel)
    os.chdir(subdir)
    zipdir(".", os.path.join("..", wheel))
    os.chdir("..")
    shutil.copy(wheel, "../27/Lib/ensurepip/_bundled")


repackage("pip", "pip-1.5.6-py2.py3-none-any.whl")
repackage("setuptools", "setuptools-3.6-py2.py3-none-any.whl")

