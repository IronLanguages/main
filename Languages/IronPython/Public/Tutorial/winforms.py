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
clr.AddReferenceByPartialName("Microsoft.Scripting")

from System.Drawing import Size
from System.Windows.Forms import Form, Application
from System.Threading import Thread
from System.Threading import ThreadStart
from System.Threading import AutoResetEvent
import IronPython

# We support interactive development of Windows Forms by creating another
# thread on which we execute all of the IronPython console's input.  This
# allows the form to execute on another thread where a message pump can
# independently handle input and allow the form to repaint.  To make this work
# we do two things.  First we create a dummy hidden form on the other thread
# which gives us a handle for marshalling the execution of console input onto
# the thread where we will develop our real form.  Second, we supply an
# alternative function for the IronPython console to run for executing console
# input.  Our alternative function simply invokes the input on the other
# thread.


are = AutoResetEvent(False)

def thread_proc():
    try:
        global dispatcher
        global are
        # Create the dummy control, and show then hide it to get Windows Forms
        # to initialize it.
        dispatcher = Form(Size = Size(0,0))
        dispatcher.Show()
        dispatcher.Hide()
        # Signal that the thread running thread_proc is ready for the main
        # thread to send input to it.
        are.Set()
        # Start the message loop.
        Application.Run()
    finally:
        # In case thread_proc's thread dies, restore the default IronPython
        # console execution behavior.
        clr.SetCommandDispatcher(None)

def DispatchConsoleCommand(consoleCommand):
    if consoleCommand:
        # consoleCommand is a delegate for a dynamic method that embodies the
        # input expression from the user.  Run it on the other thread.
        dispatcher.Invoke(consoleCommand)
    else:
        Application.Exit()

t = Thread(ThreadStart(thread_proc))
t.IsBackground = True
t.Start()
# Don't establish the alternative input execution behavior until the other
# thread is ready.  Note, 'are' starts out unsignalled.
are.WaitOne()
clr.SetCommandDispatcher(DispatchConsoleCommand)
