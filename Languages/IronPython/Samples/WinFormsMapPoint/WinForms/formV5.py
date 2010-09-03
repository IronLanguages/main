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
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Drawing")
from System.Windows.Forms import *
from System.Drawing import *
import System
from System import *
import sys #To get sys.path[0], the location of the running script.

class FormV5(Form):
    def __init__(self):
        self.Text = 'Hello World' + ' (' + __file__ + ')'
        self.txtMessage = TextBox()
        msgButton = Button(Text='Message')
        msgButton.Click += self.OnmsgButtonClick

        # 
        # Create FlowPanelLayout and add controls
        #

        self._flowLayoutPanel1 = FlowLayoutPanel(Dock = DockStyle.Fill)
        self._flowLayoutPanel1.Controls.Add(Label(Text='Enter Message:'))
        self._flowLayoutPanel1.Controls.Add(self.txtMessage)
        self._flowLayoutPanel1.Controls.Add(msgButton)
        self.Controls.Add(self._flowLayoutPanel1)


        self._components = System.ComponentModel.Container()

        # 
        # Add component - ContextMenu
        #
        self._contextMenuStrip1 = ContextMenuStrip(self._components)
        self._exitToolStripMenuItem = ToolStripMenuItem(Text='Exit')
        self._exitToolStripMenuItem.Click += self.OnExitClick
        self._contextMenuStrip1.Items.Add(self._exitToolStripMenuItem)

        #
        # add Component - NotifyIcon
        #
        self._notifyIcon1 = NotifyIcon(self._components,Visible=True,Text='Test')
        self._notifyIcon1.Icon = Icon(System.IO.Path.Combine(sys.path[0], "app.ico"))
        self._notifyIcon1.ContextMenuStrip = self._contextMenuStrip1
        self.Closed += self.OnNotifyIconExit


    def OnmsgButtonClick(self, *args):
        MessageBox.Show(self.txtMessage.Text,"Message")

    def OnExitClick(self, *args):
        self.Close()

    def OnNotifyIconExit(self, *args):
        self._notifyIcon1.Dispose()


Application.Run(FormV5())
