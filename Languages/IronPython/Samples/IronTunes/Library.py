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

from SearchFilter import *
from System.IO import Path
from System.ComponentModel import PropertyChangedEventArgs

class Library(INotifyPropertyChanged):
    "class that represents the user's library"
    
    class Song:
        def __init__(self,path):
            self.Path = path
            self.Title = Path.GetFileName(path)
        def matches(self,string):
            strU = string.ToUpper()
            return self.Title.ToUpper().Contains(strU)
    
    def __init__(self):
        self.__library = System.Collections.ArrayList()
        self.__libraryView = System.Collections.ArrayList()
        self.SearchFilter = SearchFilter(self)
        self._events = []
    
    def add_PropertyChanged(self, value):
        self._events.append(value)

    def remove_PropertyChanged(self, value):
        self._events.remove(value)

    def NotifyPropertyChanged(self, info):
        for x in self._events:
            x(self,PropertyChangedEventArgs(info))
    
    def AddDir(self,StartDir):
        AllDirs = System.IO.Directory.GetDirectories(StartDir)
        
        try:
            for dir in AllDirs:
                self.AddDir(dir)
        except Exception:
            pass
            
        for extension in ["*.mp3","*.wma"]:
            AllSongs = System.IO.Directory.GetFiles(StartDir, extension)
            for a in AllSongs:
                self.AddFile(a)
        
    def AddFile(self,file):
        song = Library.Song(file)
        self.__library.Add(song)
        self.updateLibraryView()
        
    def updateLibraryView(self):
        self.__libraryView = self.SearchFilter.applyFilterAndSort(self.__library)
        self.NotifyPropertyChanged("LibraryView")
    
    def getLibraryView(self):
        return self.__libraryView
        
    def ViewIndexOf(self,value):
        return self.__libraryView.IndexOf(value)
        
    def ViewGetValue(self,index):
        if ((index >= 0) and (index < self.__libraryView.Count)):
            return self.__libraryView[index]
        else:
            return None
        
Library.LibraryView = property(Library.getLibraryView)