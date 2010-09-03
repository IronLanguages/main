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
clr.AddReferenceByPartialName("System.Windows.Forms")
clr.AddReferenceByPartialName("System.Drawing")
clr.AddReferenceByPartialName("IronPython")

from System.Drawing import Size
from System.Windows.Forms import Form, Application
from System.Threading import Thread
from System.Threading import ThreadStart
from System.Threading import AutoResetEvent
import IronPython


are = AutoResetEvent(False)

def thread_proc():
    try:
        global dispatcher
        global are
        # Create a dummy Control so that we can use it to dispatch commands to the WinForms thread
        dispatcher = Form(Size = Size(0,0))
        dispatcher.Show()
        dispatcher.Hide()
        are.Set()
        Application.Run()
    finally:
        IronPython.Hosting.PythonEngine.ConsoleCommandDispatcher = None

def DispatchConsoleCommand(consoleCommand):
    if consoleCommand:
        dispatcher.Invoke(consoleCommand)
    else:
        Application.Exit()

t = Thread(ThreadStart(thread_proc))
t.Start()
are.WaitOne()
IronPython.Hosting.PythonEngine.ConsoleCommandDispatcher = DispatchConsoleCommand
