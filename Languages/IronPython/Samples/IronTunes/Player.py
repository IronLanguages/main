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

clr.AddReferenceToFile("QuartzTypeLib.dll")

import QuartzTypeLib
import System.Math

from System.ComponentModel import INotifyPropertyChanged
from System.ComponentModel import PropertyChangedEventArgs

#define for window message to tell us there are filter graph events
WM_GRAPHNOTIFY = 0x8001

#event code that indicates the file finished playing
EC_COMPLETE = 1

class Player(INotifyPropertyChanged):
    "class that handles the playing operations"
    
    def __init__(self,app,dataModel):
        self.graphManager = QuartzTypeLib.FilgraphManagerClass()
        self.__filename = ""
        self.__app = app
        self.__selectedItem = None
        self.__playingItem = None
        self.__dataModel = dataModel
        self.__volume = 100
        self._events = []
    
    def add_PropertyChanged(self, value):
        self._events.append(value)

    def remove_PropertyChanged(self, value):
        self._events.remove(value)

    def NotifyPropertyChanged(self, info):
        for x in self._events:
            x(self,PropertyChangedEventArgs(info))
        
    def setupEvents(self):
        if (hasattr(self.__dataModel,"windowHandle")):
            try:
                QIHelper.SetNotifyWindowForMe(self.graphManager,self.__dataModel.windowHandle.ToInt32(),WM_GRAPHNOTIFY,0)
            except:
                pass
    
    def LoadFile(self, filename):
        self.graphManager = QuartzTypeLib.FilgraphManagerClass()
        self.graphManager.RenderFile(filename)
        self.setVolume(self.__volume) #we need to reset the volume in to the new graph manager
        self.setupEvents()
        self.filename = filename
    
    def Play(self):
        self.graphManager.Run()
    
    def Pause(self):
        self.graphManager.Pause()
        
    def Stop(self):
        self.graphManager.Stop()
        
    def getPosition(self):
        return self.graphManager.CurrentPosition
        
    def setPosition(self, newPosition):
        self.graphManager.CurrentPosition = newPosition

        
    def PlayPause(self):
        state = self.graphManager.GetState(10)
        if (state == 2): # playing
            self.Pause()
        elif (state == 1): # paused
            self.Play()
        elif (state == 0): # stopped
            if ((self.SelectedItem != None) and (self.filename != self.SelectedItem.Path)):
                self.Stop()
                self.PlayingItem = self.SelectedItem
                self.LoadFile(self.SelectedItem.Path)
                self.Play()
            else:
                self.Play()
        else:
            pass  #for now just do nothing
        self.NotifyPropertyChanged("IsPlaying")
        self.NotifyPropertyChanged("Duration")

    def ProcessEvents(self):
        while (True):
            eventCode,param1,param2 = self.graphManager.GetEvent(10) #raises exception when no events are left
            if (eventCode == EC_COMPLETE):
                self.PlayingIndex += 1
            
    def Tick(self):
        self.NotifyPropertyChanged("Position")
        
    def LibraryMouseDown(self,eventargs):
        if (eventargs.ClickCount == 2):
            if (self.selectedItem != None):
                self.Stop()
                self.RenderFile(self.selectedItem.Path)
                self.Play()
    
    def getFilename(self):
        return self.__filename
        
    def setFilename(self, value):
        self.__filename = value
        self.NotifyPropertyChanged("filename")
        
    def getVolume(self):
        return self.__volume
        
    def setVolume(self,value):
        self.__volume = value
        newVal = int((value - 100.0)*25) #clipping the bottom 3/4 of the range since it's basically unhearable
        if (newVal < -2499):
            newVal = -10000
        self.graphManager.Volume = newVal
        self.NotifyPropertyChanged("Volume")
        
    def getSelectedItem(self):
        return self.__selectedItem
        
    def setSelectedItem(self,value):
        self.__selectedItem = value
        self.NotifyPropertyChanged("SelectedItem")
        
    def getSelectedIndex(self):
        return self.__dataModel.library.ViewIndexOf(self.SelectedItem)
        
    def setSelectedIndex(self,value):
        self.SelectedItem = self.__dataModel.library.ViewGetValue(value)
        self.NotifyPropertyChanged("SelectedIndex")
        
    def setPlayingItem(self,value):
        if value:
            self.Stop()
            self.__playingItem = value
            self.LoadFile(value.Path)
            self.Play()
            self.SelectedItem = self.PlayingItem
            self.NotifyPropertyChanged("PlayingItem")
        
    def getPlayingItem(self):
        return self.__playingItem
    
    def setPlayingIndex(self,value):
        self.PlayingItem = self.__dataModel.library.ViewGetValue(value)
        self.NotifyPropertyChanged("PlayingIndex")
        
    def getPlayingIndex(self):
        return self.__dataModel.library.ViewIndexOf(self.PlayingItem)
        
    def isPlaying(self):
        return (self.graphManager.GetState(10) == 2)
        
    def getDuration(self):
        try:
            return self.graphManager.StopTime
        except:
            return 0
        
Player.filename = property(Player.getFilename, Player.setFilename)
Player.volume = property(Player.getVolume, Player.setVolume)
Player.SelectedItem = property(Player.getSelectedItem, Player.setSelectedItem)
Player.SelectedIndex = property(Player.getSelectedIndex, Player.setSelectedIndex)
Player.PlayingItem = property(Player.getPlayingItem, Player.setPlayingItem)
Player.PlayingIndex = property(Player.getPlayingIndex, Player.setPlayingIndex)
Player.IsPlaying = property(Player.isPlaying)
Player.Duration = property(Player.getDuration)
Player.Position = property(Player.getPosition,Player.setPosition)