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
from System.Net import *
import nt

cacheHits = 0
cacheMisses = 0
totalDownloads = 0
allowCaching = False
cache = {}

def TurnOnCache():
    global allowCaching
    allowCaching = True

def TurnOffCache():
    global allowCaching
    allowCaching = False

def CacheStats():
    print "Tiles in Cache:", len(cache)
    print "Total Downloads:", totalDownloads
    print "Cache Hits:", cacheHits
    print "Cache Misses:", cacheMisses

def LoadCache():
    try:
        files = nt.listdir(nt.getcwd() + "\\cache")
    except:
        print "Error reading cache folder"
        return

    for f in files:
        if f.endswith(".jpeg"):
            cache[f[:-5]] = True

def ClearCache():
    try:
        files = nt.listdir(nt.getcwd() + "\\cache")
    except:
        print "Error reading cache folder"
        return
        
    for i in range(0, len(files)):
        if files[i].find(".jpeg") > -1:
            try:
                nt.remove(nt.getcwd() + "\\cache\\" + files[i])
            except:
                x = None

def GetImgUrl(type, quadkey):
    url = "http://" + type + quadkey[-1] + ".ortho.tiles.virtualearth.net/tiles/"
    url += type + quadkey + "."
    if (type == 'r'):
        url += "png"
    else:
        url += "jpeg"
    url += "?g=22"
    return url

def GetStream(type, quadkey):
    return WebClient().OpenRead(GetImgUrl(type, quadkey))

def GetImageFromStream(type, quadkey):
    try:
        img = Image.FromStream(GetStream(type, quadkey))
        global totalDownloads
        totalDownloads += 1
        return img
    except:
        print "Error connecting to tile server"
        return None

def GetImage(type, quadkey):
    global cacheHits
    global cacheMisses
    global cache
    global allowCaching
    
    if allowCaching is True:
        try:
            cache[type + quadkey]
            cacheHits += 1
            return Image.FromFile(nt.getcwd() + "\\cache\\" + type + quadkey + ".jpeg")
        except (KeyError, IOError):
            cacheMisses += 1
            img = GetImageFromStream(type, quadkey)
            if not img is None:        
                try:
                    img.Save(nt.getcwd() + "\\cache\\" + type + quadkey + ".jpeg")
                    cache[type + quadkey] = True
                    return img
                except:
                    print "Error writing cache"
                    return img
    
    else:
        return GetImageFromStream(type, quadkey)

# tile used for puzzle pieces in Play panel
class Tile(PictureBox):

    def __init__(self, i, j, size, type = None, quadkey = None):
        self.correctRow = self.currentRow = i
        self.correctCol = self.currentCol = j
        self.Size = size
        self.BackColor = Color.Coral
        self.beingDragged = False
        self.SizeMode = PictureBoxSizeMode.StretchImage
        if not quadkey is None:
            self.Image = GetImage(type, quadkey)
        self.initialClick = None
        self.direction = None
        
    def isCorrect(self):
        if self.correctRow != self.currentRow or self.correctCol != self.currentCol:
            return False
        return True

# tile used for puzzle pieces in Create panel
class RichTile(PictureBox):

    def __init__(self, tileDim):
        
        self.BackColor = Color.Gray
        self.Size = Size(tileDim, tileDim)
        self.SizeMode = PictureBoxSizeMode.StretchImage
        
        self.quadkey = "None"
        self.tile = None