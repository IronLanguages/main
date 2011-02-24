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

clr.AddReferenceByPartialName("WindowsBase")
clr.AddReferenceByPartialName("PresentationCore")
clr.AddReferenceByPartialName("PresentationFramework")
clr.AddReferenceByPartialName("IronPython")
clr.AddReferenceByPartialName("System.Windows.Forms")

from System import *
from System.Threading import Thread, ThreadStart, AutoResetEvent, ApartmentState
from System.Windows import Application
from System.Windows.Threading import Dispatcher, DispatcherPriority

import IronPython

from uilogic import IronDiskUsage

def appStart():
	try:
		global are, idu
		app = Application()
		
		idu = IronDiskUsage(app)

		global dispatcher
		dispatcher = Dispatcher.FromThread(Thread.CurrentThread)
		
		are.Set()
		app.Run()
	finally:
		IronPython.Hosting.PythonEngine.ConsoleCommandDispatcher = None

def DispatchConsoleCommand(consoleCommand):
	if consoleCommand:
		dispatcher.Invoke(DispatcherPriority.Normal, consoleCommand)

def startInteractive():
	global are
	are = AutoResetEvent(False)
	
	t = Thread(ThreadStart(appStart))
	t.ApartmentState = ApartmentState.STA
	t.Start()
	
	are.WaitOne()
	IronPython.Hosting.PythonEngine.ConsoleCommandDispatcher = DispatchConsoleCommand

def start():
	global idu
	app = Application()
	idu = IronDiskUsage(app)
	app.Run()
	

# If this module is loaded from another one (mostly in interactive mode), start in another thread
if __name__ == '__main__':
	start()
elif __name__ == 'app':
	startInteractive()

