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


class FormV8(Form):
    def __init__(self):
        self.Text = '(' + __file__ + ')'
        
        # Create TableLayoutPanel and FlowLayoutPanel    
        self._tableLayoutPanel1 = TableLayoutPanel(ColumnCount = 1,Dock = DockStyle.Fill,RowCount = 3)
        self._flowLayoutPanel1 = FlowLayoutPanel(Dock = DockStyle.Fill)
        self._flowLayoutPanel2 = FlowLayoutPanel(Dock = DockStyle.Fill)

        # controls for FlowLayout Start
        self._label1 = Label(Text ='Enter start location:',AutoSize=True)
        self._txtFromLocation = TextBox()
        
        # controls for FlowLayout End
        self._label2 = Label(Text ='Enter end location:',AutoSize=True)
        self._txtToLocation = TextBox()
        self._button1 = Button(Text ='Get map',AutoSize=True)

        # this will hold our route map
        self._pictureBox1 = PictureBox(Dock = DockStyle.Fill)

        # Setup TableLayoutPanel rows and columns and add controls
        self._tableLayoutPanel1.ColumnStyles.Add(ColumnStyle(SizeType.Percent, 100.0))
        self._tableLayoutPanel1.RowStyles.Add(RowStyle(SizeType.Absolute, 40.0))
        self._tableLayoutPanel1.RowStyles.Add(RowStyle(SizeType.Absolute, 60.0))
        self._tableLayoutPanel1.RowStyles.Add(RowStyle(SizeType.Percent, 100.0))
        self._tableLayoutPanel1.Controls.Add(self._flowLayoutPanel1, 0, 0)
        self._tableLayoutPanel1.Controls.Add(self._flowLayoutPanel2, 0, 1)
        self._tableLayoutPanel1.Controls.Add(self._pictureBox1, 0, 2)

        # Add controls to FlowLayoutPanel Start
        self._flowLayoutPanel1.Controls.Add(self._label1)
        self._flowLayoutPanel1.Controls.Add(self._txtFromLocation)

        # Add controls to FlowLayoutPanel End
        self._flowLayoutPanel2.Controls.Add(self._label2)
        self._flowLayoutPanel2.Controls.Add(self._txtToLocation)
        self._flowLayoutPanel2.Controls.Add(self._button1)

        # Setup ToolStrip and ToolStripProgresssBar - Maximum = 100 and step is 25
        self._StatusStrip1 = StatusStrip()
        self._ToolStripProgressBar1 = ToolStripProgressBar(Maximum = 100,Step =25)
        self._StatusStrip1.Items.Add(self._ToolStripProgressBar1)
        self.Controls.Add(self._StatusStrip1)

        self.AcceptButton = self._button1
        self._button1.Click += self.OnMsgButtonClick
        self.Controls.Add(self._tableLayoutPanel1)

    def OnMsgButtonClick(self, *args):
        self._ToolStripProgressBar1.Value = 0   # set progress to zero        

        try:
            self._ToolStripProgressBar1.PerformStep()      # increment progess to 25%
            mapHelper = MapPointWebServiceHelper.GetInstance("5200", "ned68Fe")

            self._ToolStripProgressBar1.PerformStep()      # increment progress to 50%
            locFrom = mapHelper.FindLocation(self._txtFromLocation.Text)

            self._ToolStripProgressBar1.PerformStep()      # increment progress to 75%
            locTo = mapHelper.FindLocation(self._txtToLocation.Text)

            self._ToolStripProgressBar1.PerformStep()      # increment progress to 100%    
            myRoute = mapHelper.GetRoute(locFrom, locTo)

            self._pictureBox1.Image = mapHelper.GetRouteMap(myRoute, self._pictureBox1.Width, self._pictureBox1.Height)
        except Exception, e:
            MessageBox.Show(e.Message,"MapPoint Exception")

        self._ToolStripProgressBar1.Value = 0        # set progress to back to zero

Application.Run(FormV8())
