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

class FormV6(Form):
    def __init__(self):
        self.Text = '(' + __file__ + ')'
        
        # setup TableLayoutPanel and FlowLayoutPanel
        self._tableLayoutPanel1 = TableLayoutPanel(ColumnCount = 1,RowCount = 2, Dock = DockStyle.Fill)
        self._flowLayoutPanel1 = FlowLayoutPanel(Dock = DockStyle.Fill)

        self._webBrowser = WebBrowser(Dock = DockStyle.Fill)
        self._label1 = Label(Text ='Enter Message')
        self._txtMessage = TextBox(TabIndex = 0,Size = Size(200,20))
        self._button1 = Button(Text ='Message')

        # Setup ToolStrip and ToolStripLabel
        self._StatusStrip1 = StatusStrip()
        self._ToolStripStatusLabel1 = ToolStripStatusLabel()
        self._StatusStrip1.Items.Add(self._ToolStripStatusLabel1)
        self.Controls.Add(self._StatusStrip1)
        self.AcceptButton = self._button1   # when the enter key is pressed self._button1 will be clicked
        self._button1.Click += self.OnMsgButtonClick


        # Set TableLayoutPanel column and row styles and add FlowLayoutPanel and Web Browser

        self._tableLayoutPanel1.ColumnStyles.Add(ColumnStyle(SizeType.Percent, 100.0))
        self._tableLayoutPanel1.RowStyles.Add(RowStyle(SizeType.Absolute, 80.0))
        self._tableLayoutPanel1.RowStyles.Add(RowStyle(SizeType.Percent, 100.0))
        self._tableLayoutPanel1.Controls.Add(self._flowLayoutPanel1, 0, 0)
        self._tableLayoutPanel1.Controls.Add(self._webBrowser, 0, 1)

        # add controls that will be contained in FlowLayoutPanel

        self._flowLayoutPanel1.Controls.Add(self._label1)
        self._flowLayoutPanel1.Controls.Add(self._txtMessage)
        self._flowLayoutPanel1.Controls.Add(self._button1)

        self.Controls.Add(self._tableLayoutPanel1)
    
    def OnMsgButtonClick(self, *args):
        self._webBrowser.Navigate(self._txtMessage.Text)
        self._ToolStripStatusLabel1.Text = self._txtMessage.Text
        self._txtMessage.Text = ""


Application.Run(FormV6())
