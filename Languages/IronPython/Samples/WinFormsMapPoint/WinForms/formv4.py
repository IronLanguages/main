##############################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Apache License,
# Version 2.0. A copy of the license can be found in the License.html file at
# the root of this distribution. If you cannot locate the  Apache License,
# Version 2.0, please send an email to ironpy@microsoft.com. By using this
# source code in any fashion, you are agreeing to be bound by the terms of
# the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
##############################################################################

import sys
import clr
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Drawing")

import System.ComponentModel.Container
from System.Windows.Forms import *
from System.Drawing import Icon


class FormV4(Form):
    def __init__(self):
        self.Text = "Hello World (" + __file__ + ")"

        # Create Label
        self.Controls.Add(Label(Text="Enter Message:"))

        # Create TextBox
        self._txtMessage = TextBox(Left=100)
        self.Controls.Add(self._txtMessage)

        # Create Button
        msgButton = Button(Text="Message", Left=20, Top=25)
        msgButton.Click += self.OnMsgButtonClick
        self.Controls.Add(msgButton)

        # Create Component Container
        self._components = System.ComponentModel.Container()

        #
        # Add component - ContextMenu
        #
        self._contextMenuStrip1 = ContextMenuStrip(self._components)
        self._exitToolStripMenuItem = ToolStripMenuItem(Text="Exit")
        self._contextMenuStrip1.Items.Add(self._exitToolStripMenuItem)
        self._exitToolStripMenuItem.Click += self.OnExitClick

        #
        # add Component - NotifyIcon
        #
        self._notifyIcon1 = NotifyIcon(self._components,
                                       Visible=True, Text="Test")
        self._notifyIcon1.Icon = Icon(System.IO.Path.Combine(sys.path[0],
                                                             "app.ico"))
        self._notifyIcon1.ContextMenuStrip = self._contextMenuStrip1
        self.Closed += self.OnNotifyIconExit

    def OnMsgButtonClick(self, *args):
        MessageBox.Show(self._txtMessage.Text, "Message")

    def OnExitClick(self, *args):
        self.Close()

    def OnNotifyIconExit(self, *args):
        self._notifyIcon1.Dispose()

Application.Run(FormV4())
