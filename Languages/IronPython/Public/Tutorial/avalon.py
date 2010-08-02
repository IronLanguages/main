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
clr.AddReferenceByPartialName("IronPython")
clr.AddReferenceByPartialName("Microsoft.Scripting")

from math import *
from System import *
from System.Windows import *
from System.Windows.Media import *
from System.Windows.Media.Animation import *
from System.Windows.Controls import *
from System.Windows.Shapes import *
from System.Threading import *
from System.Windows.Threading import *

import IronPython

are = AutoResetEvent(False)

def CallBack(f, p = DispatcherPriority.Normal):
    Application.Current.Dispatcher.BeginInvoke(p, IronPython.Runtime.CallTarget0(f))


def CallBack1(f, p0, p = DispatcherPriority.Normal):
    Application.Current.Dispatcher.BeginInvoke(p, IronPython.Runtime.CallTarget1(f), p0)

def on_startup(*args):
    global dispatcher
    dispatcher = Dispatcher.FromThread(t)
    are.Set()

def start():
    try:
        global app
        app = Application()
        app.Startup += on_startup
        app.Run()
    finally:
        clr.SetCommandDispatcher(None)

t = Thread(ThreadStart(start))
t.IsBackground = True
t.ApartmentState = ApartmentState.STA
t.Start()
are.WaitOne()

def DispatchConsoleCommand(consoleCommand):
    if consoleCommand:
        dispatcher.Invoke(DispatcherPriority.Normal, consoleCommand)

clr.SetCommandDispatcher(DispatchConsoleCommand)

def LoadXaml(filename):
    from System.IO import *
    from System.Windows.Markup import XamlReader
    f = FileStream(filename, FileMode.Open, FileAccess.Read)
    try:
        element = XamlReader.Load(f)
    finally:
        f.Close()
    return element
    
def SetScript(e,s):
    from Pythalon import PythonScript
    e.SetValue(PythonScript.ScriptProperty, s)
    
def SaveXaml(filename, element):
    from System.Windows.Markup import XamlWriter
    s = XamlWriter.Save(element)
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