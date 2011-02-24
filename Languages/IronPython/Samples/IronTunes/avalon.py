#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

import clr

clr.AddReferenceByPartialName("PresentationCore")
clr.AddReferenceByPartialName("PresentationFramework")
clr.AddReferenceByPartialName("WindowsBase")

from System import IntPtr
from System.Windows import Application
from System.Windows import Window
from System.Windows.Controls import Button
from System.Windows.Controls import TextBox
from System.Windows.Controls import ListView

def LoadXaml(filename):
    from System.IO import *
    from System.Windows.Markup import XamlReader
    from System.Windows.Markup import ParserContext
    f = FileStream(filename, FileMode.Open, FileAccess.Read)
    try:
        parserContext = ParserContext()
        parserContext.XmlnsDictionary.Add('c','IronTunesApp')
        element = XamlReader.Load(f)
    finally:
        f.Close()
    return element
    
def SetScript(e,s):
    from Pythalon import PythonScript
    e.SetValue(PythonScript.ScriptProperty, s)
    
def SaveXaml(filename, element):
    from System.Windows.Serialization import XamlReader
    s = XamlReader.SaveAsXml(element)
    try:
        f = open(filename, "w")
        f.write(s)
    finally:
        f.close()


def Walk(tree):
    yield tree
    if hasattr(tree, 'Children'):
        for child in tree.Children:
            for x in Walk(child):
                yield x
    elif hasattr(tree, 'Child'):
        for x in Walk(tree.Child):
            yield x
    elif hasattr(tree, 'Content'):
        for x in Walk(tree.Content):
            yield x

def LoadNames(tree, namespace):
    for node in Walk(tree):
        if hasattr(node, 'Name'):
            namespace[node.Name] = node

