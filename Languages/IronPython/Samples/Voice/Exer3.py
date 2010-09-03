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
clr.AddReference("System.Speech")
from System.Speech import *
from System.Diagnostics import *
from System.Collections import *

Programs = Hashtable()
Programs["Word"] = lambda : Process.Start("winword")
Programs["Excel"] = lambda : Process.Start("excel")
Programs["Powerpoint"] = lambda : Process.Start("powerpnt")
Programs["Notepad"] = lambda : Process.Start("notepad")
Programs["Internet"] = lambda : Process.Start("iexplore")
Programs["Program"] = lambda : Process.Start("devenv")

def recognized(o, eventArgs):
	if Programs.Contains(eventArgs.Result.Text):
		Programs[eventArgs.Result.Text]()
		synthesizer.Speak(eventArgs.Result.Text)
	

class VoiceRecognizer (Recognition.SpeechRecognizer):
	def __init__(self):
		self.choices = Recognition.Choices()
		for x in Programs.Keys:
			self.choices.Add(x)
		self.builder = Recognition.GrammarBuilder()
		self.builder.Append(self.choices)
		self.myGrammar = Recognition.Grammar(self.builder)
		self.myGrammar.SpeechRecognized += recognized
		self.LoadGrammar(self.myGrammar)
	

synthesizer = Synthesis.SpeechSynthesizer()
recognizer = VoiceRecognizer()
System.Console.WriteLine("Press enter to exit...")
System.Console.ReadLine()