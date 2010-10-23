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

import System
import clr
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Drawing")
from System.Windows.Forms import *
from System.Drawing import *

f = Form(Text = "My Web Browser")
f.AutoScroll = True
f.AutoSize = True
f.MinimumSize = Size(40,40)
f.Size = Size(500,500) 

browser = WebBrowser()
browser.Dock = DockStyle.Fill
browser.MinimumSize = Size(40,40)
browser.Url = System.Uri("http://www.msn.com")

f.Controls.Add(browser)
f.ShowDialog()