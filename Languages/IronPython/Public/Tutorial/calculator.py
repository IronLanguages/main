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

from System.Windows.Controls import *

def Walk(tree):
    yield tree
    if hasattr(tree, 'Children'):
        for child in tree.Children:
            for x in Walk(child):
                yield x
    elif hasattr(tree, 'Child'):
        for x in Walk(tree.Child):
            yield x
    elif hasattr(tree, 'Content'):
        for x in Walk(tree.Content):
            yield x

def enliven(w):
    try:
        controls = [ n for n in Walk(w) if isinstance(n, Button) or isinstance(n, TextBox) ]
        Calculator(controls)
    except:
        print "Function failed. Did you pass in the Calculator window?"

class Calculator:

    def __init__(self, controls):
        self.expression = ""

        for i in controls:
            if isinstance(i, Button):
                if hasattr(self, "on_" + i.Name):
                    print "Registering self.on_" + i.Name + " to handle " + i.Name + ".Click"
                    i.Click += getattr(self, "on_" + i.Name)
            elif isinstance(i, TextBox):
                if i.Name == "Result":
                    self.result = i
        self.result.Text = self.expression
        

    def on_Button(self, c):
        self.expression += c
        self.result.Text = self.expression

    def on_Clear(self, b, e):
        self.expression = ""
        self.result.Text = self.expression

    def on_Equals(self, b, e):
        try:
            result = str(eval(self.expression))
            self.result.Text = result
            self.expression = result
        except:
            self.result.Text = "<<ERROR>>"
            self.expression = ""

    def on_One(self, b, e):
        self.on_Button('1')
    def on_Nine(self, b, e):
        self.on_Button('9')
    def on_Eight(self, b, e):
        self.on_Button('8')
    def on_Five(self, b, e):
        self.on_Button('5')
    def on_Four(self, b, e):
        self.on_Button('4')
    def on_Two(self, b, e):
        self.on_Button('2')
    def on_Three(self, b, e):
        self.on_Button('3')
    def on_Six(self, b, e):
        self.on_Button('6')
    def on_Multiply(self, b, e):
        self.on_Button('*')
    def on_Seven(self, b, e):
        self.on_Button('7')
    def on_Subtract(self, b, e):
        self.on_Button('-')
    def on_Zero(self, b, e):
        self.on_Button('0')
    def on_DecimalPoint(self, b, e):
        self.on_Button('.')
    def on_Plus(self, b, e):
        self.on_Button('+')
    def on_Divide(self, b, e):
        self.on_Button('/')
