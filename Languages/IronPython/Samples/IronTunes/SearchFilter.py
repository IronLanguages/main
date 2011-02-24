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

import System
import System.Collections
from System.ComponentModel import INotifyPropertyChanged
from System.ComponentModel import PropertyChangedEventArgs

class SearchFilter(INotifyPropertyChanged):
    "class that implements the library view filter"
    
    def __init__(self,library):
        self.__searchString = ""
        self.__sortField = ""
        self.__library = library
        self._events = []
    
    def add_PropertyChanged(self, value):
        self._events.append(value)

    def remove_PropertyChanged(self, value):
        self._events.remove(value)

    def NotifyPropertyChanged(self, info):
        for x in self._events:
            x(self,PropertyChangedEventArgs(info))
    
    def getSearchString(self):
        return self.__searchString
        
    def setSearchString(self,value):
        self.__searchString = value
        self.NotifyPropertyChanged("SearchString")
        self.__library.updateLibraryView()
        
    def getSortField(self):
        return self.__sortField
        
    def setSortField(self,value):
        self.__sortField = value
        self.NotifyPropertyChanged("SortField")
        self.__library.updateLibraryView()
        
    def applyFilterAndSort(self, library):
        filteredLibrary = System.Collections.ArrayList()

        for i in library:
            if ((self.__searchString == "") or (i.matches(self.__searchString))):
                filteredLibrary.Add(i)
                
        if (self.__sortField != ""):
            filteredLibrary.Sort(SongComparer(self.__sortField))
            
        return filteredLibrary

class SongComparer(System.Collections.IComparer):
    def __init__(self,field):
        self.__field = field
        self.__comparer = CaseInsensitiveComparer()
    
    def Compare(x, y):
        if (hasattr(x,self.__field) and hasattr(y,self.__field)):
            return self.__comparer.Compare(getattr(x,self.__field),getattr(y,self.__field))
            
SearchFilter.SearchString = property(SearchFilter.getSearchString, SearchFilter.setSearchString)
SearchFilter.SortField = property(SearchFilter.getSortField, SearchFilter.setSortField)