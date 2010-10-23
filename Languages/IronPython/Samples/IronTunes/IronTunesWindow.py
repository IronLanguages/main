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

from avalon import *
import Microsoft
import Microsoft.Win32
import System.Environment
clr.AddReference("System.Windows.Forms")
import System.Windows.Forms
import System.Windows.Interop
import System.Windows.Threading

#define for window message to tell us there are filter graph events
WM_GRAPHNOTIFY = 0x8001

class IronTunesWindow:
    "The class that sets up the iron tunes window"
    
    def __init__(self, dataModel):
        IronTunesWindow.dataModel = dataModel
        self.dataModel = dataModel
    
    #our custon wndproc to respond to filter graph events
    @staticmethod
    def IronTunesWndProc(hwnd, msg, wParam, lParam, handled):
        if (msg == WM_GRAPHNOTIFY):
            IronTunesWindow.dataModel.player.ProcessEvents()
            handled.Value = True
        return IntPtr.Zero
    
    def enliven(self, w):
        self.w = w
        controls = [n for n in Walk(w) if isinstance(n, Button) or isinstance(n, TextBox) or isinstance(n, ListView)]
        for i in controls:
            if (isinstance(i,Button)):
                if (hasattr(self, "on_" + i.Name)):
                    i.Click += getattr(self, "on_" + i.Name)
            elif (isinstance(i,ListView)):
                if (i.Name == "LibraryView"):
                    i.MouseDown += getattr(self, "on_LibraryViewMouseDown")
                    
        #set up a wndproc to respond to events from our direct show filtergraph
        helper = System.Windows.Interop.WindowInteropHelper(w)
        handle = helper.Handle
        self.dataModel.windowHandle = handle
        source = System.Windows.Interop.HwndSource.FromHwnd(handle)
        source.AddHook(self.IronTunesWndProc)
        
        #set up a timer to refresh the position indicator
        dt = System.Windows.Threading.DispatcherTimer()
        dt.Tick += getattr(self,"on_tick")
        dt.Interval = System.TimeSpan.FromMilliseconds(100)
        dt.Start()
    
    def on_Back(self, sender, eventargs):
        self.dataModel.player.PlayingIndex -= 1
        
    def on_Forward(self, sender, eventargs):
        self.dataModel.player.PlayingIndex += 1
        
    def on_PlayPause(self, sender, eventargs):
        self.dataModel.player.PlayPause()
        
    def on_LibraryViewMouseDown(self, sender, eventargs):
        self.dataModel.player.LibraryMouseDown(eventargs)
        
    def on_AddFile(self, sender, eventargs):
        dlg = Microsoft.Win32.OpenFileDialog()
        dlg.Filter = "All Music Files|*.wma;*.mp3|WMA File (*.wma)|*.wma|MP3 File (*.mp3)|*.mp3|All Files (*.*)|*.*"
        dlg.FilterIndex = 0
        dlg.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic)
        dlg.Multiselect = True
        if (dlg.ShowDialog() == True):
            for filename in dlg.FileNames:
                self.dataModel.library.AddFile(filename)
                
    def on_AddDir(self, sender, eventargs):
        dlg = System.Windows.Forms.FolderBrowserDialog()
        dlg.SelectedPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic)
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK):
            self.dataModel.library.AddDir(dlg.SelectedPath)

    def on_tick(self, sender, eventargs):
        self.dataModel.player.Tick()