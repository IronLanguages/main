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

def recognized(o, eventArgs):
	System.Console.WriteLine(eventArgs.Result.Text)

recognizer = Recognition.SpeechRecognizer()
choices = Recognition.Choices()
choices.Add("Sam")
choices.Add("Mary")
choices.Add("Bob")
builder = Recognition.GrammarBuilder()
builder.Append(choices)

myGrammar = Recognition.Grammar(builder)
myGrammar.SpeechRecognized += recognized
recognizer.LoadGrammar(myGrammar)
System.Console.ReadLine()