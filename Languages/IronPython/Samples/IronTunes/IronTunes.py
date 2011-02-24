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

clr.AddReferenceByPartialName("PresentationCore")
clr.AddReferenceByPartialName("PresentationFramework")
clr.AddReferenceByPartialName("WindowsBase")

from Player import *
from avalon import *
from IronTunesWindow import *
from Library import *

from System.ComponentModel import INotifyPropertyChanged
from System.ComponentModel import PropertyChangedEventArgs

class DataModel(INotifyPropertyChanged):
    def __init__(self,app):
        self.player = Player(app,self)
        self.library = Library()
        self._events = []
    
    def add_PropertyChanged(self, value):
        self._events.append(value)

    def remove_PropertyChanged(self, value):
        self._events.remove(value)

    def NotifyPropertyChanged(self, info):
        for x in self._events:
            x(self,PropertyChangedEventArgs(info))
    
class IronTunesApp(Application): #namespace
    
    def InitializeComponent(self, skin):
        #set up the data model
        self.dataModel = DataModel(self)
        w = Window()
        w.Show()
        w.Title = "IronTunes"
        w.DataContext = self.dataModel
        w.Content = LoadXaml(skin)
        #w.SizeToContent = SizeToContent.WidthAndHeight
        self.mywindow = IronTunesWindow(self.dataModel)
        self.mywindow.enliven(w)
        
    @staticmethod
    def RealEntryPoint(skin):
        app = IronTunesApp()
        app.InitializeComponent(skin)
        app.Run()

def GetSkin():
    import sys
    if len(sys.argv) > 1:
        skin = sys.argv[1]
        if skin.endswith(".xaml"): return skin
    return "CurrentSkin.xaml"

if __name__ == "__main__":
    IronTunesApp.RealEntryPoint(GetSkin())
