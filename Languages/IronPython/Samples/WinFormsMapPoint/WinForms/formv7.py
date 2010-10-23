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

clr.AddReferenceToFile("MapPointWebServiceProject.dll")
from MapPointWebServiceProject import *
from MapPointWebServiceProject.net.mappoint.staging import *


class FormV7(Form):
    def __init__(self):
        self.Text = '(' + __file__ + ')'
        
        # Create TableLayoutPanel and FlowLayoutPanel    
        self._tableLayoutPanel1 = TableLayoutPanel(ColumnCount = 1,Dock = DockStyle.Fill,RowCount = 2)
        self._flowLayoutPanel1 = FlowLayoutPanel(Dock = DockStyle.Fill)
            
        # Create Controls
        self._pictureBox1 = PictureBox(Dock = DockStyle.Fill)
        self._label1 = Label(Text ='Enter location:',AutoSize=True)
        self._txtLocation = TextBox()
        self._button1 = Button(Text ='Get map',AutoSize=True)

        # Setup TableLayoutPanel rows and columns and add controls
        self._tableLayoutPanel1.ColumnStyles.Add(ColumnStyle(SizeType.Percent, 100.0))
        self._tableLayoutPanel1.RowStyles.Add(RowStyle(SizeType.Absolute, 45.0))
        self._tableLayoutPanel1.RowStyles.Add(RowStyle(SizeType.Percent, 100.0))
        self._tableLayoutPanel1.Controls.Add(self._flowLayoutPanel1, 0, 0)
        self._tableLayoutPanel1.Controls.Add(self._pictureBox1, 0, 1)

        # Add controls to FlowLayoutPanel
        self._flowLayoutPanel1.Controls.Add(self._label1)
        self._flowLayoutPanel1.Controls.Add(self._txtLocation)
        self._flowLayoutPanel1.Controls.Add(self._button1)

        self.AcceptButton = self._button1
        self._button1.Click += self.OnMsgButtonClick
        self.Controls.Add(self._tableLayoutPanel1)

    def OnMsgButtonClick(self, *args):
        try:
            mapHelper = MapPointWebServiceHelper.GetInstance("5200", "ned68Fe")
            loc = mapHelper.FindLocation(self._txtLocation.Text)
            if loc:
                self._pictureBox1.Image = mapHelper.GetMap(loc, self._pictureBox1.Width, self._pictureBox1.Height, 4.0)
            else:
                MessageBox.Show("Address or location is not valid","Invalid Location")
        except Exception, e:
            MessageBox.Show(e.Message,"MapPoint Exception")

Application.Run(FormV7())
