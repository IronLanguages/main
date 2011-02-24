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
clr.AddReference("System.Speech")
from System.Windows.Forms import *
from System.Drawing import *
from System.Speech import *
from System.Threading import *
from System.Collections import *
from System.IO import *

# Config Options

commands = Hashtable()
commands["Back"] = lambda x: x.browser.GoBack()
commands["Forward"] = lambda x: x.browser.GoForward()
commands["Home"] = lambda x: x.browser.GoHome()
commands["Refresh"] = lambda x: x.browser.Refresh()
commands["Stop"] = lambda x: x.browser.Stop()
commands["Maximize"] = lambda x: x.Maximize()
commands["Minimize"] = lambda x: x.Minimize()
commands["Exit"] = lambda x: x.Close()
commands["Quit"] = lambda x: x.Close()
commands["Close"] = lambda x: x.Close()
commands["Mark"] = lambda x: x.storeFavs()

curPage = Hashtable()
t = StreamReader("favs.txt").Close()

# Class definitions

class WebBrowserForm (Form):
	def __init__(self):
		self.Text = "My Web Browser"
		self.AutoScroll, self.AutoSize = True, True
		self.MinimumSize = Size(40,40)
		self.Size = Size(500,500)
		self.browser = WebBrowser(Dock = DockStyle.Fill, MinimumSize = Size(40,40), Url = System.Uri("http://www.msn.com/"))
		self.Controls.Add(self.browser)
	
	def Start(self):
		self.ShowDialog()
		
	def Maximize(self):
		if self.WindowState == FormWindowState.Maximized:
			self.WindowState = FormWindowState.Normal
		else:
			self.WindowState = FormWindowState.Maximized
		
	def Minimize(self):
		if self.WindowState == FormWindowState.Minimized:
			self.WindowState = FormWindowState.Normal
		else:
			self.WindowState = FormWindowState.Minimized
		
	def setUrl(self, addr):
		self.browser.Navigate(System.Uri(addr))
	
	def parseFavs(self):
		t = StreamReader("favs.txt")
		a = t.ReadLine()
		while a != "End":
			t1 = t.ReadLine()
			curPage[a] = t1
			a = t.ReadLine()
		
		t.Close()
	
	def storeFavs(self):
		t = StreamWriter("favs.txt", append = True)
		t.WriteLine(self.browser.Url)
		t.Close()
	

class VoiceRecognizer (Recognition.SpeechRecognizer):
	def __init__(self):
		self.choices = Recognition.Choices()
		for x in commands.Keys:
			self.choices.Add(x)
		for x in curPage.Keys:
			self.choices.Add(x)
		self.builder = Recognition.GrammarBuilder()
		self.builder.Append(self.choices)
		self.myGrammar = Recognition.Grammar(self.builder)
		self.myGrammar.SpeechRecognized += recognized
		self.LoadGrammar(self.myGrammar)
	

def recognized(o, eventArgs):
	line = eventArgs.Result.Text
	System.Console.WriteLine(line)
	if commands.Contains(line) == True:
		x = commands[line]
		x(App)
	elif curPage.Contains(line) == True:
		App.setUrl(curPage[line].ToString())
	else: #will never reach this point
		System.Console.WriteLine("Unrecognized")
	

# Main Code

App = WebBrowserForm()
App.parseFavs()
Voice = VoiceRecognizer()
App.Start()
