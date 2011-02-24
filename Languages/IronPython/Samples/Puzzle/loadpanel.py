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

from System.Windows.Forms import *
from System.Drawing import *

class SavedGameButton(Button):
    
    def __init__(self, caption, y, x, type, level, dimension):
        
        self.index = -1
        
        self.BackColor = Color.White
        self.Size = Size(250, 55)
        self.Font = Font("Verdana", 8)
        self.FlatStyle = FlatStyle.Flat
        _type = ""
        if type == "a":    _type = "Aerial"
        elif type == "h": _type = "Hybrid"
        elif type == "r": _type = "Road"
        self.Text = caption
        self.Text += "\n(" + str(x) + ", " + str(y) + ")"
        self.Text += "\n" + _type + " - Zoom Level " + str(level) + " - " + str(dimension) + "x" + str(dimension)

class LoadGamePanel(Panel):

    def __init__(self):
        self.AutoScroll = True
        self.Size = Size(270, 280)
        self.buttons = []
        self.numButtons = 0
        self.selectedIndex = -1
        
    def AddButton(self, b):
        b.index = self.numButtons
        b.Location = Point(0, self.numButtons * b.Size.Height + self.numButtons * 3)
        self.numButtons += 1
        b.Click += self.buttonClick
        self.buttons += [b]
        self.Controls.Add(b)
        
    def buttonClick(self, sender, e):
        for b in self.buttons:
            b.BackColor = Color.White
        sender.BackColor = Color.LemonChiffon
        self.selectedIndex = sender.index

def LoadAllSavedSettings():
    import clr
    clr.AddReference("System.Xml")
    from System.Xml import *
    import nt
    
    games = []
    state = []
    allowCaching = False
    
    try:
        reader = XmlTextReader(nt.getcwd() + "\load.xml")
        while reader.Read() is True:
            if reader.NodeType == XmlNodeType.Element:
            
                if reader.Name == "Game":
                    game = []
                    game += [reader.GetAttribute("caption")]
                    game += [reader.GetAttribute("type")]
                    game += [int(reader.GetAttribute("y"))]
                    game += [int(reader.GetAttribute("x"))]
                    game += [int(reader.GetAttribute("level"))]
                    game += [int(reader.GetAttribute("dimension"))]
                    games += [game]
                
                elif reader.Name == "TopLeftPreviewTile":
                    state += [int(reader.GetAttribute("x"))]
                    state += [int(reader.GetAttribute("y"))]
                    state += [int(reader.GetAttribute("dimension"))]
                    state += [int(reader.GetAttribute("level"))]
                
                elif reader.Name == "Cache":
                    if reader.GetAttribute("allow").lower() == "true":
                        allowCaching = True
    except:
        print "Error reading load.xml"
        return [[], [327, 714, 3, 11], True]
    
    return [games, state, allowCaching]