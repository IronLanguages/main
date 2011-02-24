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

from System import IO
from System import Windows
from System.Windows import Controls
from System.Windows import Media
from System.Windows import Markup
from System.Windows import Documents


# Deserialize an Xaml file
def LoadXaml(filename):
	f = IO.FileStream(filename, IO.FileMode.Open, IO.FileAccess.Read)
	try:
		element = Markup.XamlReader.Load(f)
	finally:
		f.Close()
	return element

### Manage a popup window with minimal border
class InfoPopup(object):
	def __init__(self):
		self.window = Windows.Window()
		self.window.WindowStyle = Windows.WindowStyle.None
		self.window.Width = 300
		self.window.Height = 55
		self.window.ShowInTaskbar = False
		self.window.Topmost = True

		t = Controls.TextBlock()
		t.Padding = Windows.Thickness(5,5,5,5)
		t.TextWrapping = Windows.TextWrapping.NoWrap
		t.Background = Media.Brushes.LightYellow
		self.window.Content = t
		
	def SetPosition(self, x, y):
		self.window.Left = x
		self.window.Top = y
		
	def Show(self):
		self.window.Show()
		
	def Hide(self):
		self.window.Hide()
		
	def SetText(self, text):
		self.window.Content.Text = text
		
	def ClearText(self):
		self.window.Content.Inlines.Clear()
		
	def AddText(self, text):
		self.window.Content.Inlines.Add( Documents.Run(text) )
		
	def AddBoldText(self, text):
		self.window.Content.Inlines.Add( Documents.Bold( Documents.Run(text) ) )

