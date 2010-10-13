Directory structure:
    docutils       - the docutils package (downloaded from http://pypi.python.org/pypi/docutils/0.5 )
    jinja2         - the jinja2 package (downloaded from http://pypi.python.org/pypi/Jinja2)
    sphinx         - the Sphinx RST converter tool (http://pypi.python.org/pypi/Sphinx)
    PythonDocs     - CPython documentation rst (SVN pulled from http://svn.python.org/projects/python/branches/release26-maint/Doc)
    IronPythonDocs - IronPython documentation rst (same as Python docs + IronPython specific modifications)
    HtmlHelp.exe   - HTML Help Workshop installer
    
    ipymake.bat - batch file for generating HTML help and compiled html help
                  generates output in:
                    Output\         - the directory where all output is generated to
                    Output\Html     - the pure HTML version of help
                    Output\CHtml    - the compiled HTML version of help
                    
    

Generating documentation:
    Pre-reqs: You'll need to install Html Help Workshop before you can generate the CHM help.  This can be installed
    with the HtmlHelp.exe which is available in this directory.
    
    Then just run ipymake.bat and the output will be written to the Output directory.

Updating Python Documentation:
    When updating the standard CPython documentation a new copy of the Doc folder should be pulled from CPython's
    respository (http://svn.python.org/projects/python/branches/release26-maint/Doc).  A 3-way merge should then
    be performed using PythonDocs as the base w/ IronPythonDocs and the newly pulled docs being merged.  The
    result of the merge should be saved in IronPythonDocs and the newly pulled documentation should replace the
    PythonDocs folder.
    

